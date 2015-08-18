using Common.Utility;
using NLog;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Web;

namespace SignaloBot.Demo.Sender
{
    public class NLogger : ICommonLogger
    {
         //поля
        private Logger _logger;



        //инициализация
        public NLogger()
        {
            _logger = LogManager.GetCurrentClassLogger();
        }



        //методы
        public void Debug(string message)
        {
            _logger.Debug(message);
        }

        public void Debug(string message, params object[] parameters)
        {
            _logger.Debug(message, parameters);
        }

        public void Info(string message)
        {
            _logger.Info(message);
        }

        public void Info(string message, params object[] parameters)
        {
            _logger.Info(message, parameters);
        }

        public void Error(string message)
        {
            Exception(new Exception(message));
        }

        public void Error(string message, params object[] parameters)
        {
            message = string.Format(message, parameters);
            Error(message);
        }

        public void Exception(Exception exception)
        {
            StackFrame callingMethodFrame = new StackFrame(1);
            MethodBase callingMethod = callingMethodFrame.GetMethod();
            string message = string.Format("Ошибка в методе {0} класса {1}.", callingMethod.Name, callingMethod.DeclaringType.FullName);

            Exception(exception, message);
        }

        public void Exception(Exception exception, string message)
        {
            _logger.Error(message, exception);
        }

        public void Exception(Exception exception, string message, params object[] parameters)
        {
            message = string.Format(message, parameters);
            Exception(exception, message);
        }

        //IDisposable
        public void Dispose()
        {
        }
    }
}