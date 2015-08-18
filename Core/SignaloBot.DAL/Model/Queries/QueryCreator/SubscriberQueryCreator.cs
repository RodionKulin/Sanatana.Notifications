using SignaloBot.DAL.Context;
using SignaloBot.DAL.Entities.Core;
using SignaloBot.DAL.Entities.Parameters;
using SignaloBot.DAL.Entities.Results;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SignaloBot.DAL.Model.Queries.QueryCreator
{
    public class SubscriberQueryCreator
    {

        //методы
        public IQueryable<Subscriber> CreateQuery(SubscriberParameters parameters, ClientDbContext context)
        {
            IQueryable<UserDeliveryTypeSettings> typeQueryPart = CreateDeliveryTypeQueryPart(parameters, context);

            if (parameters.SelectFromCategories)
            {
                IQueryable<UserCategorySettings> categoryQueryPart = CreateCategoryQueryPart(parameters, context);
                typeQueryPart = JoinWithCategories(parameters, typeQueryPart, categoryQueryPart);
            }

            if (parameters.SelectFromTopics)
            {
                IQueryable<UserTopicSettings> topicQueryPart = CreateTopicQueryPart(parameters, context);
                typeQueryPart = JoinWithTopics(parameters, typeQueryPart, topicQueryPart);
            }

            IQueryable<Subscriber> subscribers = typeQueryPart.Select(d => new Subscriber
            {
                UserID = d.UserID,
                Address = d.Address,
                TimeZoneID = d.TimeZoneID
            });
            
            return subscribers;
        }


        //DeliveryType
        protected virtual IQueryable<UserDeliveryTypeSettings> CreateDeliveryTypeQueryPart(
            SubscriberParameters parameters, ClientDbContext context)
        {
            IQueryable<UserDeliveryTypeSettings> query = context.UserDeliveryTypeSettings;

            query = query.Where(p => p.Address != null && p.DeliveryType == parameters.DeliveryType);

            if (parameters.CheckBlockedOfNDR)
            {
                query = query.Where(p => p.IsBlockedOfNDR == false);
            }

            if (parameters.CheckTypeEnabled)
            {
                query = query.Where(p => p.IsEnabled == true);
            }

            if (parameters.CheckTypeLastSendDate)
            {
                query = query.Where(p => p.LastSendDateUtc == null ||
                    p.LastSendDateUtc < p.LastUserVisitUtc);
            }

            if (parameters.CheckTypeSendCountNotGreater != null)
            {
                int sendCountLimitValue = parameters.CheckTypeSendCountNotGreater.Value;
                query = query.Where(p => p.SendCount < sendCountLimitValue);
            }

            if (parameters.FromUserIDList != null)
            {
                query = query.Where(p => parameters.FromUserIDList.Contains(p.UserID));
            }

            return query;
        }
        
        //Category
        protected virtual IQueryable<UserCategorySettings> CreateCategoryQueryPart(
            SubscriberParameters parameters, ClientDbContext context)
        {
            IQueryable<UserCategorySettings> query = context.UserCategorySettings;

            if (parameters.CheckCategoryEnabled)
            {
                query = query.Where(p => p.IsEnabled == true);
            }

            if (parameters.CheckCategorySendCountNotGreater != null)
            {
                int sendCountLimitValue = parameters.CheckCategorySendCountNotGreater.Value;
                query = query.Where(p => p.SendCount < sendCountLimitValue);
            }

            return query;
        }

        protected virtual IQueryable<UserDeliveryTypeSettings> JoinWithCategories(
            SubscriberParameters parameters, IQueryable<UserDeliveryTypeSettings> typeQueryPart
            , IQueryable<UserCategorySettings> categoryQueryPart)
        {
            int categoryIDValue = parameters.CategoryID.Value;

            IQueryable<UserDeliveryTypeSettings> query = null;

            if (parameters.CheckCategoryLastSendDate)
            {
                query = from d in typeQueryPart
                        join c in categoryQueryPart on
                        new
                        {
                            UserID = d.UserID,
                            DeliveryType = d.DeliveryType,
                            CategoryID = categoryIDValue
                        } equals new
                        {
                            UserID = c.UserID,
                            DeliveryType = c.DeliveryType,
                            CategoryID = c.CategoryID
                        }
                        into gr
                        from catGroup in gr.DefaultIfEmpty()
                        where catGroup == null
                        || catGroup.LastSendDateUtc == null
                        || d.LastUserVisitUtc == null
                        || catGroup.LastSendDateUtc < d.LastUserVisitUtc
                        select d;
            }
            else
            {
                query = from d in typeQueryPart
                        join c in categoryQueryPart on
                        new
                        {
                            UserID = d.UserID,
                            DeliveryType = d.DeliveryType,
                            CategoryID = categoryIDValue
                        } equals new
                        {
                            UserID = c.UserID,
                            DeliveryType = c.DeliveryType,
                            CategoryID = c.CategoryID
                        }
                        into gr
                        from catGroup in gr.DefaultIfEmpty()
                        select d;
            }

            return query;
        }
        
        //Topic
        protected virtual IQueryable<UserTopicSettings> CreateTopicQueryPart(
            SubscriberParameters parameters, ClientDbContext context)
        {
            IQueryable<UserTopicSettings> query = context.UserTopicSettings;

            query = query.Where(p => p.IsDeleted == false);

            if (parameters.CheckTopicEnabled)
            {
                query = query.Where(p => p.IsEnabled == true);
            }

            if (parameters.CheckTopicSendCountNotGreater != null)
            {
                int sendCountLimitValue = parameters.CheckTopicSendCountNotGreater.Value;
                query = query.Where(p => p.SendCount < sendCountLimitValue);
            }

            return query;
        }

        protected virtual IQueryable<UserDeliveryTypeSettings> JoinWithTopics(SubscriberParameters parameters
            , IQueryable<UserDeliveryTypeSettings> typeQueryPart, IQueryable<UserTopicSettings> topicQueryPart)
        {
            int categoryIDValue = parameters.CategoryID.Value;
            int topicIDValue = parameters.TopicID.Value;

            IQueryable<UserDeliveryTypeSettings> query = null;

            if (parameters.CheckTopicLastSendDate)
            {
                query = from d in typeQueryPart
                        join t in topicQueryPart on
                        new
                        {
                            UserID = d.UserID,
                            DeliveryType = d.DeliveryType,
                            CategoryID = categoryIDValue,
                            TopicID = topicIDValue
                        } equals new
                        {
                            UserID = t.UserID,
                            DeliveryType = t.DeliveryType,
                            CategoryID = t.CategoryID,
                            TopicID = t.TopicID
                        }
                        where t.LastSendDateUtc == null
                        || d.LastUserVisitUtc == null
                        || t.LastSendDateUtc < d.LastUserVisitUtc
                        select d;
            }
            else
            {
                query = from d in typeQueryPart
                        join t in topicQueryPart on
                        new
                        {
                            UserID = d.UserID,
                            DeliveryType = d.DeliveryType,
                            CategoryID = categoryIDValue,
                            TopicID = topicIDValue
                        } equals new
                        {
                            UserID = t.UserID,
                            DeliveryType = t.DeliveryType,
                            CategoryID = t.CategoryID,
                            TopicID = t.TopicID
                        }
                        select d;
            }

            return query;
        }

    }
}
