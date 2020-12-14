using System.Collections.Generic;
using System.IO;
using OfficeOpenXml;

namespace warcraftlogs_parser
{
    public static class OutputProcessing
    {
        public class OutputTask
        {
            public string Title { get; }

            public List<WarcraftLogsDamageResponse.DataEntry> Results { get; }

            public OutputTask(string title, List<WarcraftLogsDamageResponse.DataEntry> results)
            {
                Title = title;
                Results = results;
            }
        }

        public static void CreateXlsxFile(List<OutputTask> outputTasks, string filename)
        {
            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;

            var colNames = new[] { "Name", "Class/Spec", "Avg", "AvgReduced", "TotalTime", "TotalTimeReduced", "TotalDamage", "TotalDamageReduced", "NatureProtection", "ShadowProtection", "FireProtection", "FrostProtection", "HealingPotion" };
            using (var package = new ExcelPackage())
            {
                
                for (var i = 0; i < outputTasks.Count; ++i)
                {
                    var worksheet = package.Workbook.Worksheets.Add(outputTasks[i].Title);

                    // first row
                    for (var j = 0; j < colNames.Length; ++j)
                    {
                        worksheet.Cells[1, j+1].Value = colNames[j];
                    }


                    var rowIndex = 2; // we start at second line
                    foreach (var r in outputTasks[i].Results)
                    {
                        var excelLine = r.ToExcelForm;
                        for (var j = 0; j < excelLine.Length; ++j)
                        {
                            if (int.TryParse(excelLine[j], out var numValue))
                                worksheet.Cells[rowIndex, j + 1].Value = numValue;
                            else
                                worksheet.Cells[rowIndex, j + 1].Value = excelLine[j];
                        }

                        ++rowIndex;
                    }

                    worksheet.Cells[1, 1, rowIndex, colNames.Length].AutoFilter = true;
                    worksheet.Cells.AutoFitColumns();
                }

                package.SaveAs(new FileInfo(filename));
            }
        }
    }
}