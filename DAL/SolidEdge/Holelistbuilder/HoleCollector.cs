using SolidEdgePart;
using System;
using System.Collections.Generic;


namespace DAL.SolidEdge.HoleListBuilder
{
    /// <summary>
    /// Sammelt alle Bohrungen (direkt, aus UDP, Pattern, MirrorCopy) in einer einzigen Liste.
    /// Nutzt dabei nur die Abstraktion <see cref="IHoleFinder"/> (DIP/OCP).
    /// </summary>
    public class HoleCollector
    {
        private readonly List<IHoleFinder> _finders;

        /// <summary>
        /// Bequemer Standardkonstruktor mit den 4 bekannten Findern.
        /// </summary>
        /// <param name="model">Modell, aus dem Bohrungen gesammelt werden.</param>
        public HoleCollector(Model model)
            : this(new List<IHoleFinder>
            {
                new DirectHoleFinder(model),
                new UserDefinedPatternHoleFinder(model),
                new MirrorCopyHoleFinder(model),
                new PatternHoleFinder(model)
            })
        {
        }

        /// <summary>
        /// Volle Kontrolle via Dependency Injection:
        /// Eine beliebige Menge an Findern kann übergeben werden.
        /// </summary>
        /// <param name="finders">Auflistung von Findern. Darf <c>null</c> sein (wird intern zu leerer Liste).</param>
        public HoleCollector(IEnumerable<IHoleFinder> finders)
        {
            _finders = new List<IHoleFinder>(finders ?? Array.Empty<IHoleFinder>());
        }

        /// <summary>
        /// Führt alle Finder aus und gibt die Kombination der Ergebnisse zurück.
        /// </summary>
        /// <returns>Gesamtliste der gefundenen <see cref="Hole"/>-Objekte (nie <c>null</c>).</returns>
        public List<Hole> CollectAllHoles()
        {
            var allHoles = new List<Hole>();

            foreach (var finder in _finders)
            {
                try
                {
                    var holes = finder?.GetHoles() ?? new List<Hole>();
                    allHoles.AddRange(holes);
                }
                catch (Exception ex)
                {
                    Console.WriteLine("HoleCollector/Finder-Fehler: " + ex.Message);
                }
            }

            return allHoles;
        }
    }
}
