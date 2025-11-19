using ClosedXML.Excel;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DAL.Excel
{
    /// <summary>
    /// Hilfsklasse zum Einlesen von Excel-Dateien.
    /// </summary>
    public class ExcelConnection
    {
        /// <summary>
        /// Liest eine Excel-Datei komplett ein und gibt ein <see cref="DataSet"/> zurück,
        /// in dem jedes Arbeitsblatt als <see cref="DataTable"/> enthalten ist.
        /// </summary>
        /// <param name="filePath">Pfad zur Excel-Datei.</param>
        public static DataSet ExcelToDataSet(string filePath)
        {
            try
            {
                var dataSet = new DataSet();
                using var workBook = new XLWorkbook(filePath);
                foreach (var workSheet in workBook.Worksheets)
                {
                    var dt = new DataTable(workSheet.Name);
                    //Assumes that the first column of the excel sheet having headers
                    workSheet.FirstRowUsed()!.CellsUsed().ToList()
                        .ForEach(x =>
                        {
                            dt.Columns.Add(x.Value.ToString());
                        });

                    foreach (var row in workSheet.RowsUsed().Skip(1))
                    {
                        var dr = dt.NewRow();
                        for (var i = 0; i < dt.Columns.Count; i++)
                        {
                            dr[i] = row.Cell(i + 1).Value.ToString();
                        }
                        dt.Rows.Add(dr);
                    }
                    dataSet.Tables.Add(dt);
                }

                return dataSet;
            }
            catch (Exception ex)
            {

                throw new InvalidOperationException(ex.Message);
            }
        }

    }
}
