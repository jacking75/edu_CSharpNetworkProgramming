﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace FreeNetLite;

/// <summary>
/// byte[] 버퍼를 참조로 보관하여 pop_xxx 매소드 호출 순서대로 데이터 변환을 수행한다.
/// </summary>
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

	public Packet(Session owner, UInt16 packetId)
	{
		// 참조로만 보관하여 작업한다.
		// 복사가 필요하면 별도로 구현해야 한다.
		Owner = owner;
		Id = packetId;
	}
					
	
}