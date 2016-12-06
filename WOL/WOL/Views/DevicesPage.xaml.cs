using Android.Util;
using SQLite;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WOL.Controls;
using WOL.Dependency;
using WOL.Models;
using WOL.ViewModels;
using Xamarin.Forms;

namespace WOL.Views
{
    public partial class DevicesPage : TabbedPage
    {
        private ObservableCollection<Machine> lstRegMachine = new ObservableCollection<Machine>();
        private DbInstance db = null;

        private DevicesViewModel ViewModel
        {
            get { return BindingContext as DevicesViewModel; }
        }
        public DevicesPage()
        {
            InitializeComponent();
            
            BindingContext = new DevicesViewModel();

            string path = DependencyService.Get<IDatabase>().GetDatabasePath();
            db = new DbInstance(path);

            bool isbind = DependencyService.Get<INetworkManager>().IsServicesBinded();
            swtKeepAlive.IsToggled = isbind;

            this.lstAvailable.IsPullToRefreshEnabled = true;
            this.lstAvailable.Refreshing += LstAvailable_Refreshing;

            this.lstRegister.IsPullToRefreshEnabled = true;
            this.lstRegister.Refreshing += LstRegister_Refreshing;
            lstRegister.ItemsSource = lstRegMachine;
            InitListRegister();

            this.CurrentPage = this.Children[1];
            
            this.picTime.SelectedIndex = 1;
        }

