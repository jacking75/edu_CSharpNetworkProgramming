using System;

namespace FastSocketLite.SocketBase
{
    //TODO ArraySegment를 Span으로 바꾸기
    /// <summary>
    /// 메시지 처리 handler
    /// </summary>
    /// <param name="buffer"></param>
    /// <param name="readlength"></param>
    public delegate void MessageProcessHandler(ArraySegment<byte> buffer, int readlength);

    /// <summary>
    /// message received eventArgs
    /// </summary>
    public sealed class MessageReceivedEventArgs
    {
        /// <summary>
        /// process callback
        /// </summary>
        private readonly MessageProcessHandler _processCallback = null;

        //TODO ArraySegment를 Span으로 바꾸기
        /// <summary>
        /// Buffer
        /// </summary>
        public readonly ArraySegment<byte> Buffer;

        //TODO ArraySegment를 Span으로 바꾸기
        /// <summary>
        /// new
        /// </summary>
        /// <param name="buffer"></param>
        /// <param name="processCallback"></param>
        /// <exception cref="ArgumentNullException">processCallback is null</exception>
        public MessageReceivedEventArgs(ArraySegment<byte> buffer, MessageProcessHandler processCallback)
        {
            if (processCallback == null) throw new ArgumentNullException("processCallback");
            this.Buffer = buffer;
            this._processCallback = processCallback;
        }

        public void SetReadlength(int readlength)
        {
            this._processCallback(this.Buffer, readlength);
        }
    }
}