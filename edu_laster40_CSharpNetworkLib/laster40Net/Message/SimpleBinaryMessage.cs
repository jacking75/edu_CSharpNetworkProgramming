using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using laster40Net.Util;

namespace laster40Net.Message
{
    public static class SimpleBinaryMessageProtocol
    {
        public static readonly int HEADERSIZE = 4;
        public static readonly int MAXPAYLOAD = 4 * 1024;
        public static readonly int MAXMESSAGE = HEADERSIZE + MAXPAYLOAD;
    }

    public class SimpleBinaryMessageFactory : IMessageFactory
    {
        public IMessageBuilder CreateBuilder()
        {
            return new SimpleBinaryMessageBuilder();
        }
        public IMessageResolver CreateResolver()
        {
            return new SimpleBinaryMessageResolver();
        }
    }

    /// <summary>
    /// 간단한 길이 헤더를 가진 binary 메세지 빌더
    /// 포멧 - header(4byte) + data
    ///  - header : data length(int) - 데이터 사이즈
    ///  - data : 실제 데이터
    /// </summary>
    public class SimpleBinaryMessageBuilder : IMessageBuilder
    {
        public void OnOpen(IMessageContext context) { }
        public void OnSend(IMessageContext context, ref List<ArraySegment<byte>> lists, ArraySegment<byte> payload)
        {
            // 헤더 만들어 넣고
            byte[] header = context.BufferManager.Take(SimpleBinaryMessageProtocol.HEADERSIZE);
            FastBitConverter.GetBytes(payload.Count, header, 0);
            lists.Add(new ArraySegment<byte>(header, 0, SimpleBinaryMessageProtocol.HEADERSIZE));

            // payload 적재하고
            byte[] copyPayload = context.BufferManager.Take(payload.Count);
            Array.Copy(payload.Array, payload.Offset, copyPayload, 0, payload.Count);
            lists.Add(new ArraySegment<byte>(copyPayload, 0, payload.Count));
        }
        public void OnClose(IMessageContext context) { }
    }


    public class SimpleBinaryMessageResolver : IMessageResolver
    {
        /// <summary>
        /// 실제메세지 사이즈
        /// </summary>
        private int _messageSize = 0;
        /// <summary>
        /// 현재 분석중인 버퍼
        /// </summary>
        private byte[] _messageBuffer = new byte[SimpleBinaryMessageProtocol.MAXMESSAGE];
        /// <summary>
        /// 현재 분석중인 인덱스 위치
        /// </summary>
        private int _messagePos = 0;
        
        public void OnOpen(IMessageContext context) {
            ResetMessageBuffer();
        }
        
        public bool OnReceive(IMessageContext context, ArraySegment<byte> arraySegment)
        {
            byte[] srcBuffer = arraySegment.Array;
            int srcEndIdx = arraySegment.Offset + arraySegment.Count;
            for (int srcIdx = arraySegment.Offset; srcIdx < srcEndIdx; ++srcIdx)
            {
                // 메세지 포인터가 범위를 넘어가면 안된다.
                if( _messagePos >= SimpleBinaryMessageProtocol.MAXMESSAGE )
                {
                    context.CloseMessageContext(CloseReason.MessageResolveError);
                    return true;
                }

                // 버퍼에 복사해 넣는다.
                _messageBuffer[_messagePos] = srcBuffer[srcIdx];
                ++_messagePos;

                // 메세지 size 를 구한다.
                if (_messageSize == 0 && _messagePos >= SimpleBinaryMessageProtocol.HEADERSIZE)
                {
                    _messageSize = GetPayloadLength() + SimpleBinaryMessageProtocol.HEADERSIZE;
                    if (_messageSize <= 0 || _messageSize >= SimpleBinaryMessageProtocol.MAXMESSAGE)
                    {
                        context.CloseMessageContext(CloseReason.MessageResolveError);
                        return true;
                    }
                }

                // 패킷이 완성되었으면 처리
                if (_messageSize != 0 && _messagePos == _messageSize)
                {
                    context.CompletedMessage(new ArraySegment<byte>(_messageBuffer, SimpleBinaryMessageProtocol.HEADERSIZE, _messageSize - SimpleBinaryMessageProtocol.HEADERSIZE));
                    ResetMessageBuffer();
                    continue;
                }
            }

            return true;
        }

        public void OnClose(IMessageContext context) {}
       
        private int GetPayloadLength()
        {
            int length = 0;
            length = FastBitConverter.ToInt32( _messageBuffer, 0);
            return length;
        }

        private void ResetMessageBuffer()
        {
            _messagePos = 0;
            _messageSize = 0;
        }
    }
}
