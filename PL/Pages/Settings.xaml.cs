using DAL.SolidEdge.Connection;
using DAL.SolidEdge.Data;
using DAL.SolidEdge.HoleListBuilder;
using DAL.SolidEdge.SeObjects;
using System;
using System.Collections.Generic;
using System.Configuration;
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
using System.Windows.Shapes;

namespace PL.Pages
{
    /// <summary>
    /// Interaktionslogik für About.xaml
    /// </summary>
    public partial class Settings : Page
    {
        Configuration AppConfig = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);


        public Settings()
        {
            InitializeComponent();
            var AppSettingSection = AppConfig.GetSection("AppSettings");
            this.DataContext = AppSettingSection;
        }

        private void BtnSave_Click(object sender, RoutedEventArgs e)
        {
            AppConfig.Save(ConfigurationSaveMode.Modified);
            MessageBox.Show("Einstellungen wurden gespeichert bitte App neustarten");


        }
    }
}

