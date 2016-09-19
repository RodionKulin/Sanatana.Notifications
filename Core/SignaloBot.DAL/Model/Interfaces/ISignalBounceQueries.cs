using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SignaloBot.DAL
{
    public interface ISignalBounceQueries<TKey>
        where TKey : struct
    {
        Task<bool> Insert(List<SignalBounce<TKey>> items);
    }
}