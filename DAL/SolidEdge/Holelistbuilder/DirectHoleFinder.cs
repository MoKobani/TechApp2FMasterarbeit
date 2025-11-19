using SolidEdgePart;
using System;
using System.Collections.Generic;

namespace DAL.SolidEdge.HoleListBuilder
{
    /// <summary>
    /// Findet direkt im Modell vorhandene Bohrungen (ohne Pattern/Spiegelung).
    /// </summary>
    public class DirectHoleFinder : IHoleFinder
    {
        private readonly Model _model;

        /// <summary>
        /// Erstellt den Finder für ein gegebenes Modell.
        /// </summary>
        /// <param name="model">Quellmodell mit Features.</param>
        public DirectHoleFinder(Model model)
        {
            _model = model;
        }

        /// <summary>
        /// Ermittelt alle nicht-unterdrückten direkten Bohrungen.
        /// </summary>
        /// <returns>Liste gefundener <see cref="Hole"/>-Objekte (nie <c>null</c>).</returns>
        public List<Hole> GetHoles()
        {
            // Immer eine Liste zurückgeben → vermeidet NullReferenceExceptions.
            var result = new List<Hole>();

            try
            {
                if (_model == null)
                    return result;

                Holes holes = _model.Holes;
                if (holes == null || holes.Count == 0)
                    return result;

                foreach (Hole hole in holes)
                {
                    // Unterdrückte Bohrungen überspringen
                    if (!hole.Suppress)
                        result.Add(hole);
                }
            }
            catch (Exception e)
            {
                // Minimal halten – Bibliothekscode soll robust sein
                Console.WriteLine("DirectHoleFinder: " + e.Message);
            }

            return result;
        }

        /// <summary>
        /// Alte Bezeichnung beibehalten (Kompatibilität). Delegiert an <see cref="GetHoles"/>.
        /// </summary>
        public List<Hole> GetHols() => GetHoles();


    }
}
