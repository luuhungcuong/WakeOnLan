using System;
using Android.App;
using Android.Appwidget;
using Android.Content;
using Android.OS;
using Android.Widget;
using static Android.Views.View;
using Xamarin.Forms.Platform.Android;
using WOL.Models;
using System.Collections.Generic;
using Android.Views;
using WOL.Dependency;
using System.Threading.Tasks;

namespace WOL.Droid
{
    [BroadcastReceiver(Label = "Wake On Lan")]
    [IntentFilter(new string[] { "android.appwidget.action.APPWIDGET_UPDATE" })]
    [MetaData("android.appwidget.provider", Resource = "@layout/SwitchWidgetProvider")]
    public class WakeUpWidget : AppWidgetProvider
    {
        public override void OnUpdate(Context context, AppWidgetManager appWidgetManager, int[] appWidgetIds)
        {
            base.OnUpdate(context, appWidgetManager, appWidgetIds);
        }
        public override void OnReceive(Context context, Intent intent)
        {
            try
            {
                base.OnReceive(context, intent);
                string Name = intent.GetStringExtra("Name");
                string MAC = intent.GetStringExtra("MAC");
                string IP = intent.GetStringExtra("IP");
                string Icon = intent.GetStringExtra("Icon");
                int Port = intent.GetIntExtra("Port", 9);
                Machine m = new Machine() { Name = Name, MAC = MAC, Icon = Icon, IP = IP, Port = Port };
                if (!String.IsNullOrEmpty(m.MAC))
                {                    
                    NetworkManager net = new NetworkManager();
                    net.WakeUp(m);
                    Toast.MakeText(context, "Wake up is sent!!!", ToastLength.Short).Show();
                }
            }
            catch (Exception ex)
            {
                Toast.MakeText(context, ex.Message, ToastLength.Short).Show();
            }
            
        }
    }
    [Service]
    public class SwitchServices : Service
    {
        public override IBinder OnBind(Intent intent)
        {
            return null;
        }

