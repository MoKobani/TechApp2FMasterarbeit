using DAL.Excel;
using DAL.SolidEdge.Connection;
using DAL.SolidEdge.HoleListBuilder;
using DAL.SolidEdge.SeObjects;
using SolidEdgeFramework;
using SolidEdgeFrameworkSupport;
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
    /// <summary>
    /// Zugriffsschicht für Teileinformationen aus Solid Edge.
    /// Implementiert <see cref="IPartRepository"/>, sodass die BLL
    /// nur noch gegen dieses Interface programmiert werden muss.
    /// </summary>
    public class PartRepo /*: IPartRepository*/
    {
        /// <inheritdoc />
        public (
            string Articlenumber,
            string Holesummary,
            string FilePath,
            double Length,
            double Width,
            double Height,
            double MaterialThickness,
            string ImagePath,
            string Material,
            string StorageLocation,
            string Parttype,
            string Supplier,
            double Mass,
            string SortageUnit,
            string ProcurementType,
            Dictionary<string, string> BomEntries,
            string? BomDepartment

            )
            Loadpart()
        {
            try
            {
                var seApp = SolidEdgeConnector.TryConnect(false);

                SeDocuments seDoc = new SeDocuments(seApp);
                var activedoc = seDoc.GetActiveDocument();

                SeModels seModels = new SeModels(activedoc);
                var model = seModels.GetModel(1);
                var docpath = activedoc.FullName;
                var holesummary = HoleSummaryGenerator.Summarize(model!);

                BodyDim bodyDim = new BodyDim(model);
                var length = bodyDim.Length;
                var width = bodyDim.Width;
                var height = bodyDim.Height;


                PartProperties partProperties = new PartProperties(activedoc!);
                var materialname = partProperties.Material;
                var materialThickness = partProperties.MaterialThickness;
                var storageLocation = partProperties.StorageLocation;
                var parttype = partProperties.Parttype;
                var supplier = partProperties.Supplier;
                var storageUnit = partProperties.StorageUnit;
                var procurementType = partProperties.ProcurementType;
                var articlenumber = partProperties.Articlenumber;
                var (bomEntries, bomDepartment) = partProperties.GetBomCustomProperties();

                PartVariables partVariables = new PartVariables(activedoc);
                var mass = partVariables.Mass;
                
                SeView seView = new SeView(seApp);
                var window = seView.GetAktiveView();
                PartView partView = new PartView(window,activedoc);
                var imagePath = partView.SaveAsImage();

                return (articlenumber, holesummary, docpath, length, width, height, materialThickness, imagePath, materialname, storageLocation, parttype, supplier, mass, storageUnit, procurementType, bomEntries, bomDepartment);

            }

            finally
            {
                SolidEdgeConnector.TryDisconnect();
            }

        }

        /// <inheritdoc />
        public void SavePart(Dictionary<string, object> data)
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
