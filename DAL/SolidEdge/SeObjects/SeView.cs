using SolidEdgeFramework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DAL.SolidEdge.SeObjects
{
    public class SeView
    {
        private readonly Application _app = null!;


        public SeView(Application app)
        {
            _app = app;

        }

        public Window GetAktiveView()
        {

            try
            {
                var window = _app.ActiveWindow as Window;
                return window;
            }
            catch (Exception)
            {
                throw new InvalidOperationException("Keine aktive Ansicht vorhanden.");
            }


        }
    }
}