        public void InitListRegister()
        {            
            Task.Run(() =>
            {
                try
                {
                    Device.BeginInvokeOnMainThread(() =>
                    {
                        lstRegMachine.Clear();
                    });

                    List<Machine> lst = db.findRecords();

                    Device.BeginInvokeOnMainThread(() =>
                    {
                        foreach (var it in lst)
                        {
                            lstRegMachine.Add(it);
                        }
                        this.lstRegister.IsRefreshing = false;
                        if (this.lstRegMachine.Count == 0) this.CurrentPage = this.Children[2];
                    });
                }      
                catch (Exception ex)
                {
                    Log.Error("WOL.Error", ex.ToString());
                }
            });
        }
        private async void LstRegister_Refreshing(object sender, EventArgs e)
        {
            try
            {
                await Task.Run(() =>
                {
                    foreach (var it in lstRegMachine)
                    {
                        DependencyService.Get<INetworkManager>().IsOnline(it);
                    }
                });
                this.lstRegister.ItemsSource = lstRegMachine;
                this.lstRegister.IsRefreshing = false;
            }
            catch (Exception ex)
            {
                await DisplayAlert("Error", ex.ToString(), "Ok");
            }
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
                        var lst = lstRegMachine.Where(x => x.MAC == machine.MAC).ToList();
                        if (lst.Count() > 0)
                        {
                            machine.Icon = lst[0].Icon;
                        }
                        machineList.Add(machine);
                    });
                };
                await Task.Run(() =>
                {
                    try
                    {
                        DependencyService.Get<INetworkManager>().FindMachineList(pg);
                    }
                    catch (Exception ex)
                    {
                        Log.Error("WOL.Error", ex.ToString());
                    }                    
                });
                this.lstAvailable.IsRefreshing = false;
            }
            catch (Exception ex)
            {
                await DisplayAlert("Error", ex.ToString(), "Ok");
            }
        }

        public async void OnWakeUp(object sender, EventArgs e)
        {
            try
            {

                var mi = ((MenuItem)sender);
                DependencyService.Get<INetworkManager>().WakeUp((mi.CommandParameter as Machine));
                DependencyService.Get<IAppNotification>().Toats("WOL package is sent!!!");
                //var dialog = Acr.UserDialogs.UserDialogs.Instance;
                //dialog.Toast("WOL package is sent!!!");

            }
            catch (Exception ex)
            {
                await DisplayAlert("Error", ex.ToString(), "Ok");
            }

        }
        public async void OnDelete(object sender, EventArgs e)
        {
            try
            {
                var mi = ((MenuItem)sender);
                if (await DisplayAlert("Confirm", "Do you want to remove " + (mi.CommandParameter as Machine).Name, "Ok", "Cancel"))
                {
                    db.DeleteData(mi.CommandParameter as Machine);
                    lstRegMachine.Remove(mi.CommandParameter as Machine);
                }                                                
            }
            catch (Exception ex)
            {
                await DisplayAlert("Error", ex.ToString(), "Ok");
            }
        }
        public async void OnRegister(object sender, EventArgs e)
        {
            try
            {
                var mi = ((MenuItem)sender);                
                db.CreateDatabase();
                var machine = mi.CommandParameter as Machine;
                machine.Icon = "computer.png";
                machine.Port = 9;
                db.insertUpdateData(machine);
                lstRegMachine.Add(machine);
                //var dialog = Acr.UserDialogs.UserDialogs.Instance;
                //dialog.Toast("Devices is registerd!!!");
                DependencyService.Get<IAppNotification>().Toats("Devices is registerd!!!");
            }
            catch (Exception ex)
            {
                await DisplayAlert("Error", ex.ToString(), "Ok");
            }
        }
        public async void OnEdit(object sender, EventArgs e)
        {
            try
            {              
                var mi = ((MenuItem)sender);
                var editPage = new RegisterDevicePage(mi.CommandParameter as Machine, db);
                await Navigation.PushModalAsync(editPage);                                
            }
            catch (Exception ex)
            {
                await DisplayAlert("Error", ex.ToString(), "Ok");
            }
        }
        public async void OnKeepOnlineBySocket(object sender, EventArgs e)
        {
            try
            {
                //var mi = ((MenuItem)sender);
                //Device.StartTimer(new TimeSpan(0, 0, int.Parse(picTime.Items[picTime.SelectedIndex])),
                //   () =>
                //   {
                //       try
                //       {
                //           foreach (var it in lstRegMachine)
                //           {
                //               if (!this.swtKeepAliveTCP.IsToggled)
                //                   return this.swtKeepAliveTCP.IsToggled;
                //               DependencyService.Get<INetworkManager>().KeepAliveBySoket(it);
                //           }
                //       }
                //       catch (Exception a)
                //       {
                //           Device.BeginInvokeOnMainThread(() =>
                //           {
                //               DisplayAlert("Error", a.ToString(), "Ok");
                //           });
                //           return false;
                //       }
                //       return swtKeepAliveTCP.IsToggled;
                //   });

            }
            catch (Exception ex)
            {
                await DisplayAlert("Error", ex.ToString(), "Ok");
            }
        }
        public async void OnKeepOnlineByTCP(object sender, EventArgs e)
        {
            try
            {
                //var mi = ((MenuItem)sender);
                //Device.StartTimer(new TimeSpan(0, 0, int.Parse(picTime.Items[picTime.SelectedIndex])),
                //   () =>
                //   {
                //       try
                //       {
                //           foreach (var it in lstRegMachine)
                //           {
                //               if (!this.swtKeepAliveTCP.IsToggled)
                //                   return this.swtKeepAliveTCP.IsToggled;
                //               DependencyService.Get<INetworkManager>().KeepAliveByTCP(it);
                //           }
                //       }
                //       catch (Exception a)
                //       {
                //           Device.BeginInvokeOnMainThread(() =>
                //           {
                //               DisplayAlert("Error", a.ToString(), "Ok");
                //           });
                //           return false;
                //       }                       
                //       return swtKeepAliveTCP.IsToggled;
                //   });

            }
            catch (Exception ex)
            {
                await DisplayAlert("Error", ex.ToString(), "Ok");
            }
        }
        //public void swtKeepAliveTCP_Toggled(object sender, ToggledEventArgs e)
        //{
            
        //}

        public async void swtKeepAlive_Toggled(object sender, ToggledEventArgs e)
        {
            if (lstRegMachine.Count > 0)
            {
                if (e.Value)
                {                  
                    DependencyService.Get<INetworkManager>().KeepOnline(lstRegMachine.ToList(),
                        new TimeSpan(0, 0, int.Parse(picTime.Items[picTime.SelectedIndex])));
                    //var dialog = Acr.UserDialogs.UserDialogs.Instance;
                    //dialog.Toast("Services is started!!!");
                    DependencyService.Get<IAppNotification>().Toats("Services is started!!!");
                }
                else
                {
                    DependencyService.Get<INetworkManager>().StopKeepOnline();
                    //var dialog = Acr.UserDialogs.UserDialogs.Instance;
                    //dialog.Toast("Services is stoped!!!");
                    DependencyService.Get<IAppNotification>().Toats("Services is stoped!!!");
                }
            }
            else
            {
                await DisplayAlert("Error", "There are no devices in devices tab. You must register your device first!", "Ok");
                this.CurrentPage = this.Children[1];
            }
        }
    }
}
