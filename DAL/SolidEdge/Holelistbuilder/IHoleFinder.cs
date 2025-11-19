using SolidEdgePart;
using System.Collections.Generic;

namespace DAL.SolidEdge.HoleListBuilder
{
    /// <summary>
    /// Abstraktion für alle Bohrungs-Finder.
    /// Implementierungen sollen niemals <c>null</c> zurückgeben.
    /// </summary>
    public interface IHoleFinder
    {
        /// <summary>
        /// Ermittelt Bohrungen der jeweiligen Quelle (direkt, UDP, Muster, Spiegelung, ...).
        /// </summary>
        /// <returns>
        /// Liste gefundener <see cref="Hole"/>-Objekte.
        /// Nie <c>null</c> – bei keiner Bohrung eine leere Liste.
        /// </returns>
        List<Hole> GetHoles();
    }
}
