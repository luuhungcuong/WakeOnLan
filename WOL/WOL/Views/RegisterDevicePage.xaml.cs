using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using WOL.Dependency;
using WOL.Models;
using Xamarin.Forms;

namespace WOL.Views
{
    public partial class RegisterDevicePage : ContentPage
    {
        public Machine machine { get; set; }
        private DbInstance db { get; set; }
        public RegisterDevicePage(Machine machine, DbInstance db)
        {
            InitializeComponent();
            this.machine = machine;
            this.BindingContext = machine;
            this.db = db;
        }
        public async void btnOk_OnClicked(object sender, EventArgs e)
        {
            try
            {
                //Check name of devices
                if (String.IsNullOrEmpty(machine.Name))
                {
                    await DisplayAlert("Error", "Name of device is not correct", "Ok");
                    return;
                }
                //Check mac adrress
                if (String.IsNullOrEmpty(machine.MAC))
                {
                    await DisplayAlert("Error", "Mac address is not correct", "Ok");
                    return;
                }
                else
                {
                    var r = new System.Text.RegularExpressions.Regex("..:..:..:..:..:..");
                    if (!r.IsMatch(machine.MAC))
                    {
                        await DisplayAlert("Error", "Mac address is not correct", "Ok");
                        return;
                    }
                }
                //Check ip address
                if (String.IsNullOrEmpty(machine.IP))
                {
                    await DisplayAlert("Error", "IP is not correct", "Ok");
                    return;
                }
                else
                {
                    
                }
                //Check ip address
                if (String.IsNullOrEmpty(txtPort.Text))
                {
                    await DisplayAlert("Error", "Port is not correct", "Ok");
                    return;
                }
                else
                {
                    int ret;
                    if (!int.TryParse(txtPort.Text, out ret))
                    {
                        await DisplayAlert("Error", "Port is not correct", "Ok");
                        return;
                    }
                }
                db.insertUpdateData(machine);
                await Navigation.PopModalAsync();
            }
            catch (Exception ex)
            {
                await DisplayAlert("Error", ex.ToString(), "Ok");
            }
           
        }
        public void picType_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (picType.SelectedIndex == -1)
            {
                //machine.Icon = "computer.png";
            }
            else
            {
                string icontype = picType.Items[picType.SelectedIndex].ToLower();
                machine.Icon = string.Format("{0}.png", icontype);
            }
        }
    }
}
