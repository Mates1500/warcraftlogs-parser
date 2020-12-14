using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace warcraftlogs_parser
{
    public class AuthenticationHelper
    {
        private const string WARCRAFTLOGS_OAUTH_ENDPOINT = "https://www.warcraftlogs.com/oauth/token";
        public static async Task<string> GetBearerToken(string userId, string userSecret)
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

                var response = await client.PostAsync(WARCRAFTLOGS_OAUTH_ENDPOINT, httpContent);
                if(response.StatusCode != HttpStatusCode.OK)
                    throw new ArgumentException("Username/Secret did not authenticate properly, please make sure your credentials are valid");

                var responseString = await response.Content.ReadAsStringAsync();
                var parsedResponse = JsonSerializer.Deserialize<AuthorizationResponse>(responseString);

                return parsedResponse.AccessToken;
            }
        }
    }
}