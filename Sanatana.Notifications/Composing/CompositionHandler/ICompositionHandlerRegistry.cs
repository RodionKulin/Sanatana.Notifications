namespace Sanatana.Notifications.Composing
{
    public interface ICompositionHandlerRegistry<TKey> 
        where TKey : struct
    {
        ICompositionHandler<TKey> MatchHandler(int? handlerId);
    }
}