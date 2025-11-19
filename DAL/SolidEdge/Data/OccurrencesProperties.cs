using SolidEdgeAssembly;
using SolidEdgeFramework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace DAL.SolidEdge.Data
{
    public class OccurrencesProperties
    {
        private readonly SolidEdgeDocument _activeDocument;
        private AssemblyDocument _assemblyDocument;
        private Occurrences _occurrences;

        public OccurrencesProperties(SolidEdgeDocument activeDocument)
        {
            _activeDocument = activeDocument;
            _assemblyDocument = activeDocument as AssemblyDocument ?? throw new InvalidOperationException("Das aktive Dokument ist kein Baugruppendokument.");
            _occurrences = _assemblyDocument.Occurrences ?? throw new InvalidCastException("Das aktive Dokument ist kein Baugruppendokument.");

        }


        public List<AsmOccurrence> GetOccurrences()
        {
            List<AsmOccurrence> partsList = new List<AsmOccurrence>();

            foreach (Occurrence occurrence in _occurrences)
            {
                var partDoc = occurrence.OccurrenceDocument as SolidEdgeFramework.SolidEdgeDocument;
                if (partDoc == null) continue;
                var partProperties = new PartProperties(partDoc);

                FaceStyle faceStyle = occurrence.FaceStyle;
                string faceStyleName = "";
                //string articlenumber = occurrence.Name.LastIndexOf('.') > 0 ?
                //    occurrence.Name.Substring(0, occurrence.Name.LastIndexOf('.')) : occurrence.Name;

                if (faceStyle != null)
                {
                    faceStyleName = faceStyle.StyleName;
                }

                AsmOccurrence part = new AsmOccurrence
                {
                    OccurrenceName = occurrence.Name,
                    //Length = partProperties.GetProperty("Custom", "Länge"),
                    //Width = partProperties.GetProperty("Custom", "Breite"),
                    //Hight = partProperties.GetProperty("Custom", "Höhe"),
                    Discription = partProperties.GetProperty("Custom", "Beschreibung"),
                    Facestyle = faceStyleName
                };
                partsList.Add(part);
            }
            return partsList;

        }
    }
}