using Common.Utility;
using SignaloBot;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SignaloBot.TestParameters.Model.Stubs
{
    public class CommonLogStub : ICommonLogger
    {
        public List<string> Messages = new List<string>();
        public List<Exception> Exceptions = new List<Exception>();


        public void Debug(string message)
        {
            Messages.Add(message);
        }

        public void Debug(string message, params object[] parameters)
        {
            Messages.Add(message);
        }

        public void Info(string message)
        {
            Messages.Add(message);
        }

        public void Info(string message, params object[] parameters)
        {
            Messages.Add(message);
        }

        public void Error(string message)
        {
            Messages.Add(message);
        }

        public void Error(string message, params object[] parameters)
        {
            Messages.Add(message);
        }

        public void Exception(Exception exception)
        {
            Exceptions.Add(exception);
        }

        public void Exception(Exception exception, string message)
        {
            Messages.Add(message);
            Exceptions.Add(exception);
        }

        public void Exception(Exception exception, string message, params object[] parameters)
        {
            Messages.Add(message);
            Exceptions.Add(exception);
        }

        public void Dispose()
        {

        }
    }
}
