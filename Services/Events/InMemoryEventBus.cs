using System.Threading.Channels;
using TempMonitor.Models.Events;

namespace TempMonitor.Services.Events;

public class InMemoryEventBus : IEventBus
{
    private readonly Channel<IEvent> _channel;
    private readonly Dictionary<Type, List<Delegate>> _handlers;
    private readonly ILogger<InMemoryEventBus> _logger;
    private readonly object _lock = new();

    public InMemoryEventBus(ILogger<InMemoryEventBus> logger)
    {
        _logger = logger;
        _handlers = new Dictionary<Type, List<Delegate>>();
        
        // Create unbounded channel for event processing
        var options = new UnboundedChannelOptions
        {
            SingleWriter = false,
            SingleReader = true
        };
        _channel = Channel.CreateUnbounded<IEvent>(options);
    }

    public async Task PublishAsync<TEvent>(TEvent @event, CancellationToken cancellationToken = default) where TEvent : IEvent
    {
        try
        {
            await _channel.Writer.WriteAsync(@event, cancellationToken);
            _logger.LogDebug(
                "Event published: {EventType}, TemperatureReadingId: {TemperatureReadingId}",
                typeof(TEvent).Name,
                (@event as SensorDataReceivedEvent)?.TemperatureReadingId ?? 0);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, 
                "Failed to publish event: {EventType}",
                typeof(TEvent).Name);
            throw;
        }
    }

    public void Subscribe<TEvent>(Func<TEvent, CancellationToken, Task> handler) where TEvent : IEvent
    {
        lock (_lock)
        {
            var eventType = typeof(TEvent);
            if (!_handlers.ContainsKey(eventType))
            {
                _handlers[eventType] = new List<Delegate>();
            }

            _handlers[eventType].Add(handler);
            _logger.LogInformation("Handler subscribed for event type: {EventType}", eventType.Name);
        }
    }

    public ChannelReader<IEvent> GetChannelReader() => _channel.Reader;

    public async Task InvokeHandlersAsync(IEvent @event, CancellationToken cancellationToken)
    {
        var eventType = @event.GetType();
        List<Delegate>? handlers;

        lock (_lock)
        {
            _handlers.TryGetValue(eventType, out handlers);
        }

        if (handlers == null || handlers.Count == 0)
        {
            _logger.LogWarning("No handlers registered for event type: {EventType}", eventType.Name);
            return;
        }

        var tasks = new List<Task>();
        foreach (var handler in handlers)
        {
            try
            {
                var task = (Task)handler.DynamicInvoke(@event, cancellationToken)!;
                tasks.Add(task);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, 
                    "Error invoking handler for event type: {EventType}",
                    eventType.Name);
            }
        }

        await Task.WhenAll(tasks);
    }
}
