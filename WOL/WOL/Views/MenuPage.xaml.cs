using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WOL.Models;
using WOL.ViewModels;
using Xamarin.Forms;

namespace WOL.Views
{
    public partial class MenuPage : ContentPage
    {
        RootPage root;
        List<HomeMenuItem> menuItems;
        public MenuPage(RootPage root)
        {

            this.root = root;
            InitializeComponent();
            if (!App.IsWindows10)
            {
                BackgroundColor = Color.FromHex("#03A9F4");
                ListViewMenu.BackgroundColor = Color.FromHex("#F5F5F5");
            }
            BindingContext = new BaseViewModel
            {
                Title = "Wake On Lan",
                Subtitle = "Wake your devices up",
                Icon = "system.png"
            };

            ListViewMenu.ItemsSource = menuItems = new List<HomeMenuItem>
                {
                    new HomeMenuItem { Title = "About", MenuType = MenuType.About, Icon ="about.png" },
                    new HomeMenuItem { Title = "Devices Manager", MenuType = MenuType.Devices, Icon = "device.png" },                    
                };

            ListViewMenu.SelectedItem = menuItems[0];

            ListViewMenu.ItemSelected += async (sender, e) =>
            {
                if (ListViewMenu.SelectedItem == null)
                    return;

                await this.root.NavigateAsync(((HomeMenuItem)e.SelectedItem).MenuType);
            };
        }
    }
}
