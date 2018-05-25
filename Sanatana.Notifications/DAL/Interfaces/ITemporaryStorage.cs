using Sanatana.Notifications.DAL.Parameters;
using System;
using System.Collections.Generic;

namespace Sanatana.Notifications.DAL.Interfaces
{
    public interface ITemporaryStorage<TS>
    {
        void Insert(TemporaryStorageParameters queueParams, Guid Id, TS item);
        Dictionary<Guid, TS> Select(TemporaryStorageParameters queueParams);
        void Update(TemporaryStorageParameters queueParams, Guid Id, TS item);
        void Delete(TemporaryStorageParameters queueParams, Guid Id);
        void Delete(TemporaryStorageParameters queueParams, List<Guid> Ids);
    }
}