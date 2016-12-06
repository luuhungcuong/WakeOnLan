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
using System.IO;
using WOL.Droid;

[assembly: Xamarin.Forms.Dependency(typeof(Database))]
namespace WOL.Droid
{
    public class Database : IDatabase
    {
        public string GetDatabasePath()
        {
            var dbName = "machine.db3";
            var path = Path.Combine(System.Environment.
              GetFolderPath(System.Environment.
              SpecialFolder.Personal), dbName);
            return path;
        }
    }
}