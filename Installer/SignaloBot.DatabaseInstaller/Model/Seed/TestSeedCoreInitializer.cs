using SignaloBot.DAL.Context;
using SignaloBot.DAL.Entities;
using SignaloBot.DAL.Entities.Core;
using SignaloBot.TestParameters.Model;
using Common.EntityFramework;
using Common.Utility;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SignaloBot.DatabaseInstaller.Seed
{
    public class TestSeedCoreInitializer : CreateDatabaseIfNotExists<ClientDbContext>
    {
        protected override void Seed(ClientDbContext context)
        {
            base.Seed(context);

            InsertContent(context);
        }

        public void InsertContent(ClientDbContext context)
        {
            //UserSettings
            UserDeliveryTypeSettings settings = UserDeliveryTypeSettings.Default(SignaloBotTestParameters.ExistingUserID
                , SignaloBotTestParameters.ExistingDeliveryType, "mail@test.ml");
            settings.LastUserVisitUtc = DateTime.UtcNow.AddSeconds(5);
            context.UserDeliveryTypeSettings.Add(settings);

            settings = settings = UserDeliveryTypeSettings.Default(Guid.NewGuid()
                , SignaloBotTestParameters.ExistingDeliveryType, "mail2@test.ml");
            context.UserDeliveryTypeSettings.Add(settings);

            settings = settings = UserDeliveryTypeSettings.Default(Guid.NewGuid()
                , SignaloBotTestParameters.ExistingDeliveryType, "mail3@test.ml");
            context.UserDeliveryTypeSettings.Add(settings);


            //UserCategorySettings
            UserCategorySettings categorySettings = SignaloBotEntityCreator.CreateUserCategorySettings(1);
            categorySettings.LastSendDateUtc = categorySettings.LastSendDateUtc.Value.AddDays(-1);
            context.UserCategorySettings.Add(categorySettings);

            categorySettings = SignaloBotEntityCreator.CreateUserCategorySettings(2);
            categorySettings.LastSendDateUtc = categorySettings.LastSendDateUtc.Value.AddDays(-2);
            context.UserCategorySettings.Add(categorySettings);

            categorySettings = SignaloBotEntityCreator.CreateUserCategorySettings(3);
            categorySettings.LastSendDateUtc = categorySettings.LastSendDateUtc.Value.AddDays(-3);
            context.UserCategorySettings.Add(categorySettings);


            //UserTopicSettings
            UserTopicSettings subscription = SignaloBotEntityCreator.CreateUserTopicSettings(categoryID: 2, topicID: 1);
            subscription.AddDateUtc = subscription.AddDateUtc.AddDays(-1);
            context.UserTopicSettings.Add(subscription);

            subscription = SignaloBotEntityCreator.CreateUserTopicSettings(categoryID: 2, topicID: 2);
            subscription.AddDateUtc = subscription.AddDateUtc.AddDays(-2);
            context.UserTopicSettings.Add(subscription);

            subscription = SignaloBotEntityCreator.CreateUserTopicSettings(categoryID: 2, topicID: 3);
            subscription.AddDateUtc = subscription.AddDateUtc.AddDays(-3);
            context.UserTopicSettings.Add(subscription);

            //Save
            context.SaveChanges();
        }
    }
}
