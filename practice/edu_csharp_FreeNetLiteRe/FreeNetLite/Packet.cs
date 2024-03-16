using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace FreeNetLite;

public class Packet
{
	public Session Owner { get; private set; }
	public byte[] BodyData { get; private set; }
	public UInt16 Id { get; private set; }

			
	public Packet(Session owner, UInt16 packetId, byte[] body)
	{
		Owner = owner;
		BodyData = body;
		Id = packetId;			
	}

}
