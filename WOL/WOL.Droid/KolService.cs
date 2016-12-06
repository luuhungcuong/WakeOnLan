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
using WOL.Models;
using Xamarin.Forms;
using WOL.Dependency;

namespace WOL.Droid
{
    public class ServiceConnectedEventArgs : EventArgs
    {
        public IBinder Binder { get; set; }
    }

    [Service]
    public class KolService : Service
    {
        IBinder binder;
        public bool IsRunning { get { return repeat; } }
        bool repeat = false;
        public override IBinder OnBind(Intent intent)
        {
            binder = new KolServiceBinder(this);
            
            return binder;
        }
        public override StartCommandResult OnStartCommand(Intent intent, StartCommandFlags flags, int startId)
        {
            return StartCommandResult.Sticky;
        }
        public void KeepAlive(List<Machine> lst, TimeSpan time)
        {
            this.repeat = true;
            if (this.repeat)
            {
                Device.StartTimer(time,
                   () =>
                   {
                       foreach (var it in lst)
                       {                          
                           DependencyService.Get<INetworkManager>().WakeUp(it);
                       }
                       return this.repeat;
                   });
            }
        }
        public void StopKeepAlive()
        {
            this.repeat = false;
        }
    }

    public class KolServiceBinder : Binder
    {
        protected KolService service;
        public KolService Service
        {
            get { return this.service; }
        }

        public bool IsBound { get; set; }
        public KolServiceBinder(KolService service)
        {
            this.service = service;
        }
    }
    public class KolServiceConnection : Java.Lang.Object, IServiceConnection
    {
        public event EventHandler<ServiceConnectedEventArgs> ServiceConnected = delegate { };

        public KolServiceBinder Binder
        {
            get { return this.binder; }
            set { this.binder = value; }
        }
        protected KolServiceBinder binder;
        public KolServiceConnection(KolServiceBinder binder)
        {
            if (binder != null)
            {
                this.binder = binder;
            }
        }

        public void OnServiceConnected(ComponentName name, IBinder service)
        {
            KolServiceBinder serviceBinder = service as KolServiceBinder;

            if (serviceBinder != null)
            {
                this.binder = serviceBinder;
                this.binder.IsBound = true;

                // raise the service bound event
                this.ServiceConnected(this, 
                    new ServiceConnectedEventArgs()
                    {
                        Binder = service
                    });                
            }
        }

        public void OnServiceDisconnected(ComponentName name)
        {
            this.binder.IsBound = false;
        }
    }
}