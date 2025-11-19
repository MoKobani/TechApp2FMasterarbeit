using SolidEdgePart;
using System;
using System.Collections.Generic;

namespace DAL.SolidEdge.HoleListBuilder
{
    /// <summary>
    /// Findet Bohrungen aus User Defined Pattern (UDP).
    /// </summary>
    public class UserDefinedPatternHoleFinder : IHoleFinder
    {
        private readonly Model _model;

        /// <summary>
        /// Erstellt den Finder für ein gegebenes Modell.
        /// </summary>
        /// <param name="model">Quellmodell mit Features.</param>
        public UserDefinedPatternHoleFinder(Model model)
        {
            _model = model;
        }

        /// <summary>
        /// Ermittelt Bohrungen, die als Eingaben in UDPs verwendet werden (unterdrückte ausgenommen).
        /// </summary>
        /// <returns>Liste gefundener <see cref="Hole"/>-Objekte (nie <c>null</c>).</returns>
        public List<Hole> GetHoles()
        {
            var result = new List<Hole>();

            try
            {
                var userDefinedPatterns = _model?.UserDefinedPatterns;
                if (userDefinedPatterns == null || userDefinedPatterns.Count == 0)
                    return result;

                foreach (UserDefinedPattern pattern in userDefinedPatterns)
                {
                    // Unterdrückte UDPs überspringen
                    if (pattern.Suppress)
                        continue;

                    // Eingangsfeatures holen und prüfen
                    int n = Math.Max(0, pattern.NumberInputFeatures);
                    var inputFeatures = Array.CreateInstance(typeof(object), n);
                    pattern.GetInputFeatures(ref inputFeatures);

                    for (int i = 0; i < inputFeatures.Length; i++)
                    {
                        object obj = inputFeatures.GetValue(i)!;
                        if (obj is Hole hole && !hole.Suppress)
                        {
                            result.Add(hole);
                        }
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine("UserDefinedPatternHoleFinder: " + e.Message);
            }

            return result;
        }
    }
}
