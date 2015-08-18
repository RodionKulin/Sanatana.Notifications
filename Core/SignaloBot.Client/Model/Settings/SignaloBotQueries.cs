using SignaloBot.DAL.Queries;
using SignaloBot.DAL.Queries.Client;
using Common.Utility;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SignaloBot.Client.Settings
{
    public class SignaloBotQueries
    {
        //свойства
        public ISignalQueries Signals { get; set; }
        public IUserCategorySettingsQueries UserCategorySettings { get; set; }
        public IUserDeliveryTypeSettingsQueries UserDeliveryTypeSettings { get; set; }
        public IUserReceivePeriodQueries UserReceivePeriods { get; set; }
        public IUserTopicSettingsQueries UserTopicSettings { get; set; }
        public ISubscriberQueries Subscribers { get; set; }



        //инициализация
        public SignaloBotQueries(ICommonLogger logger, string connectionStringOrName, string sqlPrefix = null)
        {
            Signals = new SignalQueries(connectionStringOrName, sqlPrefix, logger);
            Subscribers = new SubscriberQueries(connectionStringOrName, sqlPrefix, logger);
            UserCategorySettings = new UserCategorySettingsQueries(connectionStringOrName, sqlPrefix, logger);
            UserDeliveryTypeSettings = new UserDeliveryTypeSettingsQueries(connectionStringOrName, sqlPrefix, logger);
            UserReceivePeriods = new UserReceivePeriodQueries(connectionStringOrName, sqlPrefix, logger);
            UserTopicSettings = new UserTopicSettingsQueries(connectionStringOrName, sqlPrefix, logger);
        }
    }
}
