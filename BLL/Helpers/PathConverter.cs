using System;
using System.IO;
using System.Collections.Generic;

namespace BLL.Helpers
{
    /// <summary>
    /// Hilfsklasse zum Umwandeln von UNC-Pfaden (z.B. \\datastorevm\datastore\...)
    /// in lokale Laufwerkspfade (z.B. G:\...) und zum optionalen Ändern der Dateiendung.
    /// </summary>
    public static class PathConverter
    {
        // Optional: Mehrere Netzlaufwerk-Mappings kannst du hier hinzufügen
        private static readonly Dictionary<string, string> NetworkMappings = new()
        {
            [@"\\datastorevm\datastore"] = @"G:",
            [@"\\datastorevm.2f-leuchten.com\datastore"] = @"G:"
            // Weitere Mappings:
            // [@"\\anderevm\freigabe"] = @"H:",
        };

        /// <summary>
        /// Wandelt einen Eingabepfad in einen lokalen Laufwerkspfad um.
        /// Optional wird die Dateiendung geändert.
        /// </summary>
        /// <param name="inputPath">Originalpfad (UNC oder lokal).</param>
        /// <param name="fileType">
        /// Neue Dateiendung (z.B. ".jpg", ".pdf").
        /// Wenn null oder leer, bleibt die Original-Endung erhalten.
        /// </param>
        /// <returns>Lokaler Pfad mit ggf. angepasster Endung.</returns>
        public static string ToLocalDrivePath(string inputPath, string? fileType = null)
        {
            // 1️⃣ Eingabe prüfen
            if (string.IsNullOrWhiteSpace(inputPath))
                return string.Empty;

            // 2️⃣ Slashes vereinheitlichen (UNC-Pfade nutzen immer Backslashes)
            string normalized = inputPath.Trim().Replace('/', '\\');

            // 3️⃣ Netzwerkpfade in lokale Laufwerksbuchstaben umwandeln
            foreach (var mapping in NetworkMappings)
            {
                if (normalized.StartsWith(mapping.Key, StringComparison.OrdinalIgnoreCase))
                {
                    normalized = normalized.Replace(mapping.Key, mapping.Value);
                    break;
                }
            }

            // 4️⃣ Wenn eine neue Dateiendung angegeben wurde → ändern
            if (!string.IsNullOrWhiteSpace(fileType))
            {
                // Sicherstellen, dass fileType mit Punkt beginnt
                if (!fileType.StartsWith("."))
                    fileType = "." + fileType;

                normalized = Path.ChangeExtension(normalized, fileType);
            }

            // 5️⃣ Ergebnis zurückgeben
            return normalized;
        }
    }
}
