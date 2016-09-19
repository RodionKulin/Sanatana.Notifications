using Common.MongoDb;
using Common.Utility;
using MongoDB.Bson;
using SignaloBot.DAL;
using SignaloBot.DAL.MongoDb;
using SignaloBot.Demo.Client.SignaloBot.Service;
using SignaloBot.TestParameters.Model;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.ServiceModel;
using System.Threading.Tasks;
using System.Web;

namespace SignaloBot.Demo.Client.Model
{
    public class SignaloBotManager
    {
        //поля
        private enum DeliveryType { Console }
        private enum Category { Videos, Music, Games }
        private string _language = "ru-RU";

        private IUserDeliveryTypeSettingsQueries<ObjectId> _deliveryTypeQueries;
        private IUserCategorySettingsQueries<ObjectId> _categoryQueries;
        private ISignalEventQueries<ObjectId> _signalEventQueries;
        private static WcfSafeCall<ISignalServiceOf_ObjectId> _serviceConnection;


        //инициализация
        static SignaloBotManager()
        {
            _serviceConnection = new WcfSafeCall<ISignalServiceOf_ObjectId>(
                "NetNamedPipeBinding_ISignalServiceOf_ObjectId", true);
        }
        public SignaloBotManager()
        {
            ICommonLogger logger = new NLogger();
            MongoDbConnectionSettings connection = SignaloBotTestParameters.MongoDbConnection;
            _deliveryTypeQueries = new MongoDbUserDeliveryTypeSettingsQueries(logger, connection);
            _categoryQueries = new MongoDbUserCategorySettingsQueries(logger, connection);
            _signalEventQueries = new MongoDbSignalEventQueries(logger, connection);
        }



        //методы
        public bool EnqueueSignal()
        {
            ObjectId groupID = ObjectId.GenerateNewId();

            Exception exception;
            _serviceConnection.SafeCall(() =>
            {
                _serviceConnection.Client.RaiseKeyValueEvent(groupID, null, (int)Category.Games, null, new Dictionary<string, string>()
                {
                    { "Title", "Title goes here"},
                });
            }, out exception);
            
            return exception == null;
        }

        public Task<TotalResult<List<UserDeliveryTypeSettings<ObjectId>>>> GetAllSubscribers(int page)
        {
            int pageSize = 20;
            List<int> deliveryTypes = new List<int>() { (int)DeliveryType.Console };
            return _deliveryTypeQueries.SelectPage(deliveryTypes, page, pageSize);
        }

        public async Task<QueryResult<bool>> CheckEmailExists(string address)
        {
            QueryResult<bool> exists = await _deliveryTypeQueries.CheckAddressExists(
                (int)DeliveryType.Console, address);
            return exists;
        }

        public async Task<bool> AddUser(NewUserVM user)
        {            
            ObjectId userId = ObjectId.GenerateNewId();
            var settings = UserDeliveryTypeSettings<ObjectId>.Default(
                userId, (int)DeliveryType.Console, user.Email, _language);
            settings.IsEnabled = user.IsEmailEnabled;
            
            bool deliveryResult = await _deliveryTypeQueries.Insert(
                new List<UserDeliveryTypeSettings<ObjectId>>() { settings });
            if(!deliveryResult)
            {
                return false;
            }

            var catSettings = new UserCategorySettings<ObjectId>()
            {
                UserID = userId,
                DeliveryType = (int)DeliveryType.Console,
                CategoryID = (int)Category.Games,
                IsEnabled = true
            };
            bool categoryResult = await _categoryQueries.Insert(new List<UserCategorySettings<ObjectId>>() { catSettings });

            return categoryResult;
        }
        
    }
}