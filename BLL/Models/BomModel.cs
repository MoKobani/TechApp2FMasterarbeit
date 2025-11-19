using System;
using System.Collections.Generic;
using System.Data;

namespace BLL.Models
{
    /// <summary>
    /// Repräsentiert eine strukturierte Stücklistenposition.
    /// </summary>
    public sealed record BomEntry
    {
        public BomEntry(
            string itemNumber,
            string description,
            string length,
            string lengthUnit,
            string width,
            string widthUnit,
            string amount,
            string amountUnit)
        {
            ItemNumber = itemNumber?.Trim() ?? string.Empty;
            Description = description?.Trim() ?? string.Empty;
            Length = length?.Trim() ?? string.Empty;
            LengthUnit = lengthUnit?.Trim() ?? string.Empty;
            Width = width?.Trim() ?? string.Empty;
            WidthUnit = widthUnit?.Trim() ?? string.Empty;
            Amount = amount?.Trim() ?? string.Empty;
            AmountUnit = amountUnit?.Trim() ?? string.Empty;
        }

        public string ItemNumber { get; }

        public string Description { get; }

        public string Length { get; }

        public string LengthUnit { get; }

        public string Width { get; }

        public string WidthUnit { get; }

        public string Amount { get; }

        public string AmountUnit { get; }

        /// <summary>
        /// Formatiert die Stücklistenwerte im bisherigen Textformat, um sie als Custom Property zu speichern.
        /// </summary>
        public string ToLegacyValue()
        {
            var formatted = $"Länge = {Length} {LengthUnit} / Breite = {Width} {WidthUnit} / Menge = {Amount} {AmountUnit}";
            return formatted.Replace("  ", " ").Trim();
        }

        /// <summary>
        /// Erstellt einen <see cref="BomEntry"/> aus dem früher genutzten Textformat.
        /// </summary>
        public static BomEntry FromLegacyValue(string itemNumber, string? legacyValue)
        {
            var (length, lengthUnit, width, widthUnit, amount, amountUnit) = ParseLegacyValue(legacyValue);
            return new BomEntry(itemNumber, string.Empty, length, lengthUnit, width, widthUnit, amount, amountUnit);
        }

        private static (string length, string lengthUnit, string width, string widthUnit, string amount, string amountUnit) ParseLegacyValue(string? legacyValue)
        {
            string length = string.Empty;
            string lengthUnit = string.Empty;
            string width = string.Empty;
            string widthUnit = string.Empty;
            string amount = string.Empty;
            string amountUnit = string.Empty;

            if (string.IsNullOrWhiteSpace(legacyValue))
            {
                return (length, lengthUnit, width, widthUnit, amount, amountUnit);
            }

            var segments = legacyValue.Split('/', StringSplitOptions.RemoveEmptyEntries);
            foreach (var segment in segments)
            {
                var trimmedSegment = segment.Trim();
                if (trimmedSegment.Length == 0)
                {
                    continue;
                }

                int separatorIndex = trimmedSegment.IndexOf('=');
                if (separatorIndex < 0)
                {
                    continue;
                }

                var key = trimmedSegment[..separatorIndex].Trim();
                var valuePart = trimmedSegment[(separatorIndex + 1)..].Trim();
                var (value, unit) = SplitValueAndUnit(valuePart);

                if (string.Equals(key, "Länge", StringComparison.OrdinalIgnoreCase))
                {
                    length = value;
                    lengthUnit = unit;
                }
                else if (string.Equals(key, "Breite", StringComparison.OrdinalIgnoreCase))
                {
                    width = value;
                    widthUnit = unit;
                }
                else if (string.Equals(key, "Menge", StringComparison.OrdinalIgnoreCase))
                {
                    amount = value;
                    amountUnit = unit;
                }
            }

            return (length, lengthUnit, width, widthUnit, amount, amountUnit);
        }

        private static (string value, string unit) SplitValueAndUnit(string valuePart)
        {
            if (string.IsNullOrWhiteSpace(valuePart))
            {
                return (string.Empty, string.Empty);
            }

            var parts = valuePart.Split(' ', StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length == 0)
            {
                return (string.Empty, string.Empty);
            }

            string value = parts[0];
            string unit = parts.Length > 1 ? string.Join(" ", parts, 1, parts.Length - 1) : string.Empty;
            return (value, unit);
        }
    }

