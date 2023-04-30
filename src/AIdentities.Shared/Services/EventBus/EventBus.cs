using EventAggregator.Blazor;

namespace AIdentities.Shared.Services.EventBus;
public class EventBus : IEventBus
{
   private readonly ILogger<EventBus> _logger;

   private IEventAggregator _eventAggregator { get; set; }

   public EventBus(ILogger<EventBus> logger, IEventAggregator eventAggregator)
   {
      _logger = logger;
      _eventAggregator = eventAggregator;
   }

   public void Subscribe(object subscriber)
   {
      _eventAggregator.Subscribe(subscriber);
   }

   public void Unsubscribe(object subscriber)
   {
      _eventAggregator.Unsubscribe(subscriber);
   }

   public Task Publish(object @event) => _eventAggregator.PublishAsync(@event);
}
