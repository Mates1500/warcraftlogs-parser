using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Reflection;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using GraphQL;
using GraphQL.Client.Http;
using GraphQL.Client.Serializer.SystemTextJson;
using Microsoft.Extensions.Configuration;

namespace warcraftlogs_parser
{
    class Program
    {
        private static IConfiguration BuildConfig()
        {
            var currentAssembly = Assembly.GetEntryAssembly();
            if (currentAssembly is null)
                throw new NullReferenceException("This should absolutely never happen.");

            return new ConfigurationBuilder().SetBasePath(Path.GetDirectoryName(currentAssembly.Location))
                //.AddJsonFile("config.json")
                .AddJsonFile("config.secret.json")
                .Build();
        }

        private static async Task<string> GetBearerToken(string userId, string userSecret)
        {
            using (var client = new HttpClient())
            {
                var authString = Convert.ToBase64String(Encoding.UTF8.GetBytes($"{userId}:{userSecret}"));
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", authString);

                var formContent = new List<KeyValuePair<string, string>>
                {
                    new KeyValuePair<string, string>("grant_type", "client_credentials")
                };
                var httpContent = new FormUrlEncodedContent(formContent);

                var response = await client.PostAsync("https://www.warcraftlogs.com/oauth/token", httpContent);
                var responseString = await response.Content.ReadAsStringAsync();
                var parsedResponse = JsonSerializer.Deserialize<AuthorizationResponse>(responseString);

                return parsedResponse.AccessToken;
            }
        }

        private static async Task<WarcraftLogsDamageResponse> GetResponse(string bearerToken)
        {
            using (var graphQLClient = new GraphQLHttpClient("https://classic.warcraftlogs.com/api/v2/client",
                new SystemTextJsonSerializer()))
            {
                graphQLClient.HttpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {bearerToken}");

                var tryhardRequest = new GraphQLRequest
                {
                    Query = @"query($combatId: String){
                            reportData{
                                report(code: $combatId)
                                {
                                    table(dataType: DamageDone, startTime: 0, endTime: 1603992250)
                                }
                            }
                        }",
                    Variables = new
                    {
                        combatId = "8nXGbj1FN4RWVBMa"
                    }
                };

                var response = await graphQLClient.SendQueryAsync<WarcraftLogsDamageResponse>(tryhardRequest);
                return response.Data;
            }
        }


        static async Task Main(string[] args)
        {
            var config = BuildConfig();

            var bearerToken = GetBearerToken(config["client:id"], config["client:secret"]).Result;

            var response = await GetResponse(bearerToken);

            Console.WriteLine("Hello World!");
            Console.ReadLine();

        }
    }
}
