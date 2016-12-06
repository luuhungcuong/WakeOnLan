using SQLite;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WOL.Dependency;
using Xamarin.Forms;

namespace WOL.Views
{
    public partial class DevicesPage : TabbedPage
    {
        private ObservableCollection<Machine> lstRegMachine = new ObservableCollection<Machine>();
        private DbInstance db = null;
        public DevicesPage()
        {
            InitializeComponent();
            string path = DependencyService.Get<IDatabase>().GetDatabasePath();
            db = new DbInstance(path);

            this.lstAvailable.IsPullToRefreshEnabled = true;
            this.lstAvailable.Refreshing += LstAvailable_Refreshing;
            this.lstRegister.IsPullToRefreshEnabled = true;
            this.lstRegister.Refreshing += LstRegister_Refreshing;
            lstRegister.ItemsSource = lstRegMachine;
            LstRegister_Refreshing(this, new EventArgs());

        }

        private void LstRegister_Refreshing(object sender, EventArgs e)
        {
            lstRegMachine.Clear();
            this.lstRegister.IsRefreshing = true;
            Task.Run(() =>
            {                
                List<Machine> lst = db.findRecords();

                Device.BeginInvokeOnMainThread(() =>
                {
                    foreach (var it in lst)
                    {
                        lstRegMachine.Add(it);
                    }
                    this.lstRegister.IsRefreshing = false;
                });
            });
        }

        private void LstAvailable_Refreshing(object sender, EventArgs e)
        {
            btnRefresh_Clicked(sender, e);

        }

        public async void btnRefresh_Clicked(object sender, EventArgs e)
        {
            try
            {
                this.lstAvailable.IsRefreshing = true;
                ObservableCollection<Machine> machineList = new ObservableCollection<Machine>();
                this.lstAvailable.ItemsSource = machineList;

                MachineFoundHandler pg = delegate (Machine machine)
                {
                    Device.BeginInvokeOnMainThread(() =>
                    {
                        machineList.Add(machine);
                    });
                };
                await Task.Run(() =>
                {
                    DependencyService.Get<INetworkManager>().FindMachineList(pg);
                });
                this.lstAvailable.IsRefreshing = false;
            }
            catch (Exception ex)
            {
                await DisplayAlert("Error", ex.ToString(), "Ok");
            }
        }

        public void OnWakeUp(object sender, EventArgs e)
        {
            var mi = ((MenuItem)sender);
            DependencyService.Get<INetworkManager>().WakeUp((mi.CommandParameter as Machine).MAC);
            
        }
        public void OnDelete(object sender, EventArgs e)
        {
            var mi = ((MenuItem)sender);
            string path = DependencyService.Get<IDatabase>().GetDatabasePath();
            db.DeleteData(mi.CommandParameter as Machine);
            lstRegMachine.Remove(mi.CommandParameter as Machine);
        }
        public  void OnRegister(object sender, EventArgs e)
        {
            var mi = ((MenuItem)sender);            
            string path = DependencyService.Get<IDatabase>().GetDatabasePath();
            db.CreateDatabase();
            db.insertUpdateData(mi.CommandParameter as Machine);
            lstRegMachine.Add(mi.CommandParameter as Machine);
        }       

    }
}
