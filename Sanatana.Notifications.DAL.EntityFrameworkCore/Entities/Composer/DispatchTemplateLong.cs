using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Sanatana.Notifications.Composing.Templates;
using Newtonsoft.Json;
using Sanatana.Notifications.DAL;
using Sanatana.Notifications.Composing;
using Sanatana.Notifications.DAL.Entities;
using Sanatana.Notifications.DAL.Results;

namespace Sanatana.Notifications.DAL.EntityFrameworkCore
{
    public class DispatchTemplateLong : DispatchTemplate<long>
    {
        //properties
        public string DerivedEntityData { get; set; }


        //navigation properties
        public virtual ComposerSettingsLong ComposerSettingsNavigation { get; set; }


        //methods
        public override List<SignalDispatch<long>> Build(ComposerSettings<long> settings, SignalEvent<long> signalEvent
            , List<Subscriber<long>> subscribers)
        {
            throw new NotImplementedException();
        }
    }
}
