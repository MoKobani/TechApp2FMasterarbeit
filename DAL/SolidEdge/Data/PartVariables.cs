using SolidEdgeFramework;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DAL.SolidEdge.Data
{
    internal class PartVariables
    {

        private readonly SolidEdgeDocument _activeDocument;
        private Variables _variables;

        public double Mass
        {
            get
            {
                return Math.Round((double)GetVariable("Mass"), 2);
            }
        }

        public PartVariables(SolidEdgeDocument activeDocument)
        {
            _activeDocument = activeDocument ?? throw new ArgumentNullException(nameof(activeDocument));
            _variables = _activeDocument.Variables ?? throw new ArgumentNullException(nameof(activeDocument.Variables));

        }

        private object GetVariable(string VariableName)
        {
            try
            {
                variable variable = _variables.Item(VariableName);

                object value = null!;

                value = variable.Value;

                return value;
            }
            catch (Exception ex)
            {

                throw new InvalidOperationException($"Fehler beim Lesen der Variable: {VariableName}!!{System.Environment.NewLine}{ex.Message}", ex);
            }

        }

    }
}