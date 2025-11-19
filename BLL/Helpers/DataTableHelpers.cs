using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DAL.SolidEdge.Data;
namespace BLL.Helpers
{
    public class DataTableHelpers
    {
        // Nimmt eine List<Part> und baut eine DataTable mit denselben Spaltennamen
        public static DataTable ToDataTable(List<AsmOccurrence> parts)
        {
            var table = new DataTable();

            // Spalten anlegen (alle string in deinem Beispiel)
            table.Columns.Add("Articlelnumber"); // Achtung: Schreibweise wie in deiner Klasse
            table.Columns.Add("Length");
            table.Columns.Add("Width");
            table.Columns.Add("Hight");
            table.Columns.Add("Discription");

            // Zeilen füllen
            foreach (var p in parts)
            {
                // Reihenfolge der Werte = Reihenfolge der Spalten oben
                table.Rows.Add(p.OccurrenceName, p.Length, p.Width, p.Hight, p.Discription);
            }

            return table;
        }
    }
}