using LiteNetwork.Hosting;
using LiteNetwork.Server.Abstractions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;

namespace LiteNetwork.Server.Hosting;

/// <summary>
/// Provides extensions methods for the <see cref="ILiteBuilder"/> interface.
/// </summary>
public static class LiteServerBuilderExtensions
{
    /// <summary>
    /// Registers and initializes a <typeparamref name="TLiteServer"/> instance.
    /// </summary>
    /// <typeparam name="TLiteServer">Server type.</typeparam>
    /// <param name="builder">A <see cref="ILiteBuilder"/> to add server.</param>
    /// <param name="configure">Delegate to configure a <see cref="LiteServerOptions"/>.</param>
    /// <returns>The <see cref="ILiteBuilder"/>.</returns>
    public static ILiteBuilder AddLiteServer<TLiteServer>(this ILiteBuilder builder, Action<LiteServerOptions> configure)
        where TLiteServer : class, ILiteServer
    {
        if (builder is null)
        {
            throw new ArgumentNullException(nameof(builder));
        }

        builder.Services.AddSingleton<TLiteServer>(serviceProvider => ConfigureServer<TLiteServer>(serviceProvider, configure));
        builder.Services.AddLiteServerHostedService<TLiteServer>();

        return builder;
    }

    /// <summary>
    /// Registers and initializes a <typeparamref name="TLiteServerImplementation"/> instance.
    /// </summary>
    /// <typeparam name="TLiteServer">LiteServer abstraction.</typeparam>
    /// <typeparam name="TLiteServerImplementation">LiteServer implementation.</typeparam>
    /// <param name="builder">A <see cref="ILiteBuilder"/> to add server.</param>
    /// <param name="configure">Delegate to configure a <see cref="LiteServerOptions"/>.</param>
    /// <returns>The <see cref="ILiteBuilder"/>.</returns>
    public static ILiteBuilder AddLiteServer<TLiteServer, TLiteServerImplementation>(this ILiteBuilder builder, Action<LiteServerOptions> configure)
        where TLiteServer : class
        where TLiteServerImplementation : class, TLiteServer, ILiteServer
    {
        if (builder is null)
        {
            throw new ArgumentNullException(nameof(builder));
        }

        builder.Services.AddSingleton<TLiteServerImplementation>(serviceProvider => ConfigureServer<TLiteServerImplementation>(serviceProvider, configure));
        builder.Services.AddSingleton<TLiteServer, TLiteServerImplementation>(serviceProvider => serviceProvider.GetRequiredService<TLiteServerImplementation>());
        builder.Services.AddLiteServerHostedService<TLiteServerImplementation>();

        return builder;
    }

    private static TLiteServer ConfigureServer<TLiteServer>(IServiceProvider serviceProvider, Action<LiteServerOptions> configure)
        where TLiteServer : class, ILiteServer
    {
        LiteServerOptions options = new();
        configure(options);

        return ActivatorUtilities.CreateInstance<TLiteServer>(serviceProvider, options);
    }

    private static void AddLiteServerHostedService<TLiteServer>(this IServiceCollection services)
        where TLiteServer : class, ILiteServer
    {
        services.AddSingleton<IHostedService>(serviceProvider =>
        {
            return new LiteServerHostedService(serviceProvider.GetRequiredService<TLiteServer>());
        });
    }
}
