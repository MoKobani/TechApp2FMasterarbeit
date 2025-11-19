using System.Collections.Generic;

namespace DAL.SolidEdge.Data
{
    /// <summary>
    /// Definiert, welche Datenoperationen für ein Teil verfügbar sind.
    /// Durch die Nutzung eines Interfaces können wir die Implementierung später austauschen,
    /// ohne den restlichen Code ändern zu müssen.
    /// </summary>
    public interface IPartRepository
    {
        /// <summary>
        /// Lädt die Daten eines Teils aus Solid Edge.
        /// </summary>
        /// <returns>Tupel mit den wichtigsten Eigenschaften des Teils.</returns>
        (
            string Articlenumber,
            string Holesummary,
            string FilePath,
            double Length,
            double Width,
            double Height,
            double MaterialThickness,
            string ImagePath,
            string Material,
            string StorageLocation,
            string Parttype,
            string Supplier,
            double Mass,
            string SortageUnit,
            string ProcurementType,
            Dictionary<string, string> BomEntries,
            string? BomDepartment

        ) Loadpart();

        /// <summary>
        /// Speichert die übergebenen Teilinformationen.
        /// Die Dictionary-Struktur ermöglicht eine flexible Erweiterung der Daten.
        /// </summary>
        /// <param name="data">Schlüssel/Wert-Paar der zu speichernden Eigenschaften.</param>
        void SavePart(Dictionary<string, object> data);
    }
}
