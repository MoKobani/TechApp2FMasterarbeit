using SolidEdgePart;
using System;
using System.Collections.Generic;
using DAL.SolidEdge.Assest;

namespace DAL.SolidEdge.HoleListBuilder
{
    /// <summary>
    /// Findet Bohrungen, die durch Muster-Features erzeugt wurden.
    /// </summary>
    public class PatternHoleFinder : IHoleFinder
    {
        private readonly Model _model;

        /// <summary>
        /// Erstellt den Finder für ein gegebenes Modell.
        /// </summary>
        /// <param name="model">Quellmodell mit Features.</param>
        public PatternHoleFinder(Model model)
        {
            _model = model;
        }

        /// <summary>
        /// Ermittelt Bohrungen, die durch Muster entstehen (ohne den Seed doppelt zu zählen).
        /// </summary>
        /// <returns>Liste gefundener <see cref="Hole"/>-Objekte (nie <c>null</c>).</returns>
        public List<Hole> GetHoles()
        {
            var result = new List<Hole>();

            try
            {
                var patterns = _model?.Patterns;
                if (patterns == null || patterns.Count == 0)
                    return result;

                foreach (Pattern pattern in patterns)
                {
                    if (pattern.Suppress)
                        continue;

                    int occ = Math.Max(1, pattern.NumberOfOccurrences); // inkl. Seed
                    var inputs = Array.CreateInstance(typeof(object), Math.Max(0, pattern.NumberOfInputFeatures));
                    pattern.GetInputFeatures(ref inputs);

                    for (int i = 0; i < inputs.Length; i++)
                    {
                        var input = inputs.GetValue(i);

                        // Alle „Blätter“ (z. B. Hole) unterhalb des Eingangs-Features finden
                        foreach (var leaf in FeatureExpander.Flatten(input!))
                        {
                            if (leaf is Hole hole && !hole.Suppress)
                            {
                                // Nur die Wiederholungen zählen (Seed nicht doppelt)
                                for (int k = 1; k < occ; k++)
                                    result.Add(hole);
                            }
                        }
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine("PatternHoleFinder: " + e.Message);
            }

            return result;
        }
    }
}
