using SignaloBot.DAL.Entities.Parameters;
using SignaloBot.DAL.Entities.Results;
using System;
namespace SignaloBot.DAL.Queries.Client
{
    public interface ISubscriberQueries
    {
        System.Collections.Generic.List<Subscriber> Select(SubscriberParameters parameters, out Exception exception);
    }
}
