using TempMonitor.Models.Events;

namespace TempMonitor.Services.Events;

public class EventProcessorHostedService : BackgroundService
{
    private readonly InMemoryEventBus _eventBus;
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<EventProcessorHostedService> _logger;

    public EventProcessorHostedService(
        IEventBus eventBus,
        IServiceProvider serviceProvider,
        ILogger<EventProcessorHostedService> logger)
    {
        _eventBus = (InMemoryEventBus)eventBus;
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Event Processor Hosted Service started");

        var reader = _eventBus.GetChannelReader();

        try
        {
            await foreach (var @event in reader.ReadAllAsync(stoppingToken))
            {
                try
                {
                    _logger.LogDebug(
                        "Processing event: {EventType}, TemperatureReadingId: {TemperatureReadingId}",
                        @event.GetType().Name,
                        (@event as SensorDataReceivedEvent)?.TemperatureReadingId ?? 0);

                    await _eventBus.InvokeHandlersAsync(@event, stoppingToken);

                    _logger.LogDebug(
                        "Event processed: {EventType}, TemperatureReadingId: {TemperatureReadingId}",
                        @event.GetType().Name,
                        (@event as SensorDataReceivedEvent)?.TemperatureReadingId ?? 0);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex,
                        "Error processing event: {EventType}",
                        @event.GetType().Name);
                }
            }
        }
        catch (OperationCanceledException)
        {
            _logger.LogInformation("Event Processor Hosted Service is stopping");
        }
    }

    public override async Task StopAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("Event Processor Hosted Service stopped");
        await base.StopAsync(cancellationToken);
    }
}
