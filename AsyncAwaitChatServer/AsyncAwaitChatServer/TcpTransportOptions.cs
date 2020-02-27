namespace ServerNet
{
    public class TcpTransportOptions
    {
        public int IncomingMessageBufferSize { get; set; } = 1024;
        public int Backlog { get; set; } = 100;
    }
}