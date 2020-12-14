using System.Collections.Generic;
using System.IO;

namespace warcraftlogs_parser
{
    public static class OutputProcessing
    {
        public static void CreateCsvFile(List<WarcraftLogsDamageResponse.DataEntry> damageResults,
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
    }
}