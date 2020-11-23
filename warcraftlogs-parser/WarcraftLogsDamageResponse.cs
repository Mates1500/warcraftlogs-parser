using System.Collections.Generic;

namespace warcraftlogs_parser
{
    public class WarcraftLogsDamageResponse
    {
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

        }

        public ReportDataC ReportData { get; set; }
    }
}