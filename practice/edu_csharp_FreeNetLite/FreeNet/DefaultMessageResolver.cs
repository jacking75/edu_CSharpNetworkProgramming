using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace FreeNet
{	
	/// <summary>
	/// [header][body] 구조를 갖는 데이터를 파싱하는 클래스.
	/// - header : 총 크기(2바이트), 패킷ID( 2바이트), 패킷 타입(1바이트)
	/// - body : 메시지 본문.
	/// </summary>
	public class DefaultMessageResolver : IMessageResolver
	{
		public static readonly UInt16 HEADERSIZE = 5;
		public static readonly UInt16 HEADER_PACKETID_POS = 2;

		// 진행중인 버퍼.
		byte[] SecondaryBuffer;

		// 남은 사이즈.
		int RemainBytes = 0;

				

		public void Init(int bufferSize)
		{
			SecondaryBuffer = new byte[bufferSize];
		}

		/// <summary>
		/// 소켓 버퍼로부터 데이터를 수신할 때 마다 호출된다.
		/// 데이터가 남아 있을 때 까지 계속 패킷을 만들어 callback을 호출 해 준다.
		/// 하나의 패킷을 완성하지 못했다면 버퍼에 보관해 놓은 뒤 다음 수신을 기다린다.
		/// </summary>
		/// <param name="buffer"></param>
		/// <param name="offset"></param>
		/// <param name="receiveSize"></param>
		public void OnReceive(byte[] buffer, int offset, int size, Action<Packet> callbackFunc)
		{
			(var result, var receiveBufferOffset, var receiveBufferReaminSize) = PacketProcessFromSecondaryBuffer(buffer, offset, size, callbackFunc);
			
			// 남은 데이터가 있다면 계속 반복한다.
			while (receiveBufferReaminSize >= HEADERSIZE)
			{
				(var packetSize, var packetId) = GetPacketSizeAndId(buffer, receiveBufferOffset);
				if(packetSize > receiveBufferReaminSize)
				{
					break;
				}

				byte[] bodyData = null;
				var bodySize = packetSize - HEADERSIZE;
				if (bodySize > 0)
				{
					bodyData = new byte[bodySize];
				}
					
				var packet = new Packet(packetId, bodyData);
				callbackFunc(packet);

				receiveBufferOffset += packetSize;
				receiveBufferReaminSize -= packetSize;			
			}

			if(receiveBufferReaminSize > 0)
			{
				RemainBytes = receiveBufferReaminSize;
				Buffer.BlockCopy(buffer, receiveBufferOffset, SecondaryBuffer, 0, RemainBytes);				
			}
		}
			
		(bool, int, int) PacketProcessFromSecondaryBuffer(byte[] buffer, int offset, int size, Action<Packet> callbackFunc)
		{
			var receiveBufferOffset = offset;
			var receiveBufferReaminSize = size;

			if(RemainBytes == 0)
			{
				return (true, receiveBufferOffset, receiveBufferReaminSize);
			}


			if (RemainBytes < HEADERSIZE)
			{
				var copySize = RemainBytes - HEADERSIZE;
				if (copySize < receiveBufferReaminSize)
				{
					Buffer.BlockCopy(buffer, receiveBufferOffset, SecondaryBuffer, RemainBytes, copySize);
					receiveBufferOffset += copySize;
					receiveBufferReaminSize -= copySize;
					RemainBytes += copySize;
				}
				else
				{
					return (false, receiveBufferOffset, receiveBufferReaminSize);
				}
			}


			if (RemainBytes >= HEADERSIZE)
			{
				(var packetSize, var packetId) = GetPacketSizeAndId(SecondaryBuffer, 0);
				var bodySize = packetSize - HEADERSIZE;

				if(bodySize == 0)
				{
					var packet = new Packet(packetId, null);
					callbackFunc(packet);
				}
				else
				{
					if (bodySize > receiveBufferReaminSize)
					{
						Buffer.BlockCopy(buffer, receiveBufferOffset, SecondaryBuffer, RemainBytes, receiveBufferReaminSize);
						RemainBytes += receiveBufferReaminSize;
						return (false, receiveBufferOffset, receiveBufferReaminSize);
					}

					var bodyData = new byte[bodySize];
					Buffer.BlockCopy(buffer, receiveBufferOffset, bodyData, 0, bodySize);
					
					var packet = new Packet(packetId, bodyData);
					callbackFunc(packet);


					receiveBufferOffset += bodySize;
					receiveBufferReaminSize -= bodySize;
				}				
			}

			RemainBytes = 0;
			return (true, receiveBufferOffset, receiveBufferReaminSize);
		}

		/// <summary>
		/// 헤더+바디 사이즈를 구한다.
		/// 패킷 헤더부분에 이미 전체 메시지 사이즈가 계산되어 있으므로 헤더 크기에 맞게 변환만 시켜주면 된다.
		/// </summary>
		/// <returns></returns>
		(UInt16, UInt16) GetPacketSizeAndId(byte[] buffer, int offset)
		{
			var packetSize = FastBinaryRead.UInt16(buffer, offset);
			var packetId = FastBinaryRead.UInt16(buffer, offset + 2); 
			return (packetSize, packetId);
		}		
	}
}
