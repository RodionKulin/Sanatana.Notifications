using Sanatana.Notifications.DAL.EntityFrameworkCore;
using Sanatana.Notifications.DAL;
using Sanatana.Notifications.DispatchHandling.DeliveryTypes.Email;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sanatana.Notifications.Demo.Sender.Model
{
    public class WorkOrderEmailDispatch : EmailDispatch<long>
    {
        public int WorkOrderId { get; set; }
    }
}
