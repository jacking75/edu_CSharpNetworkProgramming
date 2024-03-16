namespace ServerNet
{
    // √‚√≥: https://github.com/Horusiath/clusterpack/tree/master/src/ClusterPack/Transport 

    public class TcpTransportOptions
    {
        public int IncomingMessageBufferSize { get; set; } = 1024;
        public int Backlog { get; set; } = 100;
    }
}