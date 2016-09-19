using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SignaloBot.DAL.SQL
{
    public class SubscriberQueryCreator
    {

        //методы
        public IQueryable<Subscriber<Guid>> CreateQuery(SubscribtionParameters parameters
            , UsersRangeParameters<Guid> usersRange, ClientDbContext context)
        {
            if(usersRange.UserIDRangeFromExcludingSelf != null 
                || usersRange.UserIDRangeToIncludingSelf != null)
            {
                throw new NotImplementedException("Not supported for SQL DAL");
            }



            IQueryable<UserDeliveryTypeSettings<Guid>> typeQueryPart = 
                CreateDeliveryTypeQueryPart(parameters, usersRange, context);

            if (parameters.SelectFromCategories)
            {
                IQueryable<UserCategorySettings<Guid>> categoryQueryPart = CreateCategoryQueryPart(parameters, context);
                typeQueryPart = JoinWithCategories(parameters, typeQueryPart, categoryQueryPart);
            }

            if (parameters.SelectFromTopics)
            {
                IQueryable<UserTopicSettings<Guid>> topicQueryPart = CreateTopicQueryPart(parameters, context);
                typeQueryPart = JoinWithTopics(parameters, typeQueryPart, topicQueryPart);
            }

            IQueryable<Subscriber<Guid>> subscribers = typeQueryPart.Select(d => new Subscriber<Guid>
            {
                UserID = d.UserID,
                DeliveryType = d.DeliveryType,
                Address = d.Address,
                TimeZoneID = d.TimeZoneID,
                Language = d.Language
            });   
            
            if(usersRange.Limit != null)
            {
                subscribers = subscribers.Take(usersRange.Limit.Value);
            }
                     
            return subscribers;
        }


        //DeliveryType
        protected virtual IQueryable<UserDeliveryTypeSettings<Guid>> CreateDeliveryTypeQueryPart(
            SubscribtionParameters parameters, UsersRangeParameters<Guid> usersRange, ClientDbContext context)
        {
            IQueryable<UserDeliveryTypeSettings<Guid>> query = context.UserDeliveryTypeSettings
                .Where(p => p.Address != null);

            if (parameters.DeliveryType != null)
            {
                query = query.Where(p => p.DeliveryType == parameters.DeliveryType.Value);
            }

            if (parameters.CheckBlockedOfNDR)
            {
                query = query.Where(p => p.IsBlockedOfNDR == false);
            }

            if (parameters.CheckDeliveryTypeEnabled)
            {
                query = query.Where(p => p.IsEnabled == true);
            }

            if (parameters.CheckDeliveryTypeLastSendDate)
            {
                query = query.Where(p => p.LastSendDateUtc == null ||
                    p.LastSendDateUtc < p.LastUserVisitUtc);
            }

            if (parameters.CheckDeliveryTypeSendCountNotGreater != null)
            {
                int sendCountLimitValue = parameters.CheckDeliveryTypeSendCountNotGreater.Value;
                query = query.Where(p => p.SendCount <= sendCountLimitValue);
            }

            if (usersRange.FromUserIDs != null)
            {
                query = query.Where(p => usersRange.FromUserIDs.Contains(p.UserID));
            }
            
            return query;
        }
        
        //Category
        protected virtual IQueryable<UserCategorySettings<Guid>> CreateCategoryQueryPart(
            SubscribtionParameters parameters, ClientDbContext context)
        {
            IQueryable<UserCategorySettings<Guid>> query = context.UserCategorySettings;

            if (parameters.CheckCategoryEnabled)
            {
                query = query.Where(p => p.IsEnabled == true);
            }

            if (parameters.CheckCategorySendCountNotGreater != null)
            {
                int sendCountLimitValue = parameters.CheckCategorySendCountNotGreater.Value;
                query = query.Where(p => p.SendCount <= sendCountLimitValue);
            }

            return query;
        }

        protected virtual IQueryable<UserDeliveryTypeSettings<Guid>> JoinWithCategories(
            SubscribtionParameters parameters, IQueryable<UserDeliveryTypeSettings<Guid>> typeQueryPart
            , IQueryable<UserCategorySettings<Guid>> categoryQueryPart)
        {
            int categoryIDValue = parameters.CategoryID.Value;

            IQueryable<UserDeliveryTypeSettings<Guid>> query = null;

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
                        where catGroup != null
                        && (catGroup.LastSendDateUtc == null
                            || (d.LastUserVisitUtc != null && catGroup.LastSendDateUtc < d.LastUserVisitUtc))
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
        protected virtual IQueryable<UserTopicSettings<Guid>> CreateTopicQueryPart(
            SubscribtionParameters parameters, ClientDbContext context)
        {
            IQueryable<UserTopicSettings<Guid>> query = context.UserTopicSettings;

            query = query.Where(p => p.IsDeleted == false);

            if (parameters.CheckTopicEnabled)
            {
                query = query.Where(p => p.IsEnabled == true);
            }

            if (parameters.CheckTopicSendCountNotGreater != null)
            {
                int sendCountLimitValue = parameters.CheckTopicSendCountNotGreater.Value;
                query = query.Where(p => p.SendCount <= sendCountLimitValue);
            }

            return query;
        }

        protected virtual IQueryable<UserDeliveryTypeSettings<Guid>> JoinWithTopics(SubscribtionParameters parameters
            , IQueryable<UserDeliveryTypeSettings<Guid>> typeQueryPart, IQueryable<UserTopicSettings<Guid>> topicQueryPart)
        {
            int categoryIDValue = parameters.CategoryID.Value;
            string topicIDValue = parameters.TopicID;

            IQueryable<UserDeliveryTypeSettings<Guid>> query = null;

            if (parameters.CheckTopicLastSendDate)
            {
                query = from d in typeQueryPart
                        join t in topicQueryPart on
                        new
                        {
                            UserID = d.UserID,
                            CategoryID = categoryIDValue,
                            TopicID = topicIDValue
                        } equals new
                        {
                            UserID = t.UserID,
                            CategoryID = t.CategoryID,
                            TopicID = t.TopicID
                        }
                        where t.LastSendDateUtc == null
                        || (d.LastUserVisitUtc != null
                            && t.LastSendDateUtc < d.LastUserVisitUtc)
                        select d;
            }
            else
            {
                query = from d in typeQueryPart
                        join t in topicQueryPart on
                        new
                        {
                            UserID = d.UserID,
                            CategoryID = categoryIDValue,
                            TopicID = topicIDValue
                        } equals new
                        {
                            UserID = t.UserID,
                            CategoryID = t.CategoryID,
                            TopicID = t.TopicID
                        }
                        select d;
            }

            return query;
        }

    }
}
