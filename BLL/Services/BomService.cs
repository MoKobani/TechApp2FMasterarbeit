using DAL.Excel;
using DocumentFormat.OpenXml.Spreadsheet;
using System;
using System.Data;

namespace BLL.Services
{
    /// <summary>
    /// Liefert Stücklisten-Vorlagen aus einer Excel-Datei.
    /// </summary>
    public class BomService
    {
        /// <summary>
        /// Liest die Vorlage für die angegebene Bauteilart aus einer Excel-Datei.
        /// Der Pfad zur Datei wird als Parameter übergeben, sodass er leicht konfigurierbar ist.
        /// </summary>
        /// <param name="parttype">Name des Tabellenblatts, das die Stückliste enthält.</param>
        /// <param name="filePath">Pfad zur Excel-Vorlage.</param>
        public DataTable GetBomTemplate(string sheetName, string filePath)
        {
            try
            {
                DataSet dataSet = ExcelConnection.ExcelToDataSet(filePath);

                DataTable? bomTable = dataSet.Tables[sheetName];
                if (bomTable == null)
                {
                    throw new InvalidOperationException($"Keine Vorlage für {sheetName} gefunden!");
                }
                return bomTable;
            }
            catch (Exception ex)
            {
                // Fehler einfach als InvalidOperationException weiterreichen
                throw new InvalidOperationException(ex.Message);
            }
        }

        public List<string> GetSheetsNames(string filePath)
        {
            try
            {
                DataSet dataSet = ExcelConnection.ExcelToDataSet(filePath);
                List<string> partTypes = new List<string>();
                foreach (DataTable table in dataSet.Tables)
                {
                    partTypes.Add(table.TableName);
                }
                return partTypes;
            }
            catch (Exception ex)
            {
                // Fehler einfach als InvalidOperationException weiterreichen
                throw new InvalidOperationException(ex.Message);

            }
          

        }
    }
}
