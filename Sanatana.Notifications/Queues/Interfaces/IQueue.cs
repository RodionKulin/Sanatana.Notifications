﻿using Sanatana.Notifications.DAL;
using Sanatana.Notifications.Monitoring;
using Sanatana.Notifications.Processing;
using Sanatana.Notifications.DispatchHandling;
using System;
using Sanatana.Notifications.Models;
using System.Collections.Generic;

namespace Sanatana.Notifications.Queues
{
    public interface IQueue<TSignal>
    {
        int PersistBeginOnItemsCount { get; }

        void Append(List<TSignal> items, bool isPermanentlyStored);
        void Append(SignalWrapper<TSignal> item);
        SignalWrapper<TSignal> DequeueNext();
        void ApplyResult(SignalWrapper<TSignal> item, ProcessingResult result);
        bool CheckIsEmpty(System.Collections.Generic.List<int> activeKeys);
        int CountQueueItems();
        void RestoreFromTemporaryStorage();
    }
}