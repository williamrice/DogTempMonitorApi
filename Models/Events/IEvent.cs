namespace TempMonitor.Models.Events;

public interface IEvent
{
    DateTime OccurredAt { get; }
}
