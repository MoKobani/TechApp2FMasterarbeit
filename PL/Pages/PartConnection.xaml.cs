using BLL.Models;
using BLL.Services;
using PL.Windows;
using System.Configuration;
using System.Data;
using System.IO;
using System.Windows;
using System.Windows.Controls;


namespace PL.Pages
{
    /// <summary>
    /// Interaction logic for AllUsers.xaml
    /// </summary>
    public partial class PartConnection : Page
    {
        // Dienste und Modelle als Felder, damit sie mehrfach verwendet werden können
        private readonly PartService _partService = new();
        private PartModel _part = new();
        private readonly AppSettings? _appsettings = null;
        private BomService _bomService = new();
        private  string? _articleTemplatePath = null;
        private  string? _processTemplatePath = null;
        private string? _surfaceTemplatePath = null;



        public PartConnection()
        {
            try
            {
                InitializeComponent();
                this.DataContext = _part;
                _appsettings = ConfigurationManager.GetSection("AppSettings") as AppSettings;
                _articleTemplatePath = Path.Combine(_appsettings.TamplatesPath, "ArtikelVorlagen.xlsx");
                _processTemplatePath = Path.Combine(_appsettings.TamplatesPath, "WeiterverarbeitungVorlagen.xlsx");
                _surfaceTemplatePath = Path.Combine(_appsettings.TamplatesPath, "Oberflächen.xlsx");
                cbxPartType.ItemsSource = _bomService.GetSheetsNames(_articleTemplatePath);
                cbxProcess.ItemsSource = _bomService.GetSheetsNames(_processTemplatePath);
                cbxSurface.ItemsSource = _bomService.GetSheetsNames(_surfaceTemplatePath);
            }
            catch (Exception ex)
            {

                MessageBox.Show("Fehler: " + ex.Message);
            }

        }

        private void btnLoad_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                _part = _partService.GetPart();
                this.DataContext = _part;

                if (_part.BomTemplate is DataTable bomTable)
                {
                    dgBom.ItemsSource = bomTable.DefaultView;
                }
                else
                {
                    dgBom.ItemsSource = null;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Fehler: " + ex.Message);
            }

        }

        // Nur zum Testen
        private void BtnSave_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (_part.Parttype == "Bauteilart" || _part.StorageLocation == "Lagerplatz")
                {
                    MessageBox.Show("Bitte Bauteilart und Lagerplatz ändern.");
                    return;
                }
                _partService.BuildPayloadData(_part , true);
                MessageBox.Show("Daten wurden gespeichert.");

            }
            catch (Exception ex)
            {
                MessageBox.Show("Fehler: " + ex.Message);
            }
        }

        private void BtnLoadBom_Click(object sender, RoutedEventArgs e)
        {
            try
            {

                DataTable bomTemplate = _bomService.GetBomTemplate(_part.Parttype, _articleTemplatePath);
                _part.BomTemplate = bomTemplate;
                dgBom.ItemsSource = _part.BomTemplate.DefaultView;

            }
            catch (Exception ex)
            {

                MessageBox.Show("Fehler: " + ex.Message);
            }
        }

        private async void BtnSend_Click(object sender, RoutedEventArgs e)
        {
            if (_part.Parttype == "Bauteilart" || _part.StorageLocation == "Lagerplatz")
            {
                MessageBox.Show("Bitte Bauteilart und Lagerplatz ändern.");
                return;
            }

            try
            {
                NetworkService networkService = new NetworkService();
                var data = _partService.BuildPayloadData(_part, true);
                //var payload = networkService.BuildPayload(data);

                string serverIp = _appsettings.ServerIP;
                int port = _appsettings.PortNumber;
                string dbDescriptor = _appsettings.DbDescriptor;
                string user = _appsettings.User;

                var msg = networkService.BuildPayload(data, user);            // baut Tokens mit RS
                var result = await networkService.SendDataAsync(msg, serverIp, port, dbDescriptor);   // macht Handshake + sendet Frame

                var successText = string.IsNullOrWhiteSpace(result.Message)
                    ? "Ok"
                    : $"Ok: {result.Message}";

                MessageBox.Show(result.Success ? successText : $"Fehler: {result.Message}");
                
            }
            catch (Exception ex)
            {
                MessageBox.Show("Fehler: " + ex.Message);
            }
        }

        private void BtnProg_Click(object sender, RoutedEventArgs e)
        {
            BtnSend_Click(sender, e);

            var partcopy = _part.Clone();
            var processingWindow = new ProcessingWindow(partcopy)
            {
                Owner = Window.GetWindow(this),
            };

            DataTable bomTemplate = _bomService.GetBomTemplate(cbxProcess.Text, _processTemplatePath);
            DataRow? row = bomTemplate.AsEnumerable().FirstOrDefault(r => (string)r["Sachnummer"] == "Rohartikel");

            partcopy.BomTemplate = bomTemplate;
            if (row != null)
            {
                row["Sachnummer"] = _part.Articlenumber;
            }

            processingWindow.dgBom.ItemsSource = bomTemplate.DefaultView;
            processingWindow.ShowDialog();
        }
    }
}
