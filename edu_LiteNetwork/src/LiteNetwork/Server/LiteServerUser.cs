using System.Threading.Tasks;

namespace LiteNetwork.Server;

/// <summary>
/// Provides a basic user implementation that can be used for a <see cref="LiteServer{TUser}"/>.
/// </summary>
public class LiteServerUser : LiteConnection
{
    /// <summary>
    /// Gets the server context.
    /// </summary>
    protected internal LiteServerContext? Context { get; internal set; }

    /// <summary>
    /// Creates a new <see cref="LiteServerUser"/> instance.
    /// </summary>
    protected LiteServerUser()
    {
    }

    /// <summary>
    /// Called when this user has been Connected.
    /// </summary>
    protected internal virtual void OnConnected()
    {
    }

    /// <summary>
    /// Called when this user has been Disconnected.
    /// </summary>
    protected internal virtual void OnDisconnected()
    {
    }

    /// <inheritdoc/>
    public override Task HandleMessageAsync(byte[] packetBuffer) => Task.CompletedTask;
}
