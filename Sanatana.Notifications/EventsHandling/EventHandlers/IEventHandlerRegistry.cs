namespace Sanatana.Notifications.EventsHandling
{
    public interface IEventHandlerRegistry<TKey> 
        where TKey : struct
    {
        IEventHandler<TKey> MatchHandler(int? handlerId);
    }
}