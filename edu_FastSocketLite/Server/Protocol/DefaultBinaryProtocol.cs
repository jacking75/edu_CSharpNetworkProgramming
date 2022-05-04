using System;
using System.Collections.Generic;
using System.Text;

namespace FastSocketLite.Server.Protocol
{
    public sealed class DefaultBinaryProtocol :  IProtocol<Messaging.DefaultBinaryMessage>
    {
        /// <summary>
        /// parse
        /// </summary>
        /// <param name="connection"></param>
        /// <param name="buffer"></param>
        /// <param name="maxMessageSize"></param>
        /// <param name="readlength"></param>
        /// <returns></returns>
        /// <exception cref="BadProtocolException">bad command line protocol</exception>
        public Messaging.DefaultBinaryMessage Parse(SocketBase.IConnection connection, ArraySegment<byte> buffer,
                                                                                                         int maxMessageSize, out int readlength)
        {
            if (buffer.Count < Messaging.DefaultBinaryMessage.HEADER_SIZE)
            {
                readlength = 0;
                return null;
            }

            var startPos = buffer.Offset;
            var bufferLen = buffer.Offset + buffer.Count;
            var totalSize = BitConverter.ToUInt16(buffer.Array, startPos);

            if(totalSize > bufferLen)
            {
                readlength = 0;
                return null;
            }

            var bodyLen = totalSize - Messaging.DefaultBinaryMessage.HEADER_SIZE;
            var packetID = BitConverter.ToUInt16(buffer.Array, startPos + 2);
            var type = (SByte)buffer.Array[startPos + 4];
            var version = BitConverter.ToUInt16(buffer.Array, startPos + 5);
            byte[] body = null;

            if (bodyLen > 0)
            {
                body = new byte[bodyLen];
                Buffer.BlockCopy(buffer.Array, startPos + 7, body, 0, bodyLen);
            }

            readlength = totalSize;
            return new Messaging.DefaultBinaryMessage(totalSize, packetID, type, version, body);                        
        }
    }
}
