using Sanatana.Notifications.Composing.Templates;
using Sanatana.Notifications.DAL;
using Sanatana.Notifications.DAL.Parameters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sanatana.Notifications.DAL.Entities
{
    public class ComposerSettings<TKey>
        where TKey : struct
    {
        public TKey ComposerSettingsId { get; set; }
        public string Name { get; set; }
        public int CategoryId { get; set; }
        public int? CompositionHandlerId { get; set; }
        public SubscriptionParameters Subscription { get; set; }
        public UpdateParameters Updates { get; set; }
        public List<DispatchTemplate<TKey>> Templates { get; set; }
    }
}