        public override StartCommandResult OnStartCommand(Intent intent, StartCommandFlags flags, int startId)
        {
            OnHandleIntent(intent);
            return StartCommandResult.Sticky;
        }
        protected void OnHandleIntent(Intent intent)
        {
            new Task(() =>
            {
                RemoteViews views = new RemoteViews(this.PackageName, Resource.Layout.SwitchWidget);
                //ComponentName thisWidget = new ComponentName(this, Java.Lang.Class.FromType(typeof(WakeUpWidget)).Name);
                AppWidgetManager manager = AppWidgetManager.GetInstance(this);

                string Name = intent.GetStringExtra("Name");
                string MAC = intent.GetStringExtra("MAC");
                string IP = intent.GetStringExtra("IP");
                string Icon = intent.GetStringExtra("Icon");
                int Port = intent.GetIntExtra("Port", 9);
                int mAppWidgetId = intent.GetIntExtra("mAppWidgetId", 0);
                Intent defineIntent = new Intent(this, typeof(SwitchServices));

                defineIntent.PutExtra("Name", Name);
                defineIntent.PutExtra("MAC", MAC);
                defineIntent.PutExtra("IP", IP);
                defineIntent.PutExtra("Icon", Icon);
                defineIntent.PutExtra("Port", Port);
                defineIntent.PutExtra("mAppWidgetId", mAppWidgetId);

                Machine m = new Machine() { Name = Name, MAC = MAC, Icon = Icon, IP = IP, Port = Port };
                Xamarin.Forms.DependencyService.Get<INetworkManager>().WakeUp(m);

                views.SetImageViewBitmap(Resource.Id.icon, this.Resources.GetBitmap(Icon));
                views.SetTextViewText(Resource.Id.txtName, m.Name);

                PendingIntent pendingIntent = PendingIntent.GetService(this, mAppWidgetId, defineIntent, PendingIntentFlags.CancelCurrent);
                views.SetOnClickPendingIntent(Resource.Id.icon, pendingIntent);

                manager.UpdateAppWidget(mAppWidgetId, views);
                var handler = new Handler(Looper.MainLooper);
                handler.Post(() =>
                {
                    Toast.MakeText(this, "Wake up is sent!!!", ToastLength.Short).Show();
                });

            }).Start();
        }
    }
    [Activity(Label = "Select your device", Name = "lhc.app.WakeOnLan.ConfigurationWakeUpActivity",
        Theme = "@android:style/Theme.Material.Light", Icon = "@drawable/desktop_online")]
    [IntentFilter(new string[] { "android.appwidget.action.APPWIDGET_CONFIGURE" })]
    public class ConfigurationWakeUpActivity : Activity
    {
        private List<Machine> lstMachine = new List<Machine>();
        protected override void OnCreate(Bundle bundle)
        {

            base.OnCreate(bundle);

            this.SetContentView(Resource.Layout.WakeupConfiguration);
            this.SetResult(Result.Canceled);

            //Bind event            
            var listView = FindViewById<ListView>(Resource.Id.lstWidgetDevice);
            listView.ItemClick += OnListItemClick;

            //Get list devices
            new Task(() =>
            {
                Database db = new Database();
                DbInstance instance = new DbInstance(db.GetDatabasePath());
                lstMachine = instance.findRecords();
                RunOnUiThread(() =>
                {
                    listView.Adapter = new CusotmListAdapter(this, lstMachine);
                });
            }).Start();
        }
        private async void OnListItemClick(object sender, AdapterView.ItemClickEventArgs e)
        {
            await Task.Run(() =>
            {
                Machine item = lstMachine[e.Position];
                int mAppWidgetId = this.Intent.GetIntExtra(AppWidgetManager.ExtraAppwidgetId, AppWidgetManager.InvalidAppwidgetId);
                Intent resultValue = new Intent();
                resultValue.PutExtra(AppWidgetManager.ExtraAppwidgetId, mAppWidgetId);
                this.SetResult(Result.Ok, resultValue);

                RemoteViews views = new RemoteViews(this.PackageName, Resource.Layout.SwitchWidget);
                //ComponentName thisWidget = new ComponentName(this, Java.Lang.Class.FromType(typeof(WakeUpWidget)).Name);
                AppWidgetManager manager = AppWidgetManager.GetInstance(this);

                views.SetTextViewText(Resource.Id.txtName, item.Name);
                views.SetImageViewBitmap(Resource.Id.icon, this.Resources.GetBitmap(item.Icon));

                Intent defineIntent = new Intent(this, typeof(WakeUpWidget));
                defineIntent.PutExtra("Name", item.Name);
                defineIntent.PutExtra("MAC", item.MAC);
                defineIntent.PutExtra("IP", item.IP);
                defineIntent.PutExtra("Icon", item.Icon);
                defineIntent.PutExtra("Port", item.Port);
                defineIntent.PutExtra("mAppWidgetId", mAppWidgetId);

                defineIntent.SetAction("android.appwidget.action.APPWIDGET_UPDATE");
                PendingIntent pendingIntent = PendingIntent.GetBroadcast(this, mAppWidgetId, defineIntent, 0);

                views.SetOnClickPendingIntent(Resource.Id.icon, pendingIntent);

                manager.UpdateAppWidget(mAppWidgetId, views);
            });
            Toast.MakeText(this, "Wake up is configured!!!", ToastLength.Short).Show();
            this.Finish();
        }
    }
    public class CusotmListAdapter : BaseAdapter<Machine>
    {
        Activity context;
        List<Machine> list;

        public CusotmListAdapter(Activity _context, List<Machine> _list)
            : base()
        {
            this.context = _context;
            this.list = _list;
        }

        public override int Count
        {
            get { return list.Count; }
        }

        public override long GetItemId(int position)
        {
            return position;
        }

        public override Machine this[int index]
        {
            get { return list[index]; }
        }

        public override View GetView(int position, View convertView, ViewGroup parent)
        {
            View view = convertView;

            // re-use an existing view, if one is available
            // otherwise create a new one
            if (view == null)
                view = context.LayoutInflater.Inflate(Resource.Layout.ListItemTemplate, parent, false);

            Machine item = this[position];
            view.FindViewById<TextView>(Resource.Id.txtName).Text = item.Name;
            view.FindViewById<TextView>(Resource.Id.txtIP).Text = item.IP;
            view.FindViewById<TextView>(Resource.Id.txtMAC).Text = item.MAC;
            view.FindViewById<ImageView>(Resource.Id.imgIcon).SetImageBitmap(this.context.Resources.GetBitmap(item.Icon));

            return view;
        }
    }

}