using SolidEdgePart;
using SolidEdgeFramework;
using System;

namespace DAL.SolidEdge.SeObjects
{
    /// <summary>
    /// Kapselt das Auslesen von <see cref="Model"/>-/ <see cref="Models"/>-Sammlungen
    /// aus Part- oder SheetMetal-Dokumenten.
    /// </summary>
    public class SeModels
    {
        // Referenz auf das aktuell aktive Solid Edge Dokument (COM).
        private readonly SolidEdgeDocument _activeDocument;

        /// <summary>
        /// Erstellt eine neue Instanz für das übergebene aktive Dokument.
        /// </summary>
        /// <param name="activeDocument">Aktives <see cref="SolidEdgeDocument"/>.</param>
        public SeModels(SolidEdgeDocument activeDocument)
        {
            _activeDocument = activeDocument;
        }

        /// <summary>
        /// Liest aus einem Part- oder SheetMetal-Dokument alle Modelle aus.
        /// </summary>
        /// <remarks>
        /// Gibt <c>null</c> zurück, wenn das aktive Dokument weder ein <see cref="PartDocument"/>
        /// noch ein <see cref="SheetMetalDocument"/> ist.<br/>
        /// Wirft eine <see cref="InvalidOperationException"/>, wenn beim Zugriff auf die COM-API ein Fehler auftritt.
        /// </remarks>
        /// <returns>
        /// Die gefundenen <see cref="Models"/> oder <c>null</c>, wenn der Dokumenttyp nicht unterstützt wird.
        /// </returns>
        public Models? GetModels()
        {
            try
            {
                // Kompakte Typverzweigung: Je nach tatsächlichem Dokumenttyp die Models liefern.
                return _activeDocument switch
                {
                    PartDocument partDoc => partDoc.Models,
                    SheetMetalDocument sheetMetalDoc => sheetMetalDoc.Models,
                    _ => throw new InvalidOperationException("Das aktive ist kein .part oder .psm")
                };
            }
            catch (Exception ex)
            {
                // Fehler aus der COM-API lesbar kapseln und StackTrace bewahren.
                throw new InvalidOperationException(
                    "Das aktive ist kein .part oder .psm", ex);
            }
        }

        /// <summary>
        /// Liefert ein einzelnes <see cref="Model"/> anhand einer 1-basierten Position.
        /// </summary>
        /// <param name="number">1-basierter Index des Modells (>= 1).</param>
        /// <remarks>
        /// Gibt <c>null</c> zurück, wenn das aktive Dokument kein Part- oder SheetMetal-Dokument ist.
        /// Wirft:
        /// <list type="bullet">
        /// <item><see cref="ArgumentOutOfRangeException"/> wenn <paramref name="number"/> &lt; 1 ist.</item>
        /// <item><see cref="InvalidOperationException"/> wenn keine Modelle vorhanden sind oder der Index außerhalb des Bereichs liegt.</item>
        /// </list>
        /// </remarks>
        /// <returns>Gefundenes <see cref="Model"/> oder <c>null</c>, wenn der Dokumenttyp nicht unterstützt wird.</returns>
        public Model? GetModel(int number)
        {
            if (number < 1)
                throw new ArgumentOutOfRangeException(nameof(number), "Der Index ist 1-basiert und muss ≥ 1 sein.");

            try
            {
                var models = GetModels();

                // Dokumenttyp nicht unterstützt → konsistent zu GetModels(): null zurückgeben.
                if (models is null)
                    return null;

                // Prüfen, ob überhaupt Modelle existieren.
                if (models.Count == 0)
                    throw new InvalidOperationException("Im aktiven Dokument sind keine Modelle vorhanden.");

                // Bereichsprüfung (COM-Collection ist 1-basiert).
                if (number > models.Count)
                    throw new InvalidOperationException(
                        $"Der angeforderte Index ({number}) liegt außerhalb des Bereichs (1..{models.Count}).");

                // Zugriff ist 1-basiert.
                return models.Item(number);
            }
            catch (InvalidOperationException)
            {
                // Bereits aussagekräftig → direkt weiterwerfen.
                throw;
            }
            catch (Exception ex)
            {
                // Unerwartete Fehler kapseln.
                throw new InvalidOperationException(
                    "Fehler beim Auslesen eines Modells aus dem aktiven Dokument.", ex);
            }
        }
    }
}