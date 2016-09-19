using SignaloBot.Sender;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SignaloBot.Sender.Composers.Templates;
using SignaloBot.DAL;
using System.Linq.Expressions;

namespace SignaloBot.Sender.Composers
{
    public class KeyValueComposer<TKey> : ComposerBase<TKey>
        where TKey : struct
    {
        //поля

        //инициализация
        public KeyValueComposer()
            : base()
        {
        }


        //методы
        protected override List<SignalDispatchBase<TKey>> BuildDispatches(
            SignalEventBase<TKey> signalEvent, SignalTemplateBase<TKey> template
            , List<Subscriber<TKey>> subscribers)
        {
            KeyValueEvent<TKey> keyValueEvent = (KeyValueEvent<TKey>)signalEvent;
            return template.Build(subscribers, keyValueEvent.Values);
        }
    }
}
