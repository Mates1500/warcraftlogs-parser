using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GraphQL;
using GraphQL.Client.Http;
using GraphQL.Client.Serializer.SystemTextJson;

namespace warcraftlogs_parser
{
    public static class WarcraftLogsResponseProcessing
    {
        private const string WARCRAFTLOGS_GRAPHQL_ENDPOINT = "https://classic.warcraftlogs.com/api/v2/client";

        public static async Task<WarcraftLogsDamageResponse> GetResponse(string bearerToken, string combatEncounterId, WarcraftLogsDamageResponse.TableDataType tdt)
        {
            using (var graphQLClient = new GraphQLHttpClient(WARCRAFTLOGS_GRAPHQL_ENDPOINT,
                new SystemTextJsonSerializer()))
            {
                graphQLClient.HttpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {bearerToken}");

                var dpsChartRequest = new GraphQLRequest
                {
                    Query = @"query($combatId: String, $tdt: TableDataType, $timestampEnd: Float){
                            reportData{
                                report(code: $combatId)
                                {
                                    table(dataType: $tdt, startTime: 0, endTime: $timestampEnd)
                                }
                            }
                        }",
                    Variables = new
                    {
                        combatId = combatEncounterId,
                        tdt = tdt.ToString(),
                        timestampEnd = (float)DateTimeOffset.UtcNow.ToUnixTimeSeconds()
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
    }
}