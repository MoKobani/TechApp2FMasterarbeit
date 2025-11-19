using DAL.SolidEdge.Data;
using System.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BLL.Models;
using BLL.Helpers;

namespace BLL.Services
{
    public class AsmService
    {
        private readonly AsmRepo _asmRepo;


        public AsmService() : this(new AsmRepo())
        {
        }

        public AsmService(AsmRepo asmRepo)
        {
            _asmRepo = asmRepo;
        }

        public AsmModel GetAsm()
        {
            var data = _asmRepo.LoadAsm();
            

            var asmModel = new AsmModel
            {
                Articlenumber = data.Articlenumber,
                Mass = data.Mass,
                Length = data.length,
                Width = data.Width,
                Height = data.Height,
                ImagePath = data.ImagePath,
                FilePath = PathConverter.ToLocalDrivePath(data.FilePath),
                JpgFilePath = PathConverter.ToLocalDrivePath(data.FilePath, ".jpg"),
                ProcurementType = data.ProcurementType,
                Occurrences = data.Occurrences,
                StorageLocation = data.StorageLocation,
                Parttype = data.Parttype,
            };

            return asmModel;

           

        }


        public DataTable ToDataTable(List<AsmOccurrence>? occurrences, DataTable? template = null)
        {
            // 1) Zieltabelle: nur Schema aus der Vorlage übernehmen (Clone) – keine Daten!
            //    Wenn keine Vorlage kommt, eigenes leeres BOM-Schema anlegen.
            DataTable table = template?.Clone() ?? CreateEmptyBomTable();

            // 2) „Abteilung“-Zeilen aus der Vorlage zuerst übernehmen (falls vorhanden)
            if (template is not null && template.Columns.Contains("Sachnummer"))
            {
                var abteilungRows = template.AsEnumerable()
                    .Where(r => string.Equals(
                        (r.Field<string>("Sachnummer") ?? string.Empty).Trim(),
                        "Abteilung",
                        StringComparison.OrdinalIgnoreCase));

                foreach (var r in abteilungRows)
                    table.ImportRow(r); // ImportRow kopiert die Datenzeile in die neue Tabelle (gleiches Schema!)
            }

            // 3) Occurrences → gruppieren, sortieren und einfügen
            if (occurrences is not null && occurrences.Count > 0)
            {
                var items = occurrences
                    .Where(p => p is not null)
                    .Select(p => new
                    {
                        ArticleNumber = BuildArticleNumber(p.OccurrenceName, p.Facestyle),
                        Part = p
                    })
                    .Where(x => !string.IsNullOrWhiteSpace(x.ArticleNumber));

                var grouped = items
                    .GroupBy(x => x.ArticleNumber, StringComparer.OrdinalIgnoreCase)
                    .Select(g => new
                    {
                        ArticleNumber = g.Key,
                        Count = g.Count(),
                        FaceStyle = (g.Select(x => x.Part.Facestyle).FirstOrDefault() ?? string.Empty).Trim(),
                        FirstOccurrence = g.Select(x => x.Part).FirstOrDefault()
                    })
                    // Sortierung wie gehabt: erst nach FaceStyle, dann nach Artikelnummer
                    .OrderBy(x => x.FaceStyle, StringComparer.OrdinalIgnoreCase)
                    .ThenBy(x => x.ArticleNumber, StringComparer.OrdinalIgnoreCase);

                foreach (var group in grouped)
                {
                    var row = table.NewRow();

                    // Sicher befüllen – SetValue prüft Spaltennamen und konvertiert Datentypen
                    SetValue(row, "Sachnummer", group.ArticleNumber);
                    //SetValue(row, "Artikelnummer", group.ArticleNumber); // wird ignoriert, falls die Spalte fehlt
                    SetValue(row, "Bezeichnung", group.FirstOccurrence?.Discription);
                    SetValue(row, "Länge", group.FirstOccurrence?.Length);
                    SetValue(row, "LEinheit", string.Empty);
                    SetValue(row, "Breite", group.FirstOccurrence?.Width);
                    SetValue(row, "BEinheit", string.Empty);
                    SetValue(row, "Menge", group.Count);
                    SetValue(row, "MEinheit", string.Empty);

                    table.Rows.Add(row);
                }
            }

            // 4) Restliche Vorlagenzeilen (ohne „Abteilung“) ans Ende anhängen
            if (template is not null)
            {
                IEnumerable<DataRow> restRows;

                if (template.Columns.Contains("Sachnummer"))
                {
                    restRows = template.AsEnumerable()
                        .Where(r => !string.Equals(
                            (r.Field<string>("Sachnummer") ?? string.Empty).Trim(),
                            "Abteilung",
                            StringComparison.OrdinalIgnoreCase));
                }
                else
                {
                    // Falls es keine Spalte „Sachnummer“ gibt, dann sind „Restzeilen“ = alle Vorlagenzeilen
                    restRows = template.AsEnumerable();
                }

                foreach (var r in restRows)
                    table.ImportRow(r);
            }

            return table;
        }


