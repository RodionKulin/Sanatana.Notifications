using Common.MongoDb;
using Common.Utility;
using MongoDB.Bson;
using MongoDB.Driver;
using SignaloBot.DAL;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SignaloBot.DAL.MongoDb
{
    public class MongoDbSignalBounceQueries : ISignalBounceQueries<ObjectId>
    {  
        //поля
        protected MongoDbConnectionSettings _settings;
        protected ICommonLogger _logger;
        protected SignaloBotMongoDbContext _context;


        //инициализация
        public MongoDbSignalBounceQueries(ICommonLogger logger, MongoDbConnectionSettings connectionSettings)
        {
            _logger = logger;
            _settings = connectionSettings;
            _context = new SignaloBotMongoDbContext(connectionSettings);
        }


        
        //методы
        public virtual async Task<bool> Insert(List<SignalBounce<ObjectId>> messages)
        {
            bool result = false;

            var options = new InsertManyOptions()
            {
                IsOrdered = false
            };

            try
            {
                await _context.SignalBounces.InsertManyAsync(messages, options);
                result = true;
            }
            catch (Exception ex)
            {
                _logger.Exception(ex);
            }

            return result;
        }
    }
}
