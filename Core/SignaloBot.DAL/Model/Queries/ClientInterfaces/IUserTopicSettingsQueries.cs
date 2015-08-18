using System;
namespace SignaloBot.DAL.Queries.Client
{
    public interface IUserTopicSettingsQueries
    {
        void DeleteAll(Guid userID, out Exception exception);
        SignaloBot.DAL.Entities.Core.UserTopicSettings Find(Guid userID, int deliveryType, int categoryID, int topicID, out Exception exception);
        void Upsert(Guid userID, int deliveryType, int categoryID, int topicID, bool forceIfIsDeleted, bool? isEnabledOnNewTopic, out Exception exception);
        void MarkDeleted(Guid userID, int deliveryType, int categoryID, int topicID, out Exception exception);
        System.Collections.Generic.List<SignaloBot.DAL.Entities.Core.UserTopicSettings> SelectPage(Guid userID, int deliveryType, System.Collections.Generic.List<int> categoryIDs, int start, int end, out int total, out Exception exception);
        void Update(SignaloBot.DAL.Entities.Core.UserTopicSettings settings, out Exception exception);
        void UpsertIsEnabled(SignaloBot.DAL.Entities.Core.UserTopicSettings settings, out Exception exception);
    }
}
