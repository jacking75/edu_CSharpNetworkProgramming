using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;

namespace LiteNetwork.Internal;

/// <summary>
/// Provides network helper methods.
/// </summary>
internal static class LiteNetworkHelpers
{
    /// <summary>
    /// Creates a new <see cref="IPEndPoint"/> with the specified IP or host and a port number
    /// as an asynchronous operation.
    /// </summary>
    /// <param name="ipOrHost">IP or Host address.</param>
    /// <param name="port">Port number.</param>
    /// <returns>A <see cref="Task{TResult}"/> that representing the asynchronous operation. 
    /// The result returns a <see cref="IPEndPoint"/> with the specified IP or host and port number.></returns>
    public static async Task<IPEndPoint> CreateIpEndPointAsync(string ipOrHost, int port)
    {
        IPAddress address = ipOrHost == IPAddress.Any.ToString() ?
            IPAddress.Any :
            (await Dns.GetHostAddressesAsync(ipOrHost).ConfigureAwait(false)).First(x => x.AddressFamily == AddressFamily.InterNetwork);

        return new IPEndPoint(address, port);
    }
}
