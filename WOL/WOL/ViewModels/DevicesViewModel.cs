using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WOL.ViewModels
{
    public class DevicesViewModel : BaseViewModel
    {
        public DevicesViewModel()
        {
            this.Title = "Devices Manager";
            this.Subtitle = "Wake your devices up";
            Icon = "device.png";
        }
    }
}
