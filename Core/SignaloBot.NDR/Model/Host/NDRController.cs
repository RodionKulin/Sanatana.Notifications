using SignaloBot.NDR.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Http;

namespace SignaloBot.NDR.Host
{
    public abstract class NDRController: ApiController
    {
        //поля
        protected NDRHandler _handler;


        //инициализация
        public NDRController()
        {
            _handler = InitializeHandler();
        }
        protected abstract NDRHandler InitializeHandler();



        // GET NDR
        [HttpPost]
        public void Handle([FromBody]string value)
        {
            _handler.Handle(value);
        }
    }
}
