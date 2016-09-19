using Common.Utility;
using NLog;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
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



        //Debug
        public void Debug(string message)
        {
            _logger.Debug(message);
        }

        public void Debug(string message, params object[] parameters)
        {
            _logger.Debug(message, parameters);
        }


        //Info
        public void Info(string message)
        {
            _logger.Info(message);
        }

        public void Info(string message, params object[] parameters)
        {
            _logger.Info(message, parameters);
        }


        //Error
        public void Error(string message)
        {
            _logger.Error(message);
        }

        public void Error(string message, params object[] parameters)
        {
            message = string.Format(message, parameters);
            _logger.Error(message);
        }


        //Exception
        [MethodImpl(MethodImplOptions.NoInlining)]
        public void Exception(Exception exception)
        {
            StackFrame callingMethodFrame = new StackFrame(1);
            MethodBase callingMethod = callingMethodFrame.GetMethod();
            string message = string.Format("Exception in method: {0} of class: {1}."
                , callingMethod.Name, callingMethod.DeclaringType.FullName);

            _logger.Error(message, exception);
        }

        public void Exception(Exception exception, string message)
        {
            _logger.Error(message, exception);
        }

        public void Exception(Exception exception, string message, params object[] parameters)
        {
            message = string.Format(message, parameters);
            _logger.Error(message, exception);
        }
        


        //IDisposable
        public void Dispose()
        {
        }
    }
}