using SQLite;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using WOL.Models;

namespace WOL.Dependency
{
    public delegate void MachineFoundHandler(Machine machine);
    public interface INetworkManager
    {
        void FindMachineList(MachineFoundHandler foundProcess);
        void WakeUp(Machine machine);
        void IsOnline(Machine machine);
        void KeepOnline(List<Machine> lst, TimeSpan time);
        void StopKeepOnline();
        void KeepAliveBySoket(Machine m);
        void KeepAliveByTCP(Machine m);

        bool IsServicesBinded();
    }    
}

