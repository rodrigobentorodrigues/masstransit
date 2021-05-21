using System;

namespace MassTransitStarted.Contracts
{
    public interface SubmitOrder
    {
        Guid Id { get; }
        DateTime Timestamp { get; }
        string CustomerNumber { get; }
    }
}
