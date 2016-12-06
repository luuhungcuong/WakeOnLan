using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Android.App;
using Android.Content;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using WOL.Dependency;
using WOL.Droid;
using System.IO;
using System.Threading;
using Java.Net;
using Android.Util;
using Java.Lang;
using Android.Net.Wifi;
using Java.IO;
using System.Threading.Tasks;
using WOL.Models;
using Xamarin.Forms;

[assembly: Xamarin.Forms.Dependency(typeof(NetworkManager))]
namespace WOL.Droid
{
    public class NetworkManager : INetworkManager
    {
        protected static KolServiceConnection kolServiceConnection;

        #region Private
        private const string CMD = "/system/bin/ping -c 1 {0}";
        private static Dictionary<string, string> MacTable = null;
        private void GetInetAddressList(MachineFoundHandler foundProcess)
        {

            string subnet = GetSubnetAddress();
            var r = Runtime.GetRuntime();
            RefreshMacTable();
            const int TASK_NUMBER = 50;
            List<Task> tasksList = new List<Task>();
            for (int t = 0; t < TASK_NUMBER; t++)
            {
                var task = Scan(r, subnet, t * (255 / TASK_NUMBER), System.Math.Min((t + 1) * (255 / TASK_NUMBER), 256), foundProcess);
                tasksList.Add(task);
            }
            Task.WaitAll(tasksList.ToArray());
        }
        private Task Scan(Runtime r, string subnet, int start, int end, MachineFoundHandler foundProcess)
        {
            return Task.Run(() =>
            {
                for (var i = start; i < end; i++)
                {
                    string host = subnet + "." + i.ToString();
                    Process exec = r.Exec(string.Format(CMD, host));
                    int i1 = exec.WaitFor();
                    if (i1 == 0)
                    {
                        InetAddress a = InetAddress.GetByName(host);
                        Machine m = new Machine()
                        {
                            IP = a.HostAddress,
                            Name = a.HostName,
                            MAC = GetMacAddress(a.HostAddress),
                            Icon = "device.png",
                            Port = 9
                        };
                        if (!string.IsNullOrEmpty(m.MAC))
                        {
                            foundProcess(m);
                        }
                    }
                    exec.Destroy();
                }
            });
        }

        private string GetIPAddress()
        {
            WifiManager manager = (WifiManager)Android.App.Application.Context.GetSystemService(Service.WifiService);
            int ip = manager.ConnectionInfo.IpAddress;
            System.Net.IPAddress address = new System.Net.IPAddress(ip);
            return address.ToString();
        }
        private string GetSubnetAddress()
        {
            string ip = GetIPAddress();
            return GetIPAddress().Substring(0, ip.LastIndexOf("."));
        }

        private string GetMacAddress(string ip)
        {
            if (ip == null)
                return null;
            if (MacTable == null)
            {
                RefreshMacTable();
            }
            if (MacTable.ContainsKey(ip)) return MacTable[ip];
            else
            {
                //Read file direct
                using (var br = new BufferedReader(new FileReader("/proc/net/arp")))
                {
                    string line;
                    while ((line = br.ReadLine()) != null)
                    {
                        System.Text.RegularExpressions.Regex r = new System.Text.RegularExpressions.Regex(" +");
                        string[] splitted = r.Split(line);
                        if (splitted != null && splitted.Length >= 4 && splitted[0].Equals(ip))
                        {
                            // Basic sanity check
                            string mac = splitted[3];
                            r = new System.Text.RegularExpressions.Regex("..:..:..:..:..:..");
                            if (r.IsMatch(mac))
                            {
                                return mac;
                            }
                        }
                    }
                }
            }
            return string.Empty;
        }
        private void RefreshMacTable()
        {
            MacTable = new Dictionary<string, string>();
            using (var br = new BufferedReader(new FileReader("/proc/net/arp")))
            {
                string line;
                while ((line = br.ReadLine()) != null)
                {
                    System.Text.RegularExpressions.Regex r = new System.Text.RegularExpressions.Regex(" +");
                    string[] splitted = r.Split(line);
                    if (splitted != null && splitted.Length >= 4)
                    {
                        // Basic sanity check
                        string mac = splitted[3];
                        r = new System.Text.RegularExpressions.Regex("..:..:..:..:..:..");
                        if (r.IsMatch(mac))
                        {
                            MacTable.Add(splitted[0], mac);
                        }
                    }
                }
            }
        }
        #endregion

        public void KeepAliveBySoket(Machine m)
        {
            System.Net.IPEndPoint endPoint = new System.Net.IPEndPoint(System.Net.IPAddress.Parse(m.IP), 80);
            using (System.Net.Sockets.Socket s = new System.Net.Sockets.Socket(endPoint.AddressFamily,
                System.Net.Sockets.SocketType.Stream, System.Net.Sockets.ProtocolType.Tcp))
            {
                s.SetSocketOption(System.Net.Sockets.SocketOptionLevel.Socket, System.Net.Sockets.SocketOptionName.KeepAlive, true);
                s.Connect(endPoint);
                byte[] data = new byte[] { 0x00 };
                for (int i = 0; i < 5; i++)
                {
                    s.Send(data);
                    System.Threading.Thread.Sleep(10);
                }
                s.Close();
            }
        }

