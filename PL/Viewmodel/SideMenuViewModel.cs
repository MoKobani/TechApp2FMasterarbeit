using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;

namespace PL.ViewModel
{
    internal class SideMenuViewModel
    {
        //to call resource dictionary in our code behind
        ResourceDictionary dict = Application.LoadComponent(new Uri("/TechApp2F;component/Assets/IconDictionary.xaml", UriKind.RelativeOrAbsolute)) as ResourceDictionary;


        //Our Source List for Menu Items
        public List<MenuItemsData> MenuList
        {
            get
            {
                return new List<MenuItemsData>
                {
                    //MainMenu Button
                    new MenuItemsData(){ PathData= (PathGeometry)dict["icon_SeAbasConnction"], MenuText="Solid Edge Abas Connection"
                    
                    //SubMenu Button
                    , SubMenuList=new List<SubMenuItemsData>{
                    new SubMenuItemsData(){ PathData=(PathGeometry)dict["icon_PartConnction"], SubMenuText="Part Connection" },
                    new SubMenuItemsData(){ PathData=(PathGeometry)dict["icon_AssemblyConnction"], SubMenuText="Assembly Connection" }}
                    },

                    //MainMenu Button
                    new MenuItemsData(){ PathData= (PathGeometry)dict["icon_DesignAutomation"], MenuText="Design Automation"

                    //SubMenu Button
                    , SubMenuList=new List<SubMenuItemsData>{
                    new SubMenuItemsData(){ PathData=(PathGeometry)dict["icon_Lampshade"], SubMenuText="Lampshade Configurator" },
                    new SubMenuItemsData(){ PathData=(PathGeometry)dict["icon_WireRack"], SubMenuText="Wire Rack Configurator" },
                    new SubMenuItemsData(){ PathData=(PathGeometry)dict["icon_LEDProfiles"], SubMenuText="LED Profiles Configurator" }}},

                    //MainMenu without SubMenu Button
                    new MenuItemsData(){ PathData= (PathGeometry)dict["icon_About"], MenuText="About"},

                    new MenuItemsData(){ PathData= (PathGeometry)dict["icon_Settings"], MenuText="Settings" }
                };
            }
        }
    }

    public class MenuItemsData
    {
        //Icon Data
        public PathGeometry PathData { get; set; }
        public string MenuText { get; set; }
        public List<SubMenuItemsData> SubMenuList { get; set; }

        //To Add click event to our Buttons we will use ICommand here to switch pages when specific button is clicked
        public MenuItemsData()
        {
            Command = new CommandViewModel(Execute);
        }

        public ICommand Command { get; }

        private void Execute()
        {
            //our logic comes here
            string MT = MenuText.Replace(" ", string.Empty);
            if (!string.IsNullOrEmpty(MT))
                navigateToPage(MT);
        }

        private void navigateToPage(string Menu)
        {
            //We will search for our Main Window in open windows and then will access the frame inside it to set the navigation to desired page..
            //lets see how... ;)
            foreach (Window window in Application.Current.Windows)
            {
                if (window.GetType() == typeof(MainWindow))
                {
                    (window as MainWindow).MainWindowFrame.Navigate(new Uri(string.Format("{0}{1}{2}", "Pages/", Menu, ".xaml"), UriKind.RelativeOrAbsolute));
                }
            }
        }
    }
    public class SubMenuItemsData
    {
        public PathGeometry PathData { get; set; }
        public string SubMenuText { get; set; }

        //To Add click event to our Buttons we will use ICommand here to switch pages when specific button is clicked
        public SubMenuItemsData()
        {
            SubMenuCommand = new CommandViewModel(Execute);
        }

        public ICommand SubMenuCommand { get; }

        private void Execute()
        {
            //our logic comes here
            string SMT = SubMenuText.Replace(" ", string.Empty);
            if (!string.IsNullOrEmpty(SMT))
                navigateToPage(SMT);
        }

        private void navigateToPage(string Menu)
        {
            //We will search for our Main Window in open windows and then will access the frame inside it to set the navigation to desired page..
            //lets see how... ;)
            foreach (Window window in Application.Current.Windows)
            {
                if (window.GetType() == typeof(MainWindow))
                {
                    (window as MainWindow).MainWindowFrame.Navigate(new Uri(string.Format("{0}{1}{2}", "Pages/", Menu, ".xaml"), UriKind.RelativeOrAbsolute));
                }
            }
        }
    }
}