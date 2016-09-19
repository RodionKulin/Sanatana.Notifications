using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SignaloBot.DAL
{
    public class ComposerSettings<TKey>
        where TKey : struct
    {
        public virtual TKey ComposerSettingsID { get; set; }
        public virtual int CategoryID { get; set; }
        public virtual SubscribtionParameters Subscribtion { get; set; }
        public virtual UpdateParameters Updates { get; set; }
        public virtual List<SignalTemplateBase<TKey>> Templates { get; set; }
    }
}
