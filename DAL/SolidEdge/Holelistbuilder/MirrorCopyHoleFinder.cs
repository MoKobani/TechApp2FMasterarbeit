using SolidEdgePart;
using System;
using System.Collections.Generic;
using DAL.SolidEdge.Assest;

namespace DAL.SolidEdge.HoleListBuilder
{
    /// <summary>
    /// Findet Bohrungen, die durch Spiegelungs-Features erzeugt wurden (inkl. Kombination mit Pattern).
    /// </summary>
    public class MirrorCopyHoleFinder : IHoleFinder
    {
        private readonly Model _model;

        /// <summary>
        /// Erstellt den Finder für ein gegebenes Modell.
        /// </summary>
        /// <param name="model">Quellmodell mit Features.</param>
        public MirrorCopyHoleFinder(Model model)
        {
            _model = model;
        }

        /// <summary>
        /// Ermittelt alle durch MirrorCopy erzeugten Bohrungen.
        /// </summary>
        /// <returns>Liste gefundener <see cref="Hole"/>-Objekte (nie <c>null</c>).</returns>
        public List<Hole> GetHoles()
        {
            var result = new List<Hole>();

            try
            {
                var mirrorCopies = _model?.MirrorCopies;
                if (mirrorCopies == null || mirrorCopies.Count == 0)
                    return result;

                foreach (MirrorCopy mc in mirrorCopies)
                {
                    if (mc.Suppress)
                        continue;

                    var inputs = Array.CreateInstance(typeof(object), Math.Max(0, mc.NumberInputFeatures));
                    mc.GetInputFeatures(ref inputs);

                    for (int i = 0; i < inputs.Length; i++)
                    {
                        var input = inputs.GetValue(i);

                        if (input is Pattern pat)
                        {
                            if (pat.Suppress)
                                continue;

                            int occ = Math.Max(1, pat.NumberOfOccurrences); // alle Vorkommen werden gespiegelt
                            var patInputs = Array.CreateInstance(typeof(object), Math.Max(0, pat.NumberOfInputFeatures));
                            pat.GetInputFeatures(ref patInputs);

                            for (int p = 0; p < patInputs.Length; p++)
                            {
                                var patInput = patInputs.GetValue(p);

                                foreach (var leaf in FeatureExpander.Flatten(patInput!))
                                {
                                    if (leaf is Hole hole && !hole.Suppress)
                                    {
                                        // Spiegel erzeugt 1 zusätzliche Instanz je vorkommender Bohrung
                                        for (int c = 1; c < occ; c++)
                                            result.Add(hole);
                                    }
                                }
                            }
                        }
                        else
                        {
                            // Normale Eingaben (Hole, UDP, …) werden 1× gespiegelt
                            foreach (var leaf in FeatureExpander.Flatten(input!))
                            {
                                if (leaf is Hole hole && !hole.Suppress)
                                    result.Add(hole);
                            }
                        }
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine("MirrorCopyHoleFinder: " + e.Message);
            }

            return result;
        }
    }
}
