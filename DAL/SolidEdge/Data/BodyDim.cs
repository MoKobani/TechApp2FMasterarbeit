using SolidEdgeGeometry;
using SolidEdgePart;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DAL.SolidEdge.Data
{
    public class BodyDim
    {
        private readonly Model _model;

        public double Length { get; private set; }
        public double Width { get; private set; }
        public double Height { get; private set; }



        public BodyDim(Model model)
        {
            _model = model ?? throw new ArgumentNullException("Kein Volumenkörper vorhanden!");

            InitDimension(model);
        }

        private void InitDimension(Model model)
        {
            try
            {
                Body body = (Body)model.Body;

                // Arrays für die Min- und Max-Koordinaten vorbereiten
                Array minRange = Array.CreateInstance(typeof(double), 3);
                Array maxRange = Array.CreateInstance(typeof(double), 3);
                body.GetExactRange(ref minRange, ref maxRange);

                // Berechnung der Abmessungen in mm (Standard ist Meter)
                Length = Math.Round(((double)maxRange.GetValue(0)! - (double)minRange.GetValue(0)!) * 1000, 2); //X-Richtung
                Width = Math.Round(((double)maxRange.GetValue(1)! - (double)minRange.GetValue(1)!) * 1000, 2); //Y-Richtung
                Height = Math.Round(((double)maxRange.GetValue(2)! - (double)minRange.GetValue(2)!) * 1000, 2); //Z-Richtung
            }
            catch (Exception ex)
            {

                throw new InvalidOperationException("Ein Fehler ist bei der Ermittlung der Hauptabmesseungen aufgetretten " + ex.Message);
            }
        }




    }
}