        public void KeepAliveByTCP(Machine m)
        {
            using (System.Net.Sockets.TcpClient client = new System.Net.Sockets.TcpClient())
            {
                client.Client.SetSocketOption(System.Net.Sockets.SocketOptionLevel.Socket, System.Net.Sockets.SocketOptionName.KeepAlive, true);
                client.Connect(System.Net.IPAddress.Parse(m.IP), 80);
                byte[] data = new byte[] { 0x00 };
                for (int i = 0; i < 5; i++)
                {
                    client.Client.Send(data);
                    System.Threading.Thread.Sleep(10);
                }
                client.Close();
            }
        }

        public void WakeUp(Machine m)
        {
            string macAddress = m.MAC;
            using (System.Net.Sockets.UdpClient client = new System.Net.Sockets.UdpClient())
            {
                //System.Net.IPAddress ipadr;
                client.EnableBroadcast = true;
                //if (System.Net.IPAddress.TryParse(m.IP, out ipadr))
                //{
                //    client.Connect(ipadr, m.Port);
                //}
                //else
                client.Connect(System.Net.IPAddress.Broadcast, m.Port);

                byte[] datagram = new byte[102];
                for (int i = 0; i <= 5; i++)
                {
                    datagram[i] = 0xff;
                }

                string[] macDigits = null;
                if (macAddress.Contains("-"))
                {
                    macDigits = macAddress.Split('-');
                }
                else
                {
                    macDigits = macAddress.Split(':');
                }

                int start = 6;
                for (int i = 0; i < 16; i++)
                {
                    for (int x = 0; x < 6; x++)
                    {
                        datagram[start + i * 6 + x] = (byte)Convert.ToInt32(macDigits[x], 16);
                    }
                }
                //Send three packet
                for (int i = 0; i < 3; i++)
                {
                    client.Send(datagram, datagram.Length);
                    System.Threading.Thread.Sleep(10);
                }
            }
        }

        public void FindMachineList(MachineFoundHandler foundProcess)
        {
            GetInetAddressList(foundProcess);
        }

        public void IsOnline(Machine machine)
        {
            Task.Run(() =>
            {
                string file = string.Empty;
                int pos = -1;
                var r = Runtime.GetRuntime();
                Process exec = r.Exec(string.Format(CMD, machine.IP));
                int i1 = exec.WaitFor();
                if (i1 == 0)
                {
                    exec.Destroy();
                    file = System.IO.Path.GetFileNameWithoutExtension(machine.Icon);
                    pos = file.LastIndexOf("_");
                    if (pos > 0) file = file.Substring(0, pos);
                    machine.Icon = string.Format("{0}_online.png", file);
                    return true;
                }
                exec.Destroy();
                file = System.IO.Path.GetFileNameWithoutExtension(machine.Icon);
                pos = file.LastIndexOf("_");
                if (pos > 0) file = file.Substring(0, pos);
                machine.Icon = string.Format("{0}_offline.png", file);
                return false;
            });
        }

        private void ConnectedToServices()
        {
            if (kolServiceConnection == null)
            {
                kolServiceConnection = new KolServiceConnection(null);
            }
            
                new Task(() =>
                {
                    Android.App.Application.Context.StartService(new Intent(Android.App.Application.Context, typeof(KolService)));
                    Intent kolServiceIntent = new Intent(Android.App.Application.Context, typeof(KolService));
                    Android.App.Application.Context.BindService(kolServiceIntent, kolServiceConnection, Bind.AutoCreate);
                }).Start();
            
        }
        public async void KeepOnline(List<Machine> lst, TimeSpan time)
        {
            if (kolServiceConnection == null)
            {
                ConnectedToServices();
            }
            await Task.Run(() =>
            {
                while (kolServiceConnection.Binder == null)
                {
                    System.Threading.Thread.Sleep(100);
                }
            });
            var service = kolServiceConnection.Binder.Service;
            if (!service.IsRunning)
            {
                service.KeepAlive(lst, time);
            }

        }
        public void StopKeepOnline()
        {
            if (kolServiceConnection != null)
            {
                kolServiceConnection.Binder.Service.StopKeepAlive();
                kolServiceConnection.Binder.Service.StopSelf();
                Android.App.Application.Context.UnbindService(kolServiceConnection);
                kolServiceConnection = null;
            }
        }

        public bool IsServicesBinded()
        {
            var ret = IsMyServiceRunning(typeof(KolService));
            if (ret && kolServiceConnection == null)
            {
                ConnectedToServices();
            }
            return ret;
        }
        private bool IsMyServiceRunning(System.Type cls)
        {
            ActivityManager manager = (ActivityManager)Android.App.Application.Context.GetSystemService(Context.ActivityService);

            foreach (var service in manager.GetRunningServices(int.MaxValue))
            {
                if (service.Service.ClassName.Equals(Java.Lang.Class.FromType(cls).CanonicalName))
                {
                    return true;
                }
            }
            return false;
        }
    }
}