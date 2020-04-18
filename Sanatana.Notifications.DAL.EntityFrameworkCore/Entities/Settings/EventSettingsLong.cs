using Newtonsoft.Json;
using Sanatana.Notifications.EventsHandling.Templates;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Sanatana.Notifications.EventsHandling;
using Sanatana.Notifications.DAL;
using Sanatana.Notifications.DAL.Entities;

namespace Sanatana.Notifications.DAL.EntityFrameworkCore
{
    public class EventSettingsLong : EventSettings<long>
    {

        //navigation properties
        public virtual ICollection<DispatchTemplateLong> TemplatesNavigation
        {
            get
            {
                return (ICollection<DispatchTemplateLong>)Templates;
            }
            set
            {

            }
        }
    }

}
