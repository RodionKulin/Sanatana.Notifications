namespace Sanatana.Notifications.EventsHandling
{
    public interface ICompositionHandlerRegistry<TKey> 
        where TKey : struct
    {
        ICompositionHandler<TKey> MatchHandler(int? handlerId);
    }
}