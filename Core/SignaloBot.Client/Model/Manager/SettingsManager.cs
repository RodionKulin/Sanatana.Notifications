using SignaloBot.Client.DelayScheduler;
using SignaloBot.DAL;
using SignaloBot.DAL.Entities;
using SignaloBot.DAL.Entities.Core;
using SignaloBot.DAL.Enums;
using Common.Utility;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using SignaloBot.Client.Settings;

namespace SignaloBot.Client.Manager
{
    public class SettingsManager
    {
        //свойства
        public SignaloBotContext Context { get; set; }


        //инициализация
        public SettingsManager(SignaloBotContext context)
        {
            Context = context;
        }


        //методы
        public virtual List<UserReceivePeriod> SelectReceivePeriods(Guid userID, int deliveryType, int categoryID, out Exception exception)
        {
            int receiveDeliveryType = Context.FindReceivePeriodDeliveryType(deliveryType, categoryID);
            int receiveCategoryId = Context.FindReceivePeriodCategoryID(deliveryType, categoryID);

            return Context.Queries.UserReceivePeriods.SelectCategory(userID, receiveDeliveryType, receiveCategoryId, out exception);
        }

        public virtual void DeleteAllUserSettings(Guid userID, out Exception exception)
        {
            Context.Queries.UserDeliveryTypeSettings.DeleteAll(userID, out exception);

            if (exception == null)
            {
                Context.Queries.UserCategorySettings.DeleteAll(userID, out exception);
            }

            if (exception == null)
            {
                Context.Queries.UserTopicSettings.DeleteAll(userID, out exception);
            }

            if (exception == null)
            {
                Context.Queries.UserReceivePeriods.DeleteAll(userID, out exception);
            }
        }
    }
}