        private static DataTable CreateEmptyBomTable()
        {
            DataTable table = new("Bom");
            table.Columns.Add("Sachnummer", typeof(string));
            table.Columns.Add("Bezeichnung", typeof(string));
            table.Columns.Add("Länge", typeof(string));
            table.Columns.Add("LEinheit", typeof(string));
            table.Columns.Add("Breite", typeof(string));
            table.Columns.Add("BEinheit", typeof(string));
            table.Columns.Add("Menge", typeof(string));
            table.Columns.Add("MEinheit", typeof(string));
            return table;
        }

        private static void SetValue(DataRow row, string columnName, object? value)
        {
            if (!row.Table.Columns.Contains(columnName))
            {
                return;
            }

            if (value is null)
            {
                row[columnName] = DBNull.Value;
                return;
            }

            var column = row.Table.Columns[columnName];

            if (column.DataType == typeof(string))
            {
                row[columnName] = value.ToString();
                return;
            }

            try
            {
                row[columnName] = Convert.ChangeType(value, column.DataType);
            }
            catch
            {
                row[columnName] = value;
            }
        }



        // Hilfsfunktion: bildet die finale Artikelnummer (ohne Extension, optional Facestyle, optional "I9"-Prefix)
        static string BuildArticleNumber(string? occurrenceName, string? faceStyle)
        {
            // 1) Null/Whitespace absichern und trimmen
            var name = (occurrenceName ?? string.Empty).Trim();

            // 2) Alles bis zum letzten '.' (Dateiendung entfernen)
            int lastDotIndex = name.LastIndexOf('.');
            var articleNumber = (lastDotIndex > 0) ? name.Substring(0, lastDotIndex) : name;

            // 3) Facestyle anhängen (falls vorhanden)
            if (!string.IsNullOrWhiteSpace(faceStyle))
                articleNumber += "-" + faceStyle.Trim();

            // 4) "I9" nur hinzufügen, wenn nicht mit 'z' oder 'Z' beginnend
            if (!articleNumber.StartsWith("z", StringComparison.OrdinalIgnoreCase))
                articleNumber = "I9" + articleNumber;

            return articleNumber;
        }


        public Dictionary<string, object> BuildPayloadData(AsmModel asmModel, bool saveToSeDoc)
        {
            if (asmModel is null)
            {
                throw new ArgumentNullException(nameof(asmModel));
            }

            if (string.IsNullOrWhiteSpace(asmModel.Description))
            {
                asmModel.Description = new PartDescriptionBuilder().BuildDescription(asmModel);
            }

            var storageData = new Dictionary<string, object>(StringComparer.Ordinal)
            {
                { "Artikelnummer", asmModel.Articlenumber },
                { "Bauteilart", asmModel.Parttype },
                { "Länge", asmModel.Length },
                { "Breite", asmModel.Width },
                { "Höhe", asmModel.Height },
                { "Gewicht", asmModel.Mass },
                { "Lagereinheit", asmModel.StorageUnit },
                { "Beschreibung", asmModel.Description },
                { "Lagerplatz", asmModel.StorageLocation },
                { "JPG-Pfad", asmModel.JpgFilePath },
                { "Dateipfad", asmModel.FilePath },
                { "Lieferant", asmModel.Supplier },
                { "Beschaffungsart", asmModel.ProcurementType },
                { "Oberfläche", asmModel.Surface }
            };

            var payloadData = new Dictionary<string, object>(storageData, StringComparer.Ordinal);

            var bomModel = BomModel.FromDataTable(asmModel.BomTemplate);
            asmModel.Bom = new List<BomEntry>(bomModel.Items);

            if (!string.IsNullOrWhiteSpace(bomModel.Department))
            {
                storageData["Abteilung"] = bomModel.Department!;
                payloadData["Abteilung"] = bomModel.Department!;
            }

            payloadData["BomEntries"] = asmModel.Bom;

            //if (asmModel.Bom.Count > 0)
            //{
            //    storageData.Add("Stückliste Anfang", "--------");
            //    foreach (var entry in asmModel.Bom)
            //    {
            //        storageData[entry.ItemNumber] = entry.ToLegacyValue();
            //    }
            //    storageData.Add("Stückliste Ende", "--------");
            //}


            if (saveToSeDoc)
            {
                _asmRepo.SaveAsm(storageData);
            }
                

            return payloadData;
        }


    }
}