using SignaloBot.NDR.Model;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SignaloBot.NDR.Model.Service
{
    public abstract class NDRHandlerService<TKey>
        where TKey : struct
    {
        //поля
        protected NDRHandler<TKey> _ndrHandler;
        protected bool _logIncomingMessages;


        //инициализация
        public NDRHandlerService()
        {
            _ndrHandler = InitializeNDRHandler();
        }
        protected abstract NDRHandler<TKey> InitializeNDRHandler();



        //методы
        public Task<bool> HandleNDR(Stream stream)
        {
            if (_logIncomingMessages)
            {
                string requestMessage = _ndrHandler.ReadRequestStream(stream);

                if (_ndrHandler.Logger != null)
                {
                    _ndrHandler.Logger.Info(@"Получено сообщение:
{0}", requestMessage);
                }

                return _ndrHandler.Handle(requestMessage);
            }
            else
            {
                return _ndrHandler.Handle(stream);
            }
        }

    }
}
