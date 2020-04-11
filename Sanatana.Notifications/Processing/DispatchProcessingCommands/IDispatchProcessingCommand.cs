using Sanatana.Notifications.DAL.Entities;
using Sanatana.Notifications.Models;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Sanatana.Notifications.Processing.DispatchProcessingCommands
{
    public interface IDispatchProcessingCommand<TKey>
        where TKey : struct
    {
        int Order { get; }
        Task<bool> Execute(SignalWrapper<SignalDispatch<TKey>> item);
    }
}
