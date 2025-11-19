using BLL.Models;
using BLL.Services;
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
using System.Windows.Navigation;
using System.IO;


namespace PL.Pages
{
    /// <summary>
    /// Interaction logic for NewUser.xaml
    /// </summary>
    public partial class AssemblyConnection : Page
    {
        private readonly AsmService _asmService = new();
        private AsmModel _asmModel = new();
        private readonly AppSettings? _appsettings = null;
        private BomService _bomService = new();
        private string? _asmTemplatePath = null;
        private string? _processTemplatePath = null;
        private string? _surfaceTemplatePath = null;


        public AssemblyConnection()
        {
            try
            {
                InitializeComponent();
                this.DataContext = _asmModel;
                _appsettings = ConfigurationManager.GetSection("AppSettings") as AppSettings;
                _asmTemplatePath = Path.Combine(_appsettings.TamplatesPath, "BaugruppenVorlagen.xlsx");
                _processTemplatePath = Path.Combine(_appsettings.TamplatesPath, "WeiterverarbeitungVorlagen.xlsx");
                _surfaceTemplatePath = Path.Combine(_appsettings.TamplatesPath, "Oberflächen.xlsx");
                cbxAsmType.ItemsSource = _bomService.GetSheetsNames(_asmTemplatePath);
                cbxProcess.ItemsSource = _bomService.GetSheetsNames(_processTemplatePath);
                cbxSurface.ItemsSource = _bomService.GetSheetsNames(_surfaceTemplatePath);
                this.DataContext = _asmModel;
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
                _asmModel = _asmService.GetAsm();
                this.DataContext = _asmModel;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Fehler: " + ex.Message);
            }

        }

        private async void btnSend_Click(object sender, RoutedEventArgs e)
        {
            if (_asmModel.Parttype == "Bauteilart" || _asmModel.StorageLocation == "Lagerplatz")
            {
                MessageBox.Show("Bitte Bauteilart und Lagerplatz ändern.");
                return;
            }

            try
            {


                NetworkService networkService = new NetworkService();
                var data = _asmService.BuildPayloadData(_asmModel, true);

                if (_appsettings is null)
                {
                    throw new InvalidOperationException("AppSettings konnten nicht geladen werden.");
                }

                string serverIp = _appsettings.ServerIP;
                int port = _appsettings.PortNumber;
                string dbDescriptor = _appsettings.DbDescriptor;
                string user = _appsettings.User;

                var msg = networkService.BuildPayload(data, user);
                var result = await networkService.SendDataAsync(msg, serverIp, port, dbDescriptor);

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

        private void BtnLoadBom_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                DataTable? template = null;
                if (!string.IsNullOrWhiteSpace(_asmTemplatePath))
                {
                    var selectedTemplateName = cbxAsmType.SelectedItem as string;
                    if (string.IsNullOrWhiteSpace(selectedTemplateName))
                    {
                        selectedTemplateName = cbxAsmType.Text;
                    }

                    if (!string.IsNullOrWhiteSpace(selectedTemplateName))
                    {
                        template = _bomService.GetBomTemplate(selectedTemplateName, _asmTemplatePath).Copy();
                    }
                }

                var bom = _asmService.ToDataTable(_asmModel.Occurrences, template);

                _asmModel.BomTemplate = bom;
                dgBom.ItemsSource = bom.DefaultView;

            }
            catch (Exception ex)
            {
                MessageBox.Show("Fehler: " + ex.Message);
            }
        }
    }

}