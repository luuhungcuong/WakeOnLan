using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WOL.Controls;
using WOL.Models;
using WOL.ViewModels;
using Xamarin.Forms;

namespace WOL.Views
{
    public class RootPage : MasterDetailPage
    {
        public static bool IsUWPDesktop { get; set; }
        Dictionary<MenuType, NavigationPage> Pages { get; set; }
        public RootPage()
        {

            Pages = new Dictionary<MenuType, NavigationPage>();
            Master = new MenuPage(this);
            BindingContext = new BaseViewModel
            {
                Title = "Wake On Lan",
                Icon = "slideout.png"
            };
            //setup home page
            //Task.Run(async () =>
            //{
                NavigateAsync(MenuType.Devices);
            //});            
            InvalidateMeasure();
        }



        public async Task NavigateAsync(MenuType id)
        {

            if (Detail != null)
            {
                if (IsUWPDesktop || Device.Idiom != TargetIdiom.Tablet)
                    IsPresented = false;

                if (Device.OS == TargetPlatform.Android)
                    await Task.Delay(300);
            }

            Page newPage;
            if (!Pages.ContainsKey(id))
            {

                switch (id)
                {
                    case MenuType.About:
                        Pages.Add(id, new MenuNavigationPage(new AboutPage()));
                        break;
                    case MenuType.Devices:
                        Pages.Add(id, new MenuNavigationPage(new DevicesPage()));
                        break;                                            
                }
            }

            newPage = Pages[id];
            if (newPage == null)
                return;

            //pop to root for Windows Phone
            if (Detail != null && Device.OS == TargetPlatform.WinPhone)
            {
                await Detail.Navigation.PopToRootAsync();
            }

            Detail = newPage;
        }
    }
}
