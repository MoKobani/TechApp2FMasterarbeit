using DocumentFormat.OpenXml.Wordprocessing;
using SolidEdgeFramework;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Xml.Linq;

namespace DAL.SolidEdge.Data
{
    public class PartProperties
    {
        private readonly SolidEdgeDocument _activeDocument;
        private PropertySets? _propertySets;

        public string Material
        {
            get
            {
                if (string.IsNullOrEmpty(GetProperty(propertySetName: "MechanicalModeling", propertyName: "Material")))
                    return "Material";
                else
                    return GetProperty(propertySetName: "MechanicalModeling", propertyName: "Material");
            }

        }
        public string ProcurementType
        {
            get
            {
                if (string.IsNullOrEmpty(GetProperty(propertySetName: "Custom", propertyName: "Beschaffungsart")))
                    return "Fremdbeschaffung";
                else
                    return GetProperty(propertySetName: "Custom", propertyName: "Beschaffungsart");
            }
        }

        public string StorageLocation
        {
            get
            {
                return GetProperty(propertySetName: "Custom", propertyName: "Lagerplatz");
            }
        }

        public string Parttype
        {
            get
            {
                return GetProperty(propertySetName: "Custom", propertyName: "Bauteilart");
            }
        }

        public string Supplier
        {
            get
            {
                return GetProperty(propertySetName: "Custom", propertyName: "Lieferant");

            }
        }

        public string StorageUnit
        {
            get
            {
                if (string.IsNullOrEmpty(GetProperty(propertySetName: "Custom", propertyName: "Lagereinheit")))
                    return "Stück";
                else
                    return GetProperty(propertySetName: "Custom", propertyName: "Lagereinheit");
            }
        }


        public string Articlenumber
        {

            get 
            {
                if (string.IsNullOrEmpty(GetProperty(propertySetName: "Custom", propertyName: "Artikelnummer")))
                {
                    string articlenumber = System.IO.Path.GetFileNameWithoutExtension(_activeDocument.Name);

                    if (articlenumber.StartsWith("z", StringComparison.OrdinalIgnoreCase))
                    {
                        // Nur den Namen selbst zurückgeben
                        return articlenumber;
                    }
                    else
                    {
                        // Dateiname ohne Extension und Präfix "I9"
                        return "I9" + articlenumber;
                    }

                }
                else
                    return GetProperty(propertySetName: "Custom", propertyName: "Artikelnummer");

            } 
        }



        public double MaterialThickness
        {
            get
            {
                string materilaThickness = GetProperty(propertySetName: "Custom", propertyName: "Materialstärke");

                // " mm" wegtrimmen
                string cleaned = materilaThickness.Replace("mm", "", StringComparison.OrdinalIgnoreCase).Trim();

                // Versuch: String -> double (mit deutscher Kultur, weil dort Komma als Dezimalzeichen gilt)
                if (double.TryParse(cleaned, NumberStyles.Float, CultureInfo.GetCultureInfo("de-DE"), out double value))
                {
                    return value;
                }

                // Falls Parsen fehlschlägt -> 0 zurückgeben
                return 0;
            }
        }


        public PartProperties(SolidEdgeDocument activeDocument)
        {
            _activeDocument = activeDocument?? throw new ArgumentNullException(nameof(activeDocument));
            _propertySets = activeDocument.Properties ?? throw new InvalidOperationException("Das Dokument hat keine Eigenschaften.");

        }


        public string GetProperty(string propertySetName, string propertyName)
        {
            Properties properties = _propertySets!.Item(propertySetName) ?? throw new InvalidOperationException($"Das Dokument hat keine Eigenschaften der Gruppe '{propertySetName}'.");

            dynamic property = null!;  // Using dynamic so that property.Value works.

            System.Runtime.InteropServices.VarEnum nativePropertyType = System.Runtime.InteropServices.VarEnum.VT_EMPTY;
            //Type runtimePropertyType = null;

            object value = null!;

            try
            {
                property = properties.Item(propertyName);
                nativePropertyType = (System.Runtime.InteropServices.VarEnum)property.Type;

                // May throw an exception...
                value = property.Value;
                if (value.ToString() == string.Empty)
                {
                    value = "";
                }

                return value.ToString()!;

            }
            catch (Exception)
            {
                //throw new InvalidOperationException($"Fehler beim Auslesen der Eigenschaft: {PropertyName}!!{System.Environment.NewLine}{ex.Message}",ex);
                return "";

            }
            //return "";
        }

        public void SetCustomProperty(string propertyName, object value)
        {
            try
            {
                Properties properties = _propertySets!.Item("Custom");
                properties.Add($"{propertyName}", $"{value}");
            }
            catch (Exception ex)
            {

                throw new InvalidOperationException($"Fehler beim Setzen der Eigenschaft: {propertyName}!!{System.Environment.NewLine}{ex.Message}", ex);
            }
        }

        public (Dictionary<string, string> BomEntries, string? Department) GetBomCustomProperties()
        {
            Dictionary<string, string> bomEntries = new(StringComparer.Ordinal);
            string? department = null;

            try
            {
                Properties properties = _propertySets!.Item("Custom");

                HashSet<string> baseFields = new(StringComparer.OrdinalIgnoreCase)
                {
                    "Artikelnummer",
                    "Bauteilart",
                    "Länge",
                    "Breite",
                    "Höhe",
                    "Gewicht",
                    "Lagereinheit",
                    "Beschreibung",
                    "Lagerplatz",
                    "Dateipfad",
                    "JPG-Pfad",
                    "Lieferant",
                    "Beschaffungsart",
                    "Materialstärke",
                    "Neutralfaktor",
                    "Ausklinkungslänge",
                    "Ausklinkungsbreite",
                    "Biegeradius",
                    "Materialstärke",
                    "Minimale Bogenlänge",
                    "Abweichungstoleranz",
                    "Dichte",
                    "Genauigkeit",
                    "Oberfläche",


                };

                foreach (dynamic property in properties)
                {
                    string name;
                    string value;

                    try
                    {
                        name = property.Name;
                        object rawValue = property.Value;
                        value = rawValue?.ToString() ?? string.Empty;
                    }
                    catch
                    {
                        continue;
                    }

                    if (string.IsNullOrWhiteSpace(name))
                        continue;

                    if (baseFields.Contains(name) ||
                        string.Equals(name, "Stückliste Anfang", StringComparison.OrdinalIgnoreCase) ||
                        string.Equals(name, "Stückliste Ende", StringComparison.OrdinalIgnoreCase))
                    {
                        continue;
                    }

                    if (string.Equals(name, "Abteilung", StringComparison.OrdinalIgnoreCase))
                    {
                        if (!string.IsNullOrWhiteSpace(value))
                        {
                            department = value.Trim();
                        }

                        continue;
                    }

                    bomEntries[name] = value.Trim();
                }
            }
            catch
            {
                // Wenn keine benutzerdefinierten Eigenschaften vorhanden sind, einfach leeres Ergebnis zurückgeben.
            }

            return (bomEntries, department);
        }

        public void DeleteCustomProperties()
        {
            try
            {
                Properties properties = _propertySets!.Item("Custom");

                System.Runtime.InteropServices.VarEnum nativePropertyType = System.Runtime.InteropServices.VarEnum.VT_EMPTY;
                foreach (Property property in properties)
                {
                    nativePropertyType = (System.Runtime.InteropServices.VarEnum)property.Type;
                    try
                    {
                        property.Delete();

                    }
                    catch
                    {
                        continue;
                    }

                }
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Fehler beim Löschen der BDE!!{System.Environment.NewLine}{ex.Message}", ex);

            }

        }
    }
}