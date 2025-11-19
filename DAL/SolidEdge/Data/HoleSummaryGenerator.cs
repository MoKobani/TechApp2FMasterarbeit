using SolidEdgePart;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace DAL.SolidEdge.Data
{
    public class HoleSummaryGenerator
    {
        // Deine Konstanten aus der Beschreibung:
        private const int HoleType_EinfacheBohrung = 33; // HoleData.HoleType
        private const int HoleType_Senkbohrung = 35; // HoleData.HoleType
        private const int HoleType_Außengewinde = 36; // HoleData.HoleType

        private const int Treatment_Einfach = 44; // HoleData.TreatmentType
        private const int Treatment_Gewinde = 37; // HoleData.TreatmentType

        /// <summary>
        /// Erzeugt eine Zusammenfassung der Bohrungen im Model, z.B.
        /// "3x Ø5, 2x M10x1, 3x M5-Senkung"
        /// </summary>
        public static string Summarize(Model model)
        {
            if (model == null) throw new ArgumentNullException( nameof(model));

            var holeGeoms = model.HoleGeometries;
            if (holeGeoms == null || holeGeoms.Count == 0)
                return "";

            // Gruppenzähler pro textuelle Beschreibung
            var counts = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);

            foreach (HoleGeometry geom in holeGeoms)
            {
                var data = geom?.HoleData;
                if (data == null) continue;

                var descriptor = BuildDescriptor(data);
                if (string.IsNullOrWhiteSpace(descriptor)) continue;

                int n;
                if (counts.TryGetValue(descriptor, out n))
                    counts[descriptor] = n + 1;
                else
                    counts[descriptor] = 1;
            }

            if (counts.Count == 0)
                return "";

            // Sortierung: Ø… zuerst, dann Gewinde, dann Senkung, danach alphabetisch
            var ordered = counts
                .OrderBy(kv => SortKey(kv.Key))
                .ThenBy(kv => kv.Key, StringComparer.OrdinalIgnoreCase)
                .Select(kv => FormatCountAndLabel(kv.Value, kv.Key));

            return  string.Join(", ", ordered);
        }

        private static string BuildDescriptor(HoleData data)
        {
            // 1) Einfache Bohrung: HoleType=33 & Treatment=44 -> "Ø{Durchmesser}"
            if (((int)data.HoleType) == HoleType_EinfacheBohrung && ((int)data.TreatmentType) == Treatment_Einfach)
            {
                var dMm = ToMillimeters(data.HoleDiameter);
                var pretty = TrimZeros(dMm);
                return $"Ø{pretty}";
            }

            // 2) Gewindebohrung: HoleType=33 & Treatment=37 -> ThreadDescription (z.B. "M10x1")
            if (((int)data.HoleType) == HoleType_EinfacheBohrung && ((int)data.TreatmentType) == Treatment_Gewinde)
            {
                var desc = SafeThreadDescription(data.ThreadDescription);
                return desc; // "M10x1" o.ä.
            }

            // 3) Senkbohrung: HoleType=35 & Treatment=44 -> "{ThreadDescription}-Senkung" (z.B. "M5-Senkung")
            if (((int)data.HoleType) == HoleType_Senkbohrung && ((int)data.TreatmentType) == Treatment_Einfach)
            {
                var desc = SafeThreadDescription(data.ThreadDescription);
                return $"{desc}-Senkung";
            }


            // 3) Außengewinde: HoleType=36 & Treatment=37 -> "{ThreadDescription}-Außengewinde" (z.B. "M5-Außengewinde")
            if (((int)data.HoleType) == HoleType_Außengewinde && ((int)data.TreatmentType) == Treatment_Gewinde)
            {
                var desc = SafeThreadDescription(data.ThreadDescription);
                return $"{desc}-Außengewinde";
            }


            // Fallback: wenn nichts passt, gib trotzdem etwas Sinnvolles zurück
            return $"Typ{data.HoleType}/Tr{data.TreatmentType}";
        }

        private static string SafeThreadDescription(string s)
        {
            if (string.IsNullOrWhiteSpace(s))
                return "Gewinde";

            // Aufräumen:
            // - Leer- und schmale/geschützte Leerzeichen entfernen
            // - Multiplikationszeichen „×“ in 'x' umwandeln
            // - Tabs usw. eliminieren
            return s
                .Trim()
                .Replace(" ", "")
                .Replace("\u00A0", "") // non-breaking space
                .Replace("\u2009", "") // thin space
                .Replace("\u2002", "") // en space
                .Replace("\u2003", "") // em space
                .Replace("\t", "")
                .Replace("×", "x");
        }

        private static string FormatCountAndLabel(int count, string label)
        {
            // Konsistente Ausgabe: "3x Ø5" / "2x M10x1" / "3x M5-Senkung"
            return $"{count}x {label}";
        }

        private static int SortKey(string label)
        {
            // Ø... zuerst (0), Gewinde "M..." (1), "-Senkung" (2), sonst (3)
            if (label.StartsWith("Ø", StringComparison.Ordinal)) return 0;
            if (label.StartsWith("M", StringComparison.OrdinalIgnoreCase) && !label.Contains("-Senkung", StringComparison.OrdinalIgnoreCase)) return 1;
            if (label.Contains("-Senkung", StringComparison.OrdinalIgnoreCase)) return 2;
            return 3;
        }

        /// <summary>
        /// Heuristik: SE liefert oft Meter. Wenn Wert < 0,5, interpretieren wir als Meter und wandeln in mm.
        /// Ansonsten gehen wir davon aus, dass es schon mm sind.
        /// </summary>
        private static double ToMillimeters(double value)
        {
            return value < 0.5 ? value * 1000.0 : value;
        }

        /// <summary>
        /// Schöne Zahlendarstellung (ohne unnötige Nachkommastellen).
        /// </summary>
        private static string TrimZeros(double value, int maxDecimals = 3)
        {
            var rounded = Math.Round(value, maxDecimals, MidpointRounding.AwayFromZero);
            return rounded.ToString("0.###", CultureInfo.InvariantCulture);
        }
    }
}
