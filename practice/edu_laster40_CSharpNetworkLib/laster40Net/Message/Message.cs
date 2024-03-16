using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using laster40Net.Util;

namespace laster40Net.Message
{
    /// <summary>
    /// 메세지 Builder와 Resolver에서 사용할수 있는 Context
    /// </summary>
    public interface IMessageContext
    {
        /// <summary>
        /// 고유 아이디
        /// </summary>
        long ID { get; }

        /// <summary>
        /// 메모리 관리자
        /// </summary>
        IBufferManager BufferManager { get; }
        
        /// <summary>
        /// Remote EndPoint
        /// </summary>
        EndPoint RemoteEndPoint { get; }

        /// <summary>
        /// 메세지 빌더
        /// </summary>
        IMessageBuilder MessageBuilder { get; }
        /// <summary>
        /// 메세지 Resolver
        /// </summary>
        IMessageResolver MessageResolver { get; }

        /// <summary>
        /// 접속을 해제한다.
        /// </summary>
        void CloseMessageContext(CloseReason closeReason);

        /// <summary>
        /// 메세지가 완료되었음
        /// </summary>
        /// <param name="message"></param>
        void CompletedMessage(ArraySegment<byte> message);
    }

    public interface IMessageFactory
    {
        IMessageBuilder CreateBuilder();
        IMessageResolver CreateResolver();
    }

    //TODO ArraySegment<byte>를 Span으로 바꾼다

    /// <summary>
    /// 메세지를 전송하기 위해서 만들어준다.
    /// </summary>
    public interface IMessageBuilder
    {
        /// <summary>
        /// 세션이 오픈 되었다.
        /// </summary>
        /// <param name="context">메세지 Context</param>
        void OnOpen(IMessageContext context);
        /// <summary>
        /// 메세지를 만들어야한다.
        /// </summary>
        /// <param name="context">메세지 Context</param>
        /// <param name="lists">만들어진 메세지 버퍼들의 리스트</param>
        /// <param name="payload">실제로 적재된 데이터</param>
        void OnSend(IMessageContext context, ref List<ArraySegment<byte>> lists, ArraySegment<byte> payload);
        /// <summary>
        /// 세션이 닫혔음
        /// </summary>
        void OnClose(IMessageContext context);
    }

    /// <summary>
    /// 메세지를 받아서 풀어준다.
    /// </summary>
    public interface IMessageResolver
    {
        /// <summary>
        /// 세션이 열였음
        /// </summary>
        /// <param name="context"></param>
        void OnOpen(IMessageContext context);
        /// <summary>
        /// 받았으니 패킷을 찢어 봐야함
        /// </summary>
        /// <param name="context"></param>
        /// <param name="arraySegment"></param>
        /// <returns>
        /// 패킷을 처리 했으니 따로 Event을 호출하지 않아도 된다.
        /// </returns>
        bool OnReceive(IMessageContext context, ArraySegment<byte> arraySegment);
        /// <summary>
        /// 세션이 끊어짐
        /// </summary>
        /// <param name="context"></param>
        void OnClose(IMessageContext context);
    }

    

}
