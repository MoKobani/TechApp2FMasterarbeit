using SolidEdgePart;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;

namespace DAL.SolidEdge.Holelistbuilder
{
    /// <summary>
    /// Baut eine kompakte, deutschsprachige Zusammenfassung der übergebenen Bohrungen.
    /// Beispielausgabe:
    /// "Bohrungen: 2x M5, 1x M24 Zylindersenkung, 1x M10 Senkung, 1x Ø5,3, 1x Ø10."
    /// </summary>
    public static class HoleSummaryBuilder
    {
        /// <summary>
        /// Erstellt die Zusammenfassung. 
        /// </summary>
        /// <param name="holes">Liste von SolidEdgePart.Hole</param>
        /// <returns>Formatierte Zeichenkette</returns>
        public static string BuildSummary(IEnumerable<Hole> holes)
        {
            // Sicherheitscheck.
            if (holes == null)
                return "Bohrungen: –";

            // Wir wollen die Reihenfolge der ersten Vorkommen beibehalten.
            // Darum merken wir uns zusätzlich zur Zähl-Dictionary eine "Order"-Liste.
            var counts = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
            var order = new List<string>();

            foreach (var hole in holes)
            {
                if (hole == null || hole.Suppress)
                    continue;

                // Hole Daten aus dem Feature
                var holeData = hole.HoleData as HoleData;
                if (holeData == null)
                    continue;

                string label = BuildLabel(holeData);

                // Label zählen und erste Auftretensreihenfolge merken
                if (!counts.ContainsKey(label))
                {
                    counts[label] = 1;
                    order.Add(label);
                }
                else
                {
                    counts[label]++;
                }
            }

            // Wenn nichts zählbares vorhanden ist
            if (order.Count == 0)
                return "";

            // Ausgabe zusammenbauen
            var sb = new StringBuilder();
            sb.Append("Bohrungen: ");

            for (int i = 0; i < order.Count; i++)
            {
                var label = order[i];
                var count = counts[label];

                // "2x M5" / "1x M10 Senkung" / "1x Ø5,3"
                sb.Append(count).Append("x ").Append(label);

                if (i < order.Count - 1)
                    sb.Append(", ");
                else
                    sb.Append(".");
            }

            return sb.ToString();
        }

        /// <summary>
        /// Erzeugt die menschenlesbare Bezeichnung für eine einzelne Bohrung,
        /// basierend auf SubType/Size/Durchmesser.
        /// </summary>
        private static string BuildLabel(HoleData holeData)
        {
            // Kultur "de-DE" für Komma als Dezimaltrennzeichen.
            var de = new CultureInfo("de-DE");

            string subtype = holeData.SubType != null ? holeData.SubType.ToString() : string.Empty;
            string size = holeData.Size != null ? holeData.Size.ToString() : string.Empty;
            double dia = 0.0;
            try { dia = holeData.HoleDiameter * 1000; } catch { /* manche Subtypes liefern ggf. keinen Durchmesser */ } //  (dia = holeData.HoleDiameter * 1000) von m in mm

            // 1) Standardgewinde -> "M5", "M10", ...
            if (Contains(subtype, "Standardgewinde") && !string.IsNullOrWhiteSpace(size))
                return NormalizeThreadSize(size);

            // 2) Zylinderschraube mit Innensechskant.DIN4762 -> "<Size> Zylindersenkung"
            if (Contains(subtype, "Zylinderschraube") && !string.IsNullOrWhiteSpace(size))
                return $"{size} Zylindersenkung";

            // 3) Senkschraube mit Innensechskant. 10642 -> "<Size> Senkung"
            if (Contains(subtype, "Senkschraube") && !string.IsNullOrWhiteSpace(size))
                return $"{size} Senkung";

            // 4) Schraubendurchgangsbohrung / Bohrdurchmesser -> "Ø<Durchmesser>"
            if (Contains(subtype, "Schraubendurchgangsbohrung") || Contains(subtype, "Bohrdurchmesser"))
                return $"Ø{FormatDiameter(dia, de)}";

            // 5) Fallbacks: wenn Size wie "M.." aussieht, nimm Size; sonst Durchmesser, wenn vorhanden
            if (!string.IsNullOrWhiteSpace(size) && size.Trim().StartsWith("M", StringComparison.OrdinalIgnoreCase))
                return size;

            if (dia > 0)
                return $"Ø{FormatDiameter(dia, de)}";

            // Letzter Ausweg: Subtype ausgeben (sehr selten nötig)
            return string.IsNullOrWhiteSpace(subtype) ? "Bohrung" : subtype;
        }

        /// <summary>
        /// Einfache, tolerante Stringsuche (ohne Kultur).
        /// </summary>
        private static bool Contains(string source, string part)
        {
            return source?.IndexOf(part, StringComparison.OrdinalIgnoreCase) >= 0;
        }

        /// <summary>
        /// Formatiert Durchmesser „schön“:
        /// - Ganze Zahlen ohne Nachkommastellen (z. B. 10 -> "10")
        /// - Ansonsten eine Nachkommastelle (z. B. 5.3 -> "5,3") 
        /// </summary>
        private static string FormatDiameter(double value, CultureInfo culture)
        {
            // Ganze Zahl?
            if (Math.Abs(value - Math.Round(value)) < 1e-6)
                return value.ToString("0", culture);

            // Eine Nachkommastelle reicht meist für Bohrungsangaben.
            return value.ToString("0.#", culture);
        }

        /// <summary>
        /// Entfernt Leerzeichen um das "x" in Gewindegrößen.
        /// Beispiel: "M10 x 1" -> "M10x1"
        /// </summary>
        private static string NormalizeThreadSize(string size)
        {
            if (string.IsNullOrWhiteSpace(size))
                return size;

            // Alle Varianten von Leerzeichen rund um 'x' entfernen
            return size.Replace(" x ", "x").Replace(" X ", "x").Trim();
        }
    }
}
