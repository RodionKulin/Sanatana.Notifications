using Common.EntityFramework.SafeCall;
using Common.Utility;
using SignaloBot.DAL.Context;
using SignaloBot.DAL.Entities.Core;
using SignaloBot.TestParameters.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace SignaloBot.Demo.Client.Model
{
    public class SignaloBotQueries
    { 
        //поля
        protected ICommonLogger _logger;
        protected EntityCRUD<SenderDbContext, Signal> _signalCrud;


        //инициализация
        public SignaloBotQueries()
        {
            _logger = new NLogger();
            _signalCrud = new EntityCRUD<SenderDbContext, Signal>(
                SignaloBotTestParameters.ConnectionString, SignaloBotTestParameters.SqlPrefix);
        }
        

        //методы
        public List<Signal> SelectSignals()
        {
            Exception exception;
            List<Signal> list = _signalCrud.SelectAll(out exception);
            
            if (_logger != null && exception != null)
            {
                _logger.Exception(exception);
            }

            if (list == null)
                list = new List<Signal>();

            return list;
        }
    }
}