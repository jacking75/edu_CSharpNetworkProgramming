using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MGAsyncNet
{
    // 이 클래스는 멤버만 존재하는데...
    public class AsyncSocketContext
    {
        public INetworkSender NetSender { get; set; }

        public UInt64 ManagedID { get; set; }

        public Int64 UniqueId { get; set; }

        public void Serialize(Packet packet)
        {
            packet.WriteUInt64(ManagedID);
            packet.WriteInt64(UniqueId);
        }            
    }
}