    /// <summary>
    /// Domänenmodell für eine Stückliste (BOM) inklusive zugehöriger Abteilung.
    /// </summary>
    public sealed class BomModel
    {
        public IReadOnlyList<BomEntry> Items { get; init; } = Array.Empty<BomEntry>();

        public string? Department { get; init; }

        /// <summary>
        /// Baut ein <see cref="BomModel"/> aus einer DataTable.
        /// </summary>
        public static BomModel FromDataTable(DataTable? table)
        {
            var items = new List<BomEntry>();
            string? department = null;

            if (table is null || table.Rows.Count == 0)
            {
                return new BomModel { Items = items, Department = department };
            }

            static string Get(DataRow row, string columnName)
                => row.Table.Columns.Contains(columnName)
                    ? row[columnName]?.ToString()?.Trim() ?? string.Empty
                    : string.Empty;

            foreach (DataRow row in table.Rows)
            {
                try
                {
                    string sachnummer = Get(row, "Sachnummer");

                    if (string.Equals(sachnummer, "Abteilung", StringComparison.OrdinalIgnoreCase))
                    {
                        string value = Get(row, "Länge");
                        if (string.IsNullOrWhiteSpace(value))
                        {
                            value = Get(row, "Bezeichnung");
                        }

                        if (!string.IsNullOrWhiteSpace(value))
                        {
                            department = value;
                        }

                        continue;
                    }

                    if (string.IsNullOrWhiteSpace(sachnummer))
                    {
                        continue;
                    }

                    var entry = new BomEntry(
                        sachnummer,
                        Get(row, "Bezeichnung"),
                        Get(row, "Länge"),
                        Get(row, "LEinheit"),
                        Get(row, "Breite"),
                        Get(row, "BEinheit"),
                        Get(row, "Menge"),
                        Get(row, "MEinheit"));

                    items.Add(entry);
                }
                catch (Exception ex)
                {
                    throw new InvalidOperationException(
                        $"Fehler beim Verarbeiten einer Zeile:{Environment.NewLine}{ex.Message}");
                }
            }

            return new BomModel
            {
                Items = items,
                Department = department
            };
        }

        /// <summary>
        /// Erzeugt ein <see cref="BomModel"/> aus den gespeicherten Custom-Properties.
        /// </summary>
        public static BomModel FromLegacyDictionary(Dictionary<string, string> bomEntries, string? department)
        {
            var items = new List<BomEntry>();

            if (bomEntries is null || bomEntries.Count == 0)
            {
                return new BomModel { Items = items, Department = department };
            }

            foreach (var entry in bomEntries)
            {
                if (string.Equals(entry.Key, "Abteilung", StringComparison.OrdinalIgnoreCase))
                {
                    if (string.IsNullOrWhiteSpace(department) && !string.IsNullOrWhiteSpace(entry.Value))
                    {
                        department = entry.Value.Trim();
                    }

                    continue;
                }

                if (string.IsNullOrWhiteSpace(entry.Key))
                {
                    continue;
                }

                items.Add(BomEntry.FromLegacyValue(entry.Key, entry.Value));
            }

            return new BomModel
            {
                Items = items,
                Department = department
            };
        }

        /// <summary>
        /// Wandelt die gespeicherten Stücklisten-Einträge in eine DataTable-Struktur um.
        /// </summary>
        public static DataTable ToDataTable(IEnumerable<BomEntry> entries, string? department)
        {
            var table = CreateEmptyBomTable();

            if (!string.IsNullOrWhiteSpace(department))
            {
                var departmentValue = department.Trim();
                var departmentRow = table.NewRow();
                departmentRow["Sachnummer"] = "Abteilung";
                departmentRow["Bezeichnung"] = departmentValue;
                departmentRow["Länge"] = departmentValue;
                table.Rows.Add(departmentRow);
            }

            if (entries is null)
            {
                return table;
            }

            foreach (var entry in entries)
            {
                var row = table.NewRow();
                row["Sachnummer"] = entry.ItemNumber;
                row["Bezeichnung"] = entry.Description;
                row["Länge"] = entry.Length;
                row["LEinheit"] = entry.LengthUnit;
                row["Breite"] = entry.Width;
                row["BEinheit"] = entry.WidthUnit;
                row["Menge"] = entry.Amount;
                row["MEinheit"] = entry.AmountUnit;
                table.Rows.Add(row);
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
    }
}
