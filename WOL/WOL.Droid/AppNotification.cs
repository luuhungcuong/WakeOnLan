using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using WOL.Dependency;
using Android.Support.Design.Widget;
using Xamarin.Forms;
[assembly: Xamarin.Forms.Dependency(typeof(WOL.Droid.AppNotification))]
namespace WOL.Droid
{
    public class AppNotification : IAppNotification
    {
        public void Toats(string message)
        {
            Toast.MakeText(Forms.Context, message, ToastLength.Short).Show();
        }
    }
}