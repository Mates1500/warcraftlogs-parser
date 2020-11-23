using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Reflection;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
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
                .AddJsonFile("config.json")
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
                if(response.StatusCode != HttpStatusCode.OK)
                    throw new ArgumentException("Username/Secret did not authenticate properly, please make sure your credentials are valid");

                var responseString = await response.Content.ReadAsStringAsync();
                var parsedResponse = JsonSerializer.Deserialize<AuthorizationResponse>(responseString);

                return parsedResponse.AccessToken;
            }
        }

        public enum TableDataType
        {
            DamageDone,
            Healing
        }

        private static async Task<WarcraftLogsDamageResponse> GetResponse(string bearerToken, string combatEncounterId, TableDataType tdt)
        {
            using (var graphQLClient = new GraphQLHttpClient("https://classic.warcraftlogs.com/api/v2/client",
                new SystemTextJsonSerializer()))
            {
                graphQLClient.HttpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {bearerToken}");

                var dpsChartRequest = new GraphQLRequest
                {
                    Query = @"query($combatId: String, $tdt: TableDataType){
                            reportData{
                                report(code: $combatId)
                                {
                                    table(dataType: $tdt, startTime: 0, endTime: 1603992250)
                                }
                            }
                        }",
                    Variables = new
                    {
                        combatId = combatEncounterId,
                        tdt = tdt.ToString()
                    }
                };

                var response = await graphQLClient.SendQueryAsync<WarcraftLogsDamageResponse>(dpsChartRequest);
                return response.Data;
            }
        }

        public static List<WarcraftLogsDamageResponse.DataEntry> FilterPlayers(WarcraftLogsDamageResponse response)
        {
            return response.ReportData.Report.Table.Data.Entries
                .Where(x => x.Type != "Unknown" && x.ActiveTime != 0 && x.Gear.Count != 0)
                .ToList();
        }

        public static List<WarcraftLogsDamageResponse.DataEntry> FilterHealers(
            List<WarcraftLogsDamageResponse.DataEntry> healingList)
        {
            var totalHealingDone = healingList.Sum(x => x.Total);
            var validHealers = healingList.Where(x => x.Total >= (totalHealingDone / 100 * 2));
            return validHealers.ToList();
        }

        private static void CreateCsvFile(List<WarcraftLogsDamageResponse.DataEntry> damageResults,
            List<WarcraftLogsDamageResponse.DataEntry> healingOnlyResults,
            List<WarcraftLogsDamageResponse.DataEntry> healingAllResults, string filename)
        {
            using (var f = new StreamWriter(filename, false))
            {
                var firstLine =
                    "Name, Class/Spec, Avg, AvgReduced, TotalTime, TotalTimeReduced, TotalDamage, TotalDamageReduced";
                f.WriteLine(firstLine);

                foreach (var r in damageResults)
                {
                    var currentLine =
                        $"{r.Name}, {r.Icon}, {r.Average}, {r.AverageReduced}, {r.ActiveTime}, {r.ActiveTimeReduced}, {r.Total}, {r.TotalReduced}";
                    f.WriteLine(currentLine);
                }

                f.WriteLine("");
                f.WriteLine("HEALING EXCLUDING ABSORBS/POTIONS ETC");
                f.WriteLine("");
                foreach (var r in healingOnlyResults)
                {
                    var currentLine =
                        $"{r.Name}, {r.Icon}, {r.Average}, {r.AverageReduced}, {r.ActiveTime}, {r.ActiveTimeReduced}, {r.Total}, {r.TotalReduced}";
                    f.WriteLine(currentLine);
                }

                f.WriteLine("");
                f.WriteLine("HEALING INCLUDING ABSORBS/POTIONS ETC");
                f.WriteLine("");
                foreach (var r in healingAllResults)
                {
                    var currentLine =
                        $"{r.Name}, {r.Icon}, {r.Average}, {r.AverageReduced}, {r.ActiveTime}, {r.ActiveTimeReduced}, {r.Total}, {r.TotalReduced}";
                    f.WriteLine(currentLine);
                }
            }
        }

        public static string GetCombatIdFromArgs(string[] args)
        {
            if (args.Count() > 1)
                throw new ArgumentException("Expected exactly 1 argument in form of link to the combat encounter or the code itself");

            string firstArg;
            if (args.Count() == 0)
            {
                Console.WriteLine("Enter the combat code either in form of https://classic.warcraftlogs.com/reports/J1p4M8gd3b72RLGC or J1p4M8gd3b72RLGC");
                firstArg = Console.ReadLine();
            }
            else
            {
                firstArg = args[0];
            }

            if (firstArg.Length == 16) // direct combat id code
                return firstArg;

            if(!firstArg.StartsWith("https://"))
                throw new ArgumentException("Argument is not a 16-char combat code, neither a link to the combat encounter, terminating...");


            var rx = new Regex(@"reports\/(.{16})");
            var matches = rx.Matches(firstArg);

            if(matches.Count != 1)
                throw new ArgumentException($"Could not parse value {firstArg} with {matches.Count} matches, exact count of 1 matches expected!");

            return matches[0].Groups[1].Value;

        }


        static async Task Main(string[] args)
        {
            try
            {
                var combatId = GetCombatIdFromArgs(args);

                var config = BuildConfig();

                var bearerToken = GetBearerToken(config["client:id"], config["client:secret"]).Result;

                var damageResponse = await GetResponse(bearerToken, combatId, TableDataType.DamageDone);
                var healingResponse = await GetResponse(bearerToken, combatId, TableDataType.Healing);

                var filteredDamageResults = FilterPlayers(damageResponse);

                var playerOnlyHealingResults = FilterPlayers(healingResponse);
                var healerOnlyHealingResults = FilterHealers(playerOnlyHealingResults);


                var orderedDamageResults = filteredDamageResults.OrderByDescending(x => x.Average).ThenBy(x => x.Icon);
                var orderedHealingOnlyResults =
                    healerOnlyHealingResults.OrderByDescending(x => x.TotalReduced).ThenBy(x => x.Icon);
                var orderedHealingAllResults =
                    healerOnlyHealingResults.OrderByDescending(x => x.Total).ThenBy(x => x.Icon);

                var outputFileName = Path.Join(config["defaultSaveDirectory"], $"{DateTime.Now:yyyy-MM-ddTHH_mm_ss}.csv");

                CreateCsvFile(orderedDamageResults.ToList(), orderedHealingOnlyResults.ToList(),
                    orderedHealingAllResults.ToList(), outputFileName);

                Console.WriteLine($"Done! Saved as {outputFileName}");
                Console.WriteLine("Press any key to quit...");
                Console.ReadLine();
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                Console.ReadLine();
            }
        }
    }
}
