using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
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
                    playerOnlyHealingResults.OrderByDescending(x => x.Total).ThenBy(x => x.Icon);

                var outputTasks = new List<OutputProcessing.OutputTask>
                {
                    new OutputProcessing.OutputTask("DamageDone", orderedDamageResults.ToList()),
                    new OutputProcessing.OutputTask("HealingOnly", orderedHealingOnlyResults.ToList()),
                    new OutputProcessing.OutputTask("HealingAll", orderedHealingAllResults.ToList())
                };

                var outputFileName = Path.Join(config["defaultSaveDirectory"], $"{DateTime.Now:yyyy-MM-ddTHH_mm_ss}.xlsx");

                OutputProcessing.CreateXlsxFile(outputTasks, outputFileName);

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
