using DocumentFormat.OpenXml.Drawing.Charts;
using DocumentFormat.OpenXml.Office2010.ExcelAc;
using DocumentFormat.OpenXml.Spreadsheet;
using SolidEdgeAssembly;
using SolidEdgeConstants;
using SolidEdgeFramework;
using SolidEdgeFrameworkSupport;
using SolidEdgePart;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.ConstrainedExecution;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace DAL.SolidEdge.Data
{
    public class PartView
    {
        private readonly Window _window = null!;
        private readonly SolidEdgeDocument _activeDocument;
        private View _view = null!;




        public PartView( Window window, SolidEdgeDocument aktiveDoc)
        {
            _window = window;
            _activeDocument = aktiveDoc;
            _view = window.View;
        }

        public string SaveAsImage()
        {

            try
            {
                double resolution = 1;
                int colorDepth = 24;
                int width = _window.UsableWidth;
                int height = _window.UsableHeight;

                _view.ApplyNamedView("iso"); // Die iso Anschit holen
                _view.Fit(); // Fit

                string tempFile = System.IO.Path.ChangeExtension(System.IO.Path.GetTempFileName(), ".jpg");

                try
                {
                    _view.SaveAsImage
                    (
                        Filename: tempFile,
                        Width: width,
                        Height: height,
                        AltViewStyle: null,
                        Resolution: resolution,
                        ColorDepth: colorDepth,
                        ImageQuality: SolidEdgeFramework.SeImageQualityType.seImageQualityHigh,
                        Invert: false
                    );

                    return tempFile;

                }
                catch (Exception ex)
                {

                    throw new InvalidOperationException("Ein Fehler ist bei der Bildgenerierung aufgetretten " + ex.Message);
                }
            }

            catch (Exception ex)
            {

                throw new InvalidOperationException(ex.Message);
            }

        }

        public (double x, double y, double z) GetModelDim()
        {
            SolidEdgeDocument aktiveDocument = _activeDocument;
            PMI pmi = null!;

            if (aktiveDocument is PartDocument partDoc)
            {
                pmi = partDoc.PMI;
            }
            else if (aktiveDocument is SheetMetalDocument sheetDoc)
            {
                pmi = sheetDoc.PMI;
            }
            else if (aktiveDocument is AssemblyDocument asmDoc)
            {
                pmi = asmDoc.PMI;
                pmi.Show = false;
                asmDoc.UpdateAll();
            }

            try
            {
                if (pmi.Dimensions.Count > 0)
                {
                    pmi.ShowDimensions = false;  // Ausblenen, damit Bounding Box richtig ermittelt wird.
                }
 

                // Bounding Box
                _view.GetModelRange(out double minx, out double miny, out double minz, out double maxx, out double maxy, out double maxz);

                return (Math.Round((maxx - minx) * 1000, 2), Math.Round((maxy - miny) * 1000, 2), Math.Round((maxz - minz) * 1000, 2));
            }

            catch (Exception ex)
            {
                throw new InvalidOperationException("Ein Fehler ist bei der Berechnung der Hauptabmessungen aufgetretten " + ex.Message);
            }

            finally
            {
                if (pmi.Dimensions.Count > 0)
                {
                    pmi.ShowDimensions = true;
                }
            }
        }
    }
}
