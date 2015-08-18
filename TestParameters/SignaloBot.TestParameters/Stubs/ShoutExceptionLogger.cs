using Common.Utility;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SignaloBot.TestParameters.Model.Stubs
{
    public class ShoutExceptionLogger : ICommonLogger
    {
        public void Debug(string message)
        {
            throw new NotImplementedException();
        }

        public void Debug(string message, params object[] parameters)
        {
            throw new NotImplementedException();
        }

        public void Info(string message)
        {
            throw new NotImplementedException();
        }

        public void Info(string message, params object[] parameters)
        {
            throw new NotImplementedException();
        }

        public void Error(string message)
        {
            throw new Exception(message);
        }

        public void Error(string message, params object[] parameters)
        {
            string fullMessage = string.Format(message, parameters);
            throw new Exception(fullMessage);
        }

        public void Exception(Exception exception)
        {
            throw exception;
        }

        public void Exception(Exception exception, string message)
        {
            throw exception;
        }

        public void Exception(Exception exception, string message, params object[] parameters)
        {
            throw exception;
        }

        public void Dispose()
        {

        }
    }
}
