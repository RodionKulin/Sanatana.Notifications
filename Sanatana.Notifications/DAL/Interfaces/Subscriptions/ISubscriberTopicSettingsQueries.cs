﻿using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Sanatana.Notifications.DAL.Results;
using Sanatana.Notifications.DAL.Entities;

namespace Sanatana.Notifications.DAL.Interfaces
{
    public interface ISubscriberTopicSettingsQueries<TKey>
        where TKey : struct
    {
        Task Insert(List<SubscriberTopicSettings<TKey>> settings);
        Task<SubscriberTopicSettings<TKey>> Select(TKey subscriberId, int categoryId, string topicId);
        Task<TotalResult<List<SubscriberTopicSettings<TKey>>>> SelectPage(int page, int pageSize,
             List<TKey> subscriberIds = null,  List<int> deliveryTypeIds = null, List<int> categoryIds = null, List<string> topics = null);
        Task UpdateIsEnabled(List<SubscriberTopicSettings<TKey>> items);
        Task UpsertIsEnabled(List<SubscriberTopicSettings<TKey>> items);
        Task UpdateIsDeleted(SubscriberTopicSettings<TKey> settings);
        Task Upsert(SubscriberTopicSettings<TKey> settings, bool updateExisting);
        Task Delete(TKey subscriberId);
        Task Delete(List<TKey> subscriberIds = null, List<int> deliveryTypeIds = null
            , List<int> categoryIds = null, List<string> topicIds = null);
    }
}