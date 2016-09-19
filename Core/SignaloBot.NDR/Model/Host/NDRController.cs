using SignaloBot.NDR.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Http;

namespace SignaloBot.NDR.Host
{
    public abstract class NDRController<TKey> : ApiController
        where TKey : struct
    {
        //поля
        protected NDRHandler<TKey> _handler;


        //инициализация
        public NDRController()
        {
            _handler = InitializeHandler();
        }
        protected abstract NDRHandler<TKey> InitializeHandler();



        // GET NDR
        [HttpPost]
        public async Task Handle([FromBody]string value)
        {
            await _handler.Handle(value);
        }
    }
}
