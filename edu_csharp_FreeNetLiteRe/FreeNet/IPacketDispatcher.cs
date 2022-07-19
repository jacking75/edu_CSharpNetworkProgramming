using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace FreeNet;

public interface IPacketDispatcher
{
    void OnReceive(Session session, byte[] buffer, int offset, int size);

    void IncomingPacket(bool IsSystem, Session user, Packet packet);

    Queue<Packet> DispatchAll();
}
