using System.Collections.Concurrent;

public class EventBus
{
    private readonly ConcurrentDictionary<Type, List<object>> _handlers = new();

    public void Subscribe<T>(IEventHandler<T> handler) where T : IEvent
    {
        var type = typeof(T);

        _handlers.TryAdd(type, new List<object>());
        _handlers[type].Add(handler);
    }

    public async Task Publish<T>(T evt) where T : IEvent
    {
        var type = typeof(T);

        if (!_handlers.TryGetValue(type, out var handlers))
            return;

        foreach (var handler in handlers.Cast<IEventHandler<T>>())
        {
            await handler.Handle(evt);
        }
    }
}