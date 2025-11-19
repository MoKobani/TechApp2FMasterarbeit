using System.Collections.Generic;
using System.Globalization;
using BLL.Models;

namespace BLL.Helpers
{
    /// <summary>
    /// Baut eine Beschreibung für ein Einzelteil zusammen <see cref="PartModel"/>.
    /// </summary>
    public class PartDescriptionBuilder
    {
        /// <summary>
        /// Erstellt eine Beschreibung für das angegebene Bauteil.  
        /// </summary>
        public string BuildDescription(PartModel part)
        {
            var culture = CultureInfo.CurrentCulture;

            // --- Prefix (z.B. Ø120, s=4) ---
            var prefixParts = new List<string>();
            if (part.IsRound) prefixParts.Add($"Ø{part.Length.ToString("0.###", culture)}");
            if (part.IsSheetMetal) prefixParts.Add($"s={part.MaterialThickness.ToString("0.###", culture)}");

            string prefix = string.Join(" ",prefixParts);

            // --- Linker Teil (z.B. Zuschnitt Messing) ---
            string left = string.Empty;
            bool hasParttype = !string.IsNullOrWhiteSpace(part.Parttype);
            bool hasMaterial = !string.IsNullOrWhiteSpace(part.Material);

            string parttype = part.Parttype.Split(' ')[^1]; // letzter Wort von parttype (z.B. "Wasserstrahl Zuschnitt" zu "Zuschnitt")

            if (hasParttype && hasMaterial)
                left = $"{parttype} {part.Material}";
            else if (hasParttype)
                left = $"{parttype}:";
            else if (hasMaterial)
                left = $"{part.Material}:";

            // --- Rechter Teil (z.B. mit Bohrungen: 2xØ5, 1x M10) ---
            string right = string.IsNullOrWhiteSpace(part.Holesummary) ? string.Empty : $"mit {part.Holesummary}";

            // --- Alle Teile zusammenbauen ---
            return $"{left} {prefix} {right}";
        }
    }
}
