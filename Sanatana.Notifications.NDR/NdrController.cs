using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Sanatana.Notifications.NDR
{
    public class NdrController : Controller
    {
        private INdrHandler _ndrHandler;


        //init
        public NdrController(INdrHandler ndrHandler)
        {
            _ndrHandler = ndrHandler;
        }

        
        [HttpPost]
        public Task Handle([FromBody]string value)
        {
            return _ndrHandler.Handle(value);
        }
    }
}
