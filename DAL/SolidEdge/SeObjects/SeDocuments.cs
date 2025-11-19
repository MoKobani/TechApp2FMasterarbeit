using SolidEdgeAssembly;
using SolidEdgeFramework;
using SolidEdgePart;
using System;

namespace DAL.SolidEdge.SeObjects
{
    /// <summary>
    /// Kapselt den Zugriff auf das aktuell aktive Solid Edge Dokument.
    /// </summary>
    public class SeDocuments
    {
        private readonly Application _app = null!;

        /// <summary>
        /// Erstellt eine neue Instanz und übernimmt die Solid Edge Application.
        /// </summary>
        /// <param name="app">Instanz der Solid Edge Anwendung (COM).</param>
        public SeDocuments(Application app)
        {
            _app = app;

        }
        /// <summary>
        /// Liefert das aktuell aktive Solid Edge Dokument (Part- oder SheetMetal-Dokument).
        /// </summary>
        /// <returns>
        /// Das aktive <see cref="SolidEdgeDocument"/> oder <c>null</c>, wenn keines vorhanden ist.
        /// </returns>
        public SolidEdgeDocument GetActiveDocument()
        {

            try
            {
                var document = (SolidEdgeDocument)_app.ActiveDocument;
                return document;
            }
            catch (Exception)
            {
                throw new InvalidOperationException("Kein aktives Dokument geöffnet.");
            }


        }
    }
}