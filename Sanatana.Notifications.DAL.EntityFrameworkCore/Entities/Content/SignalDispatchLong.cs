using Newtonsoft.Json;
using Sanatana.Notifications.DAL;
using Sanatana.Notifications.DAL.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mail;
using System.Text;
using System.Threading.Tasks;

namespace Sanatana.Notifications.DAL.EntityFrameworkCore
{
    public class SignalDispatchLong : SignalDispatch<long>
    {
        //properties
        public string DerivedEntityData { get; set; }
    }
}
