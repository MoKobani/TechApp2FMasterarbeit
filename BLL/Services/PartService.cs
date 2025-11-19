using BLL.Models;
using DAL.SolidEdge.Data;
using System;
using System.Collections.Generic;
using BLL.Helpers;


namespace BLL.Services
{
    /// <summary>
    /// BLL-Schicht zum Verarbeiten von <see cref="PartModel"/>-Objekten.
    /// Greift über <see cref="IPartRepository"/> auf die Datenquelle zu.
    /// </summary>
    public class PartService
    {
        private readonly PartRepo _repo;
        private const string BomEntriesKey = "BomEntries";

        /// <summary>
        /// Parameterloser Konstruktor für einfache Nutzung ohne DI.
        /// Intern wird die Standard-Implementierung verwendet.
        /// </summary>
        public PartService() : this(new PartRepo())
        {
        }

        /// <summary>
        /// Repository wird per Dependency Injection bereitgestellt.
        /// So kann beim Testen ein Mock übergeben werden.
        /// </summary>
        public PartService(PartRepo repo)
        {
            _repo = repo;
        }

        /// <inheritdoc />
        public PartModel GetPart()
        {
            var data = _repo.Loadpart();

            // Daten aus dem Repository in das BLL-Modell übertragen
            var part = new PartModel
            {
                Articlenumber = data.Articlenumber,
                Holesummary = data.Holesummary,
                Length = data.Length,
                Width = data.Width,
                Height = data.Height,
                ImagePath = data.ImagePath,
                MaterialThickness = data.MaterialThickness,
                FilePath = PathConverter.ToLocalDrivePath(data.FilePath),
                JpgFilePath = PathConverter.ToLocalDrivePath(data.FilePath, ".jpg"),
                Material = data.Material,
                StorageLocation = data.StorageLocation,
                Parttype = data.Parttype,
                Supplier = data.Supplier,
                Mass = data.Mass,
                StorageUnit = data.SortageUnit,
                ProcurementType = data.ProcurementType,
            };

            if (string.Equals(part.ProcurementType, "Eigenfertigung", StringComparison.OrdinalIgnoreCase))
            {
                var legacyEntries = data.BomEntries ?? new Dictionary<string, string>(StringComparer.Ordinal);
                var bomModel = BomModel.FromLegacyDictionary(legacyEntries, data.BomDepartment);
                part.Bom = new List<BomEntry>(bomModel.Items);

                var bomTable = BomModel.ToDataTable(part.Bom, bomModel.Department);
                part.BomTemplate = bomTable.Rows.Count > 0 ? bomTable : null;
            }
            else
            {
                part.Bom = new List<BomEntry>();
                part.BomTemplate = null;
            }

            if (string.IsNullOrWhiteSpace(part.Description))
            {
                part.Description = new PartDescriptionBuilder().BuildDescription(part);
            }
            return part;
        }

        /// <inheritdoc />
        public Dictionary<string, object> BuildPayloadData(PartModel part , bool saveToSePart)
        {
            if (string.IsNullOrWhiteSpace(part.Description))
            {
                part.Description = new PartDescriptionBuilder().BuildDescription(part);
            }

            // Dictionary für flexible Attributübergabe an das Repository
            Dictionary<string, object> storageData = new(StringComparer.Ordinal)
            {
                { "Artikelnummer", part.Articlenumber },
                { "Bauteilart", part.Parttype },
                { "Länge", part.Length },
                { "Breite", part.Width },
                { "Höhe", part.Height },
                { "Gewicht", part.Mass },
                { "Lagereinheit", part.StorageUnit},
                { "Beschreibung", part.Description },
                { "Lagerplatz", part.StorageLocation },
                {"JPG-Pfad",  part.JpgFilePath},
                { "Dateipfad", part.FilePath },
                { "Lieferant", part.Supplier },
                { "Beschaffungsart", part.ProcurementType },
                { "Oberfläche", part.Surface}

            };

            Dictionary<string, object> payloadData = new(storageData, StringComparer.Ordinal);

            // Stückliste nur anhängen, wenn keine Fremdbeschaffung
            if (string.Equals(part.ProcurementType, "Eigenfertigung", StringComparison.OrdinalIgnoreCase))
            {
                var bomModel = BomModel.FromDataTable(part.BomTemplate);
                part.Bom = new List<BomEntry>(bomModel.Items);

                if (!string.IsNullOrWhiteSpace(bomModel.Department))
                {
                    storageData["Abteilung"] = bomModel.Department!;
                    payloadData["Abteilung"] = bomModel.Department!;
                }

                storageData.Add("Stückliste Anfang", "--------");
                foreach (var entry in part.Bom)
                {
                    storageData[entry.ItemNumber] = entry.ToLegacyValue();
                }
                storageData.Add("Stückliste Ende", "--------");

                payloadData[BomEntriesKey] = part.Bom;
            }
            else
            {
                part.Bom = new List<BomEntry>();
                part.BomTemplate = null;
                payloadData[BomEntriesKey] = part.Bom;
            }

            if (saveToSePart)
            {
                _repo.SavePart(storageData);
            }
            return payloadData;
        }
    }
}

