using MongoDB.Bson;
using Sanatana.Notifications.DAL;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Sanatana.MongoDb;
using MongoDB.Driver;
using Sanatana.Notifications.DAL.Interfaces;
using Sanatana.Notifications.DAL.Entities;

namespace Sanatana.Notifications.DAL.MongoDb.Queries
{
    public class MongoDbSubscriberScheduleSettingsQueries : ISubscriberScheduleSettingsQueries<ObjectId>
    {
        //fields
        protected MongoDbConnectionSettings _settings;
        
        protected SenderMongoDbContext _context;


        //init
        public MongoDbSubscriberScheduleSettingsQueries(MongoDbConnectionSettings connectionSettings)
        {
            
            _settings = connectionSettings;
            _context = new SenderMongoDbContext(connectionSettings);
        }



        //methods
        public virtual async Task Insert(List<SubscriberScheduleSettings<ObjectId>> periods)
        {
            var options = new InsertManyOptions()
            {
                IsOrdered = false
            };

            await _context.SubscriberReceivePeriods.InsertManyAsync(periods, options);
        }

        public virtual async Task<List<SubscriberScheduleSettings<ObjectId>>> Select(
            List<ObjectId> subscriberIds, List<int> receivePeriodSets = null)
        {
            var filter = Builders<SubscriberScheduleSettings<ObjectId>>.Filter.Where(
                    p => subscriberIds.Contains(p.SubscriberId));

            if (receivePeriodSets != null)
            {
                filter = Builders<SubscriberScheduleSettings<ObjectId>>.Filter.And(filter,
                    Builders<SubscriberScheduleSettings<ObjectId>>.Filter.Where(
                        p => receivePeriodSets.Contains(p.Set)));
            }

            List<SubscriberScheduleSettings<ObjectId>> list = await _context.SubscriberReceivePeriods.Find(filter).ToListAsync();

            return list;
        }

        public virtual async Task RewriteSets(ObjectId subscriberId
            , List<SubscriberScheduleSettings<ObjectId>> periods)
        {
            List<int> receivePeriodSets = periods
                    .Select(x => x.Set)
                    .Distinct()
                    .ToList();

            var requests = new List<WriteModel<SubscriberScheduleSettings<ObjectId>>>();

            var filter = Builders<SubscriberScheduleSettings<ObjectId>>.Filter.Where(
                p => p.SubscriberId == subscriberId
                && receivePeriodSets.Contains(p.Set));
            requests.Add(new DeleteManyModel<SubscriberScheduleSettings<ObjectId>>(filter));

            foreach (SubscriberScheduleSettings<ObjectId> item in periods)
            {
                requests.Add(new InsertOneModel<SubscriberScheduleSettings<ObjectId>>(item));
            }

            var options = new BulkWriteOptions()
            {
                IsOrdered = true
            };

            BulkWriteResult response = await _context.SubscriberReceivePeriods.BulkWriteAsync(requests, options);
        }

        public virtual async Task Delete(List<ObjectId> subscriberIds, List<int> receivePeriodSets = null)
        {
            var filter = Builders<SubscriberScheduleSettings<ObjectId>>.Filter.Where(
                    p => subscriberIds.Contains(p.SubscriberId));

            if (receivePeriodSets != null)
            {
                filter = Builders<SubscriberScheduleSettings<ObjectId>>.Filter.And(filter,
                    Builders<SubscriberScheduleSettings<ObjectId>>.Filter.Where(
                        p => receivePeriodSets.Contains(p.Set)));
            }

            DeleteResult response = await _context.SubscriberReceivePeriods.DeleteManyAsync(filter);
        }


    }
}
