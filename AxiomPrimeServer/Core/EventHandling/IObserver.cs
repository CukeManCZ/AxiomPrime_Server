public interface IEventHandler<T> where T : IEvent
{
    Task Handle(T evt);
}