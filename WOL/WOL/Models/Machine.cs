using SQLite;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace WOL.Models
{
    public class Machine : INotifyPropertyChanged
    {
        private string ip;
        public string IP
        {
            get { return this.ip; }
            set { this.ip = value; OnPropertyChanged(); }
        }
        private string mac;
        [PrimaryKey]
        public string MAC
        {
            get { return this.mac; }
            set { this.mac = value; OnPropertyChanged(); }
        }

        private string name;
        public string Name
        {
            get { return this.name; }
            set { this.name = value; OnPropertyChanged(); }
        }

        private string icon;
        public string Icon
        {
            get { return icon; }
            set { this.icon = value; OnPropertyChanged(); }
        }
        public int Port { get; set; }

        #region INotifyPropertyChanged implementation
        public event PropertyChangedEventHandler PropertyChanged;
        #endregion

        public void OnPropertyChanged([CallerMemberName] string propertyName = "")
        {
            var changed = PropertyChanged;
            if (changed == null)
                return;

            changed(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
