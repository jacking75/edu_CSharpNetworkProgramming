using System;

namespace LiteNetwork.Exceptions;

/// <summary>
/// Describes a basic exception that occured on a <see cref="LiteNetwork.Client.LiteClient"/>
/// </summary>
public class LiteClientException : Exception
{
    /// <summary>
    /// Creates a new <see cref="LiteClientException"/> instance.
    /// </summary>
    public LiteClientException()
        : this(string.Empty)
    {
    }

    /// <summary>
    /// Creates a new <see cref="LiteClientException"/> instance.
    /// </summary>
    /// <param name="message">Exception message.</param>
    public LiteClientException(string message)
        : this(message, null!)
    {
    }

    /// <summary>
    /// Creates a new <see cref="LiteClientException"/> instance.
    /// </summary>
    /// <param name="innerException">Inner exception.</param>
    public LiteClientException(Exception innerException)
        : this(string.Empty, innerException)
    {
    }

    /// <summary>
    /// Creates a new <see cref="LiteClientException"/> instance.
    /// </summary>
    /// <param name="message">Exception message.</param>
    /// <param name="innerException">Inner exception.</param>
    public LiteClientException(string message, Exception innerException)
        : base(message, innerException)
    {
    }
}
