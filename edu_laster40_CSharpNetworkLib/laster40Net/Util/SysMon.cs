using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;
using System.Net;

namespace laster40Net.Util
{
    public class SysMon
    {
        public struct MachinePerfmon
        {
            /// <summary>
            /// 머신 이름
            /// </summary>
            public string name;
            /// <summary>
            /// 주소
            /// </summary>
            public string[] address;
            /// <summary>
            /// cpu사용량
            /// </summary>
	        public int cpu ;
            /// <summary>
            /// 메모리 사용가능 mb
            /// </summary>
            public int mem;
            /// <summary>
            /// 메모리 사용가능 mb
            /// </summary>
            public int avaliableMem;
            /// <summary>
            /// nic 사용량 byte
            /// </summary>
            public int[] nic;
            /// <summary>
            /// 남은 hdd 공간
            /// </summary>
            public int[] freeHDD;
        }

        public string MachineName { get { return _machineName; } }
        private string _machineName = "(Unknown)";
        private PerformanceCounter _CPUCounter;
        private PerformanceCounter _memCounter;
        private PerformanceCounter[] _nicCounter;
        
        public SysMon()
        {
            try{
                this._machineName = System.Environment.MachineName;
            }catch(Exception)
            {
            }

            InitMachinePerfCounter();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        private bool InitMachinePerfCounter()
        {
            try
            {
                _CPUCounter = new PerformanceCounter("Processor", "% Processor Time", "_Total");
                _memCounter = new PerformanceCounter("Memory", "Available MBytes");
                _nicCounter = InitNICCounters();
            }catch(Exception )
            {
                return false;
            }

            return true;
        }

        /// <summary>
        /// 로컬 ip 주소
        /// </summary>
        /// <returns></returns>
        private string[] GetLocalIPAddress()
        {
            List<string> lists = new List<string>();
            IPAddress[] localIPs = Dns.GetHostAddresses(Dns.GetHostName());
            foreach (var ip in localIPs)
            {
                // local
                if (IPAddress.IsLoopback(ip)) continue;
                lists.Add(ip.ToString());
            }

            return lists.ToArray();
        }

        private string[] GetNICInstanceNames(string machineName)
        {
            string filter = "MS TCP Loopback interface";
            List<string> nics = new List<string>();
            PerformanceCounterCategory category = new PerformanceCounterCategory("Network Interface", machineName);
            if (category.GetInstanceNames() != null)
            {
                foreach (string nic in category.GetInstanceNames())
                {
                    if (!nic.Equals(filter, StringComparison.InvariantCultureIgnoreCase))
                    { 
                        nics.Add(nic); 
                    }
                }
            }
            return nics.ToArray();
        }

        /// <summary>
        /// nic counter 를 생성해서 가져온다.
        /// </summary>
        /// <returns></returns>
        private PerformanceCounter[] InitNICCounters()
        {
            string[] nics = GetNICInstanceNames(this._machineName);
            List<PerformanceCounter> nicCounters = new List<PerformanceCounter>();
            foreach (string nicInstance in nics)
            {
                nicCounters.Add(new PerformanceCounter("Network Interface", "Bytes Total/sec", nicInstance, this._machineName));
            }
            return nicCounters.ToArray();
        }

        /// <summary>
        /// 남은 hdd공간 가져오기
        /// </summary>
        /// <returns></returns>
        public int[] GetHddFreeSpace()
        {
            System.IO.DriveInfo[] driverInfo = System.IO.DriveInfo.GetDrives();
            if (driverInfo.Length <= 0)
            {
                return null;
            }

            int[] freeSpace = new int[driverInfo.Length];
            for (int i = 0; i < driverInfo.Length; ++i)
            {
                try
                {
                    freeSpace[i] = (int)(driverInfo[i].AvailableFreeSpace / 1024);
                }catch(Exception)
                {
                    freeSpace[i] = 0;
                }
            }

            return freeSpace;
        }

        public MachinePerfmon GetMachinePerfmon()
        {
            MachinePerfmon mon = new MachinePerfmon();
            try
            {
                mon.name = _machineName;
                mon.address = GetLocalIPAddress();
                mon.cpu = (int)_CPUCounter.NextValue();
                mon.mem = (int)Environment.WorkingSet;
                mon.avaliableMem = (int)_memCounter.NextValue();
                mon.nic = new int[_nicCounter.Length];
                for (int i = 0; i < _nicCounter.Length; ++i)
                {
                    mon.nic[i] = (int)_nicCounter[i].NextValue();
                }

                mon.freeHDD = GetHddFreeSpace();
            }
            catch(Exception )
            {
            }

            return mon;
        }


    }
}
