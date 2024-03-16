using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace laster40Net.Message
{
    public class RawMessageFactory : IMessageFactory
    {
        public IMessageBuilder CreateBuilder()
        {
            return new RawMessageBuilder();
        }
        public IMessageResolver CreateResolver()
        {
            return new RawMessageResolver();
        }
    }

    /// <summary>
    /// Raw데이터를 그대로 넘기는 메세지 빌더
    /// </summary>
    public class RawMessageBuilder : IMessageBuilder
    {
        public void OnOpen(IMessageContext context) { }
        public void OnSend(IMessageContext context, ref List<ArraySegment<byte>> lists, ArraySegment<byte> payload)
        {
            //TODO ArraySegment를 사용하는데 왜 복사를 하나???

            // payload 적재하고
            byte[] copyPayload = context.BufferManager.Take(payload.Count);
            Array.Copy(payload.Array, payload.Offset, copyPayload, 0, payload.Count);
            lists.Add(new ArraySegment<byte>(copyPayload, 0, payload.Count));
        }

        public void OnClose(IMessageContext context) { }
    }

    /// <summary>
    /// Raw 데이터 Resolver (아무것도 안한다-0-)
    /// </summary>
    public class RawMessageResolver : IMessageResolver
    {
        public void OnOpen(IMessageContext context) { }
        public bool OnReceive(IMessageContext context, ArraySegment<byte> arraySegment)
        {
            return false;
        }
        public void OnClose(IMessageContext context) { }
    }
}
