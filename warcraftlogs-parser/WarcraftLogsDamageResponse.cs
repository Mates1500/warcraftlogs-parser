using System.Collections.Generic;
using System.Linq;

namespace warcraftlogs_parser
{
    public class WarcraftLogsDamageResponse
    {
        public enum TableDataType
        {
            DamageDone,
            Healing
        }

        public class ReportDataC
        {
            public Report Report { get; set; }
        }

        public class Report
        {
            public Table Table { get; set; }
        }

        public class Table
        {
            public InnerData Data { get; set; }
        }

        public class InnerData
        {
            public List<DataEntry> Entries { get; set; }
        }

        public class GearEntry
        {
            public int Id { get; set; }
            public int Slot { get; set; }
            public int Quality { get; set; }
            public string Icon { get; set; }
            public string Name { get; set; }
            public int ItemLevel { get; set; }
            public int PermanentEnchant { get; set; }
            public string PermanentEnchantName { get; set; }
        }

        public class AbilityEntry
        {
            public string Name { get; set; }
            public int Total { get; set; }
            public int TotalReduced { get; set; }
            public int Type { get; set; }
        }

        public class DataEntry
        {
            public string Name { get; set; }
            public int Id { get; set; }
            public int Guid { get; set; }
            public string Type { get; set; }
            public string Icon { get; set; }
            public int Total { get; set; }
            public int TotalReduced { get; set; }
            public int ActiveTime { get; set; }

            public int Average => Total / (ActiveTime / 1000);

            public int AverageReduced => ActiveTimeReduced == 0 ? 0 : (int)(TotalReduced / (float)(ActiveTimeReduced / 1000));
            public int ActiveTimeReduced { get; set; }
            public int Blocked { get; set; }
            public List<GearEntry> Gear { get; set; }
            public List<AbilityEntry> Abilities { get; set; }

            public int NatureProtectionTotal =>
                Abilities.Where(x => x.Name == "Nature Protection").Sum(x => x.Total);
            public int ShadowProtectionTotal =>
                Abilities.Where(x => x.Name == "Shadow Protection").Sum(x => x.Total);
            public int FireProtectionTotal =>
                Abilities.Where(x => x.Name == "Fire Protection").Sum(x => x.Total);
            public int FrostProtectionTotal =>
                Abilities.Where(x => x.Name == "Frost Protection").Sum(x => x.Total);
            public int HealingPotionTotal =>
                Abilities.Where(x => x.Name == "Healing Potion").Sum(x => x.Total);

            public string[] ToExcelForm => new[]
            {
                Name, Icon, Average.ToString(), AverageReduced.ToString(), ActiveTime.ToString(), ActiveTimeReduced.ToString(), Total.ToString(), TotalReduced.ToString(),
                NatureProtectionTotal.ToString(), ShadowProtectionTotal.ToString(), FireProtectionTotal.ToString(), FrostProtectionTotal.ToString(), HealingPotionTotal.ToString()
            };

        }

        public ReportDataC ReportData { get; set; }
    }
}