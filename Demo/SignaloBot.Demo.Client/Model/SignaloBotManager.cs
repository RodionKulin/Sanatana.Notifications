using Common.EntityFramework;
using Common.Utility;
using SignaloBot.Client.Manager;
using SignaloBot.Client.Settings;
using SignaloBot.Client.Templates;
using SignaloBot.DAL.Entities.Core;
using SignaloBot.DAL.Entities.Parameters;
using SignaloBot.DAL.Entities.Results;
using SignaloBot.Demo.Client.Model.ViewModels;
using SignaloBot.TestParameters.Model;
using SignaloBot.TestParameters.Model.TestParameters;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;

namespace SignaloBot.Demo.Client.Model
{
    public class SignaloBotManager
    {
        private SignaloBotContext _signaloBotSettings;
        private SignalManager _signalManager;
        private enum DeliveryType { Email }
        private enum Category { Videos, Music, Games }


        //инициализация
        public SignaloBotManager()
        {
            ICommonLogger logger = new NLogger();
            string connection = SignaloBotTestParameters.ConnectionString;
            string sqlPrefix = SignaloBotTestParameters.SqlPrefix;
            _signaloBotSettings = new SignaloBotContext(logger, connection, sqlPrefix);

            _signaloBotSettings.CategorySettings.Add(new CategorySettings()
            {
                CategoryID = (int)Category.Games,
                DeliveryType = (int)DeliveryType.Email,
                ReceivePeriodCategoryID = (int)Category.Games,
                ReceivePeriodDeliveryType = (int)DeliveryType.Email,
                Template = new SignalTemplate()
                {
                    CategoryID = (int)Category.Games,
                    DeliveryType = (int)DeliveryType.Email,
                    TopicID = null,
                    SubjectProvider = new StringTemplate("Demo event in games category"),
                    SubjectTransformer = null,
                    BodyProvider = new StringTemplate("Games-Body.cshtml"),
                    BodyTransformer = new RazorTransformer(Path.Combine("App_Data", "Templates")),
                    IsBodyHtml = true,
                    SenderAddress = DemoParameters.EmailFromAddress,
                    SenderDisplayName = "SignaloBot DEMO client"
                }                
            });

            _signalManager = new SignalManager(_signaloBotSettings);
        }



        //методы
        public void SendGamesSignal(out Exception exception, out int subscribersCount)
        {
            int deliveryType = (int)DeliveryType.Email;
            int categoryID = (int)Category.Games;

            SubscriberParameters subscriberParameters = new SubscriberParameters()
            {
                DeliveryType =  deliveryType,
                CategoryID = categoryID,
                CheckTypeEnabled = true,
                CheckCategoryEnabled = true,
            };

            List<Subscriber> subscribers = _signaloBotSettings.Queries.Subscribers.Select(subscriberParameters, out exception);
            subscribersCount = subscribers.Count;
            if (exception != null)
            {
                return;
            }
                        
            List<TemplateData> bodyData = new List<TemplateData>();
            foreach (Subscriber subscriber in  subscribers)
	        {
                string unsubLink = string.Format("http://site.com/unsubscribe?u={0}", subscriber.UserID);

                Dictionary<string, string> replaceModel = new Dictionary<string, string>()
                {
                     { "Title", "Games title here"},
                     { "UserAddress", subscriber.Address },
                     { "UnsubscribeLink", unsubLink }
                };
                bodyData.Add(new TemplateData(replaceModel));
	        }

            SignalTemplate signalTemplate = _signalManager.Context.FindSignalTemplate(deliveryType, categoryID);
            List<Signal> messages = signalTemplate.Build(subscribers, bodyData);
            
            _signalManager.Context.Queries.Signals.Insert(messages, out exception);
        }

        public List<UserDeliveryTypeSettings> GetAllSubscribers(int page, out int total, out Exception exception)
        {
            int uiPageSize = 20;
            int sqlPageSize = uiPageSize * Enum.GetValues(typeof(DeliveryType)).Length;

            int numberStart;
            int numberEnd;
            SqlUtility.SqlRowNumber(page, sqlPageSize, out numberStart, out numberEnd);

            return _signaloBotSettings.Queries.UserDeliveryTypeSettings.SelectAllDeliveryTypes(
                numberStart, numberEnd, out total, out exception);
        }

        public bool CheckEmailExists(string address)
        {
            Exception exception;
            bool exists = _signaloBotSettings.Queries.UserDeliveryTypeSettings.CheckAddressExists(
                address, (int)DeliveryType.Email, out exception);

            return exception == null
                ? exists
                : false;
        }

        public bool AddUser(NewUserVM user)
        {
            Exception exception;

            //delivery type settings
            Guid userId = Guid.NewGuid();
            var settings = UserDeliveryTypeSettings.Default(userId, (int)DeliveryType.Email, user.Email);
            settings.IsEnabled = user.IsEmailEnabled;

            _signaloBotSettings.Queries.UserDeliveryTypeSettings.Insert(settings, out exception);


            //category settings
            if (exception == null)
            {
                UserCategorySettings catSettings = new UserCategorySettings()
                {
                    UserID = userId,
                    DeliveryType = (int)DeliveryType.Email,
                    CategoryID = (int)Category.Games,
                    IsEnabled = true
                };
                _signaloBotSettings.Queries.UserCategorySettings.Insert(catSettings, out exception);
            }

            return exception == null;
        }
    }
}