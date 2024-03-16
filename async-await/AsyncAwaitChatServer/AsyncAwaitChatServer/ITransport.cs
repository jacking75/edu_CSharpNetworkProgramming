using System.Buffers;
using System.Collections.Generic;
using System.Net;
using System.Threading;
using System.Threading.Tasks;

namespace ServerNet
{
    // √‚√≥: https://github.com/Horusiath/clusterpack/tree/master/src/ClusterPack/Transport 

    /// <summary>
    /// The transport interface used by membership service to provide means of communication between members.
    /// </summary>
    /// <seealso cref="TcpTransport"/>
    public interface ITransport
    {
        /// <summary>
        /// Asynchronously sends provided <paramref name="payload"/> to a given <paramref name="destination"/>.
        /// Completes once the whole <paramref name="payload"/> has been received and acknowledged by remote side.
        /// </summary>
        ValueTask SendAsync(IPEndPoint destination, ReadOnlySequence<byte> payload, CancellationToken cancellationToken);
        
        /// <summary>
        /// Binds current transport to the given <paramref name="endpoint"/>, returning an asynchronous sequence
        /// of serialized messages send to that <paramref name="endpoint"/>.
        /// </summary>
        IAsyncEnumerable<IncomingMessage> BindAsync(EndPoint endpoint, CancellationToken cancellationToken);
    }
}