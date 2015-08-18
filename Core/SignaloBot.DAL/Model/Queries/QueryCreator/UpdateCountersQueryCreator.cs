using SignaloBot.DAL.Context;
using SignaloBot.DAL.Entities.Core;
using Common.EntityFramework.Merge;
using Common.EntityFramework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data.Entity;
using SignaloBot.DAL.Entities.Parameters;

namespace SignaloBot.DAL.Queries.QueryCreator
{
    public class UpdateCountersQueryCreator
    {
        //методы
        public string CreateQuery(UpdateParameters parameters, DbContext context
            , string prefix = null)
        {
            StringBuilder scriptBuilder = new StringBuilder();

            if (parameters.UpdateDeliveryType)
            {
                scriptBuilder.AppendLine(
                    CreateDeliveryTypeQuery(parameters, context, prefix));
                scriptBuilder.AppendLine();
            }

            if (parameters.UpdateCategory)
            {
                scriptBuilder.AppendLine(
                    CreateCategoryQuery(parameters, context, prefix));
                scriptBuilder.AppendLine();
            }

            if (parameters.UpdateTopic)
            {
                scriptBuilder.AppendLine(
                    CreateTopicQuery(parameters, context, prefix));
                scriptBuilder.AppendLine();
            }

            return scriptBuilder.ToString();
        }

        public string CreateDeliveryTypeQuery(UpdateParameters parameters, DbContext context
            , string prefix = null)
        {
            string tvpName = prefix + CoreTVP.UPDATE_USER_TYPE;
            var merge = new MergeOperation<UserDeliveryTypeSettings>(context, null, tvpName, CoreTVP.UPDATE_USERS_PARAMETER_NAME);
            
            merge.Compare.IncludeProperty(p => p.UserID)
                .IncludeProperty(p => p.DeliveryType);

            merge.Update.ExcludeAllPropertiesByDefault = true;

            if (parameters.UpdateDeliveryTypeSendCount)
            {
                merge.Update.Assign(t => t.SendCount, (t, s) => t.SendCount + s.SendCount);
            }

            if (parameters.UpdateDeliveryTypeLastSendDateUtc)
            {
                merge.Update.Assign(t => t.LastSendDateUtc, (t, s) => DateTime.UtcNow);
            }

            return merge.ConstructCommand(MergeType.Update);
        }

        public string CreateCategoryQuery(UpdateParameters parameters, DbContext context
            , string prefix = null)
        {
            string tvpName = prefix + CoreTVP.UPDATE_USER_TYPE;
            var merge = new MergeOperation<UserCategorySettings>(context, null, tvpName, CoreTVP.UPDATE_USERS_PARAMETER_NAME);

            merge.Compare.IncludeProperty(p => p.UserID)
                .IncludeProperty(p => p.DeliveryType)
                .IncludeProperty(p => p.CategoryID);

            merge.Update.ExcludeAllPropertiesByDefault = true;

            if (parameters.UpdateCategorySendCount)
            {
                merge.Update.Assign(t => t.SendCount, (t, s) => t.SendCount + s.SendCount);
            }

            if (parameters.UpdateCategoryLastSendDateUtc)
            {
                merge.Update.Assign(t => t.LastSendDateUtc, (t, s) => DateTime.UtcNow);
            }

            if (parameters.CreateCategoryIfNotExist)
            {
                merge.Insert.IncludeDefault(t => t.IsEnabled, true)
                    .IncludeDefault(t => t.LastSendDateUtc, DateTime.UtcNow);
            }

            MergeType mergeType = parameters.CreateCategoryIfNotExist
                ? MergeType.Upsert
                : MergeType.Update;
            return merge.ConstructCommand(mergeType);
        }

        public string CreateTopicQuery(UpdateParameters parameters, DbContext context
            , string prefix = null)
        {
            string tvpName = prefix + CoreTVP.UPDATE_USER_TYPE;
            var merge = new MergeOperation<UserTopicSettings>(context, null, tvpName, CoreTVP.UPDATE_USERS_PARAMETER_NAME);

            merge.Compare.IncludeProperty(p => p.UserID)
                .IncludeProperty(p => p.DeliveryType)
                .IncludeProperty(p => p.CategoryID)
                .IncludeProperty(p => p.TopicID);

            merge.Update.ExcludeAllPropertiesByDefault = true;

            if (parameters.UpdateTopicSendCount)
            {
                merge.Update.Assign(t => t.SendCount, (t, s) => t.SendCount + s.SendCount);
            }

            if (parameters.UpdateTopicLastSendDateUtc)
            {
                merge.Update.Assign(t => t.LastSendDateUtc, (t, s) => DateTime.UtcNow);
            }

            if (parameters.CreateTopicIfNotExist)
            {
                merge.Insert.IncludeDefault(t => t.AddDateUtc, DateTime.UtcNow)
                    .IncludeDefault(t => t.IsEnabled, true)
                    .IncludeDefault(t => t.LastSendDateUtc, DateTime.UtcNow)
                    .IncludeDefault(t => t.IsDeleted, false);
            }

            MergeType mergeType = parameters.CreateTopicIfNotExist
                ? MergeType.Upsert
                : MergeType.Update;
            return merge.ConstructCommand(mergeType);
        }
    }
}
