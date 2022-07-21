using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace FreeNetLite;

public interface IPacketDispatcher
{
    public void Init(UInt16 headerSize);

    void DispatchPacket(Session session, byte[] buffer, int offset, int size);

    void IncomingPacket(bool IsSystem, Session user, Packet packet);

    Queue<Packet> DispatchAll();
}
