using DAL.Excel;
using DAL.SolidEdge.Connection;
using DAL.SolidEdge.HoleListBuilder;
using DAL.SolidEdge.SeObjects;
using DocumentFormat.OpenXml.Drawing.Charts;
using DocumentFormat.OpenXml.Wordprocessing;
using SolidEdgeAssembly;
using SolidEdgeFramework;
using SolidEdgeFrameworkSupport;
using SolidEdgePart;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;


namespace DAL.SolidEdge.Data
{

    public class AsmRepo
    {
        /// <inheritdoc />
        public (
            List<AsmOccurrence> Occurrences,
            string Articlenumber,
            string FilePath,
            double Mass,
            double length,
            double Width,
            double Height,
            string ImagePath,
            string StorageLocation,
            string Parttype,
            string Supplier,
            string SortageUnit,
            string ProcurementType



            )
            LoadAsm()
        {
            try
            {
                var seApp = SolidEdgeConnector.TryConnect(false);

                SeDocuments seDoc = new SeDocuments(seApp);
                var activedoc = seDoc.GetActiveDocument();
                var docpath = activedoc.FullName;
                OccurrencesProperties occurrencesProperties = new OccurrencesProperties(activedoc);
                List<AsmOccurrence> parts = occurrencesProperties.GetOccurrences();

                PartProperties partProperties = new PartProperties(activedoc!);
                var articlenumber = partProperties.Articlenumber;
                var storageLocation = partProperties.StorageLocation;
                var parttype = partProperties.Parttype;
                var supplier = partProperties.Supplier;
                var storageUnit = partProperties.StorageUnit;
                var procurementType = partProperties.ProcurementType;
                var (bomEntries, bomDepartment) = partProperties.GetBomCustomProperties();

                PartVariables partVariables = new PartVariables(activedoc);
                var mass = partVariables.Mass;

                SeView seView = new SeView(seApp);
                var window = seView.GetAktiveView();

                PartView partView = new PartView(window, activedoc);
                var dim = partView.GetModelDim();
                var length = dim.x;
                var width = dim.y;
                var height = dim.z;
                var imagePath = partView.SaveAsImage();



                return (parts, articlenumber, docpath, mass,length, width, height, imagePath, storageLocation, parttype, supplier, storageUnit, procurementType);

            }

            finally
            {
                SolidEdgeConnector.TryDisconnect();
            }

        }

        public void SaveAsm(Dictionary<string, object> data)
        {
            try
            {
                var seApp = SolidEdgeConnector.TryConnect(false);

                SeDocuments seDoc = new SeDocuments(seApp);
                var activedoc = seDoc.GetActiveDocument();


                PartProperties partProperties = new PartProperties(activedoc!);
                partProperties.DeleteCustomProperties();
                activedoc.Save();

                foreach (var obj in data)
                {
                    var propertyName = obj.Key;
                    dynamic PropertyValue = obj.Value;

                    partProperties.SetCustomProperty(propertyName, PropertyValue);
                }

                activedoc.Save();
            }

            finally
            {
                SolidEdgeConnector.TryDisconnect();
            }

        }
    }


}