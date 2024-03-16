using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace FreeNet
{
    public interface IPacketDispatcher
    {
        void IncomingPacket(bool IsSystem, Session user, Packet packet);

        Queue<Packet> DispatchAll();
    }
}
