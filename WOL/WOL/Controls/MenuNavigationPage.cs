using System;
using Xamarin.Forms;

namespace WOL.Controls
{
    public class MenuNavigationPage :NavigationPage
    {
        public MenuNavigationPage(Page root) : base(root)
        {
            Init();
        }

        public MenuNavigationPage()
        {
            Init();
        }

        void Init()
        {

            BarBackgroundColor = Color.FromHex("#03A9F4");
            BarTextColor = Color.White;
        }
    }
}

