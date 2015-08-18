using SignaloBot.NDR.Model;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SignaloBot.NDR.Model.Service
{
    public abstract class NDRHandlerService
    {
        //поля
        protected NDRHandler _ndrHandler;
        protected bool _logIncomingMessages;


        //инициализация
        public NDRHandlerService()
        {
            _ndrHandler = InitializeNDRHandler();
        }


        //методы
        public void HandleNDR(Stream stream)
        {
            if (_logIncomingMessages)
            {
                string requestMessage = _ndrHandler.ReadRequestStream(stream);

                if (_ndrHandler.Logger != null)
                {
                    _ndrHandler.Logger.Info(@"Получено сообщение:
{0}", requestMessage);
                }

                _ndrHandler.Handle(requestMessage);
            }
            else
            {
                _ndrHandler.Handle(stream);
            }
        }

        protected abstract NDRHandler InitializeNDRHandler();
    }
}
