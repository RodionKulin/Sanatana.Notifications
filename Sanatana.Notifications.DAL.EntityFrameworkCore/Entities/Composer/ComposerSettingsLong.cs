using Newtonsoft.Json;
using Sanatana.Notifications.Composing.Templates;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Sanatana.Notifications.Composing;
using Sanatana.Notifications.DAL;
using Sanatana.Notifications.DAL.Entities;

namespace Sanatana.Notifications.DAL.EntityFrameworkCore
{
    public class ComposerSettingsLong : ComposerSettings<long>
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
