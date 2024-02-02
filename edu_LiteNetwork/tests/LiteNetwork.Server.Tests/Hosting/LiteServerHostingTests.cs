using LiteNetwork.Hosting;
using LiteNetwork.Server.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;
using Xunit;

namespace LiteNetwork.Server.Tests.Hosting;

public class LiteServerHostingTests
{
    [Fact]
    public void SetupCustomLiteServerHostTest()
    {
        IHost host = new HostBuilder()
            .ConfigureLiteNetwork(builder =>
            {
                builder.AddLiteServer<CustomServer>(options =>
                {
                    options.Host = "127.0.0.1";
                    options.Port = 4444;
                });
            })
            .Build();

        using (host)
        {
            var server = host.Services.GetRequiredService<CustomServer>();

            Assert.NotNull(server);
            Assert.IsType<CustomServer>(server);
            Assert.True(server is LiteServer<CustomUser>);
        }
    }

    private class CustomUser : LiteServerUser
    {
    }

    private class CustomServer : LiteServer<CustomUser>
    {
        public CustomServer(LiteServerOptions options, IServiceProvider serviceProvider) 
            : base(options, serviceProvider)
        {
        }
    }
}
