using LiteNetwork.Internal;
using System;
using Xunit;

namespace LiteNetwork.Network.Tests;

public class NetHelperTests
{
    [Theory]
    [InlineData("127.0.0.1", 4444)]
    [InlineData("92.5.1.44", 8080)]
    [InlineData("156.16.255.55", 4444)]
    [InlineData("", 8080)]
    [InlineData("0.0.0.0", 4444)]
    public async void CreateValidIPEndPoint(string ipAddress, int port)
    {
        var ipEndPoint = await LiteNetworkHelpers.CreateIpEndPointAsync(ipAddress, port);

        Assert.NotNull(ipEndPoint);
    }

    [Theory]
    [InlineData(-2)]
    [InlineData(-18334)]
    public void CreateIPEndPointWithInvalidPort(int port)
    {
        Assert.ThrowsAsync<ArgumentOutOfRangeException>(() => LiteNetworkHelpers.CreateIpEndPointAsync("127.0.0.1", port));
    }

    [Theory]
    [InlineData("143.34.33.243435")]
    [InlineData("143.34.33.-1")]
    [InlineData("InvalidHost")]
    [InlineData(null)]
    public void CreateIPEndPointWithInvalidIPOrHost(string host)
    {
        if (host is null)
        {
            Assert.ThrowsAsync<ArgumentNullException>(() => LiteNetworkHelpers.CreateIpEndPointAsync(host, 4444));
        }
        else
        {
            Assert.ThrowsAsync<AggregateException>(() => LiteNetworkHelpers.CreateIpEndPointAsync(host, 4444));
        }
    }
}
