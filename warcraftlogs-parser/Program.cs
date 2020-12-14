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

        static async Task Main(string[] args)
        {
            try
            {
                var combatId = InputParsing.GetCombatIdFromArgs(args);

                var config = BuildConfig();

                var bearerToken = AuthenticationHelper.GetBearerToken(config["client:id"], config["client:secret"]).Result;

                var damageResponse = await WarcraftLogsResponseProcessing.GetResponse(bearerToken, combatId, WarcraftLogsDamageResponse.TableDataType.DamageDone);
                var healingResponse = await WarcraftLogsResponseProcessing.GetResponse(bearerToken, combatId, WarcraftLogsDamageResponse.TableDataType.Healing);

                var filteredDamageResults = WarcraftLogsResponseProcessing.FilterPlayers(damageResponse);

                var playerOnlyHealingResults = WarcraftLogsResponseProcessing.FilterPlayers(healingResponse);
                var healerOnlyHealingResults = WarcraftLogsResponseProcessing.FilterHealers(playerOnlyHealingResults);


                var orderedDamageResults = filteredDamageResults.OrderByDescending(x => x.Average).ThenBy(x => x.Icon);
                var orderedHealingOnlyResults =
                    healerOnlyHealingResults.OrderByDescending(x => x.TotalReduced).ThenBy(x => x.Icon);
                var orderedHealingAllResults =
                    healerOnlyHealingResults.OrderByDescending(x => x.Total).ThenBy(x => x.Icon);

                var outputFileName = Path.Join(config["defaultSaveDirectory"], $"{DateTime.Now:yyyy-MM-ddTHH_mm_ss}.csv");

                OutputProcessing.CreateCsvFile(orderedDamageResults.ToList(), orderedHealingOnlyResults.ToList(),
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
