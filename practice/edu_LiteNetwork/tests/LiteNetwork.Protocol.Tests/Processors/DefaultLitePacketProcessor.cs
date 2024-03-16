namespace LiteNetwork.Protocol.Tests.Processors;

public class DefaultLitePacketProcessor : LitePacketProcessor
{
    /// <summary>
    /// Creates a new <see cref="DefaultLitePacketProcessor"/> instance.
    /// </summary>
    /// <param name="includeHeader"></param>
    public DefaultLitePacketProcessor(bool includeHeader)
    {
        IncludeHeader = includeHeader;
    }
}
