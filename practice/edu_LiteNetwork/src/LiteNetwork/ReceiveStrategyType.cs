namespace LiteNetwork;

/// <summary>
/// Defines the different receive strategy types available.
/// </summary>
public enum ReceiveStrategyType
{
    /// <summary>
    /// The default strategy. Handles the incoming packets right after they got received.
    /// </summary>
    Default,

    /// <summary>
    /// Place the received packet into a receive queue. The packet will be processed in the same order it got received.
    /// </summary>
    Queued
}
