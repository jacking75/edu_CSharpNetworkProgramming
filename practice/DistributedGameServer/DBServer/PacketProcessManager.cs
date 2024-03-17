using Microsoft.Extensions.Logging;

using System;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Text;
using System.Threading;

namespace DBServer
{
    class PacketProcessManager
    {
        UInt64 RLProcessIndex = UInt64.MaxValue;

        UInt64 ProcessCount = 0;
        List<PacketProcess> ProcessList = new ();
                

        public void Init(ServerOption serverOpt, Action<Int32, string, byte[], int> mqSendFunc)
        {
            ProcessCount = (UInt64)serverOpt.WorkerThreadCount;

            for (int i = 0; i < serverOpt.WorkerThreadCount; ++i)
            {
                var process = new PacketProcess();
                process.Init(serverOpt, mqSendFunc);

                ProcessList.Add(process);
            }
        }
                
        public void Destory()
        {
            foreach (var process in ProcessList)
            {
                process.Destory();
            }
        }

        public void Distribute(int mqIndex, byte[] data)
        {
            var value = Interlocked.Increment(ref RLProcessIndex);
            var index = (int)(value % ProcessCount);

            ProcessList[index].AddReqData(mqIndex, data);
        }

              

        
    }
}
