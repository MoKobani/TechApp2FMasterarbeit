using SolidEdgePart;
using System;
using System.Collections.Generic;

namespace DAL.SolidEdge.Assest
{
    /// <summary>
    /// Hilfsklasse zum Traversieren verschachtelter Feature-Strukturen (Pattern, MirrorCopy, UDP) und
    /// zum Ausgeben der „Blatt“-Elemente (z. B. <see cref="Hole"/>).
    /// </summary>
    public static class FeatureExpander
    {
        /// <summary>
        /// Durchläuft iterativ alle Eingangsfeatures ab <paramref name="root"/> und liefert jedes Blatt-Objekt.
        /// </summary>
        /// <param name="root">Startobjekt (z. B. Pattern, MirrorCopy, UserDefinedPattern oder ein Feature).</param>
        /// <returns>Aufzählung aller Blatt-Objekte (z. B. <see cref="Hole"/>).</returns>
        public static IEnumerable<object> Flatten(object root)
        {
            if (root == null) yield break;

            var stack = new Stack<object>();
            stack.Push(root);

            while (stack.Count > 0)
            {
                var current = stack.Pop();

                switch (current)
                {
                    case MirrorCopy mc:
                        // unterdrückte Spiegelung: komplett überspringen
                        if (mc.Suppress) continue;

                        var mcInputs = Array.CreateInstance(typeof(object), mc.NumberInputFeatures);
                        mc.GetInputFeatures(ref mcInputs);
                        PushAll(mcInputs, stack);
                        break;

                    case Pattern p:
                        // unterdrücktes Pattern: komplett überspringen
                        if (p.Suppress) continue;

                        var pInputs = Array.CreateInstance(typeof(object), p.NumberOfInputFeatures);
                        p.GetInputFeatures(ref pInputs);
                        PushAll(pInputs, stack);
                        break;

                    case UserDefinedPattern udp:
                        // unterdrücktes UDP: komplett überspringen
                        if (udp.Suppress) continue;

                        var udpInputs = Array.CreateInstance(typeof(object), udp.NumberInputFeatures);
                        udp.GetInputFeatures(ref udpInputs);
                        PushAll(udpInputs, stack);
                        break;

                    default:
                        // Blatt-Feature (z. B. Hole)
                        yield return current;
                        break;
                }
            }
        }
        /// <summary>
        /// Legt alle Elemente eines Eingabearrays auf den Traversierungs-Stack.
        /// </summary>
        /// <param name="inputs">Array der Eingangsfeatures.</param>
        /// <param name="stack">Stack zur iterativen Traversierung.</param>
        private static void PushAll(Array inputs, Stack<object> stack)
        {
            if (inputs == null) return;
            for (int i = 0; i < inputs.Length; i++)
            {
                var obj = inputs.GetValue(i);
                if (obj != null) stack.Push(obj);
            }
        }
    }

}
