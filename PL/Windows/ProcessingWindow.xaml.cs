using BLL.Models;
using BLL.Services;
using PL.Pages;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.IO;

namespace PL.Windows
{
    /// <summary>
    /// Interaktionslogik für ProcessingWindow.xaml
    /// </summary>
    public partial class ProcessingWindow : Window 
    {
        private readonly PartService _partService = new();
        private PartModel _part = new();
        private readonly AppSettings? _appsettings = null;
        private BomService _bomService = new();
        private  string? _surfaceTemplatePath = null;
        private string? _processTemplatePath = null;


        public ProcessingWindow(PartModel part)
        {
            try
            {
                InitializeComponent();
                _part = part;
                this.DataContext = part;

                _appsettings = ConfigurationManager.GetSection("AppSettings") as AppSettings;
                _processTemplatePath = Path.Combine(_appsettings!.TamplatesPath, "WeiterverarbeitungVorlagen.xlsx");
                _surfaceTemplatePath = Path.Combine(_appsettings.TamplatesPath, "Oberflächen.xlsx");
                cbxProcess.ItemsSource = _bomService.GetSheetsNames(_processTemplatePath);
                cbxSurface.ItemsSource = _bomService.GetSheetsNames(_surfaceTemplatePath);
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

            DataTable bomTemplate = _bomService.GetBomTemplate(cbxProcess.Text, _processTemplatePath!);
            DataRow? row = bomTemplate.AsEnumerable().FirstOrDefault(r => (string)r["Sachnummer"] == "Rohartikel");

            partcopy.BomTemplate = bomTemplate;
            if (row != null)
            {
                row["Sachnummer"] = _part.Articlenumber;
            }
            processingWindow.dgBom.ItemsSource = bomTemplate.DefaultView;
            processingWindow.ShowDialog();

           
        }

        private async void BtnSend_Click(object sender, RoutedEventArgs e)
        {
            var partcopy = _part.Clone();

            try
            {
                NetworkService networkService = new NetworkService();
                var data = _partService.BuildPayloadData(partcopy, false);

                string serverIp = _appsettings!.ServerIP;
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
    }
}
