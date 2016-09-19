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
    public class ExpressionComposer<TKey> : ComposerBase<TKey>
        where TKey : struct
    {
        //поля
        protected Func<SignalEventBase<TKey>, SignalTemplateBase<TKey>, List<Subscriber<TKey>>, List<SignalDispatchBase<TKey>>> _signalBuilder;


        //инициализация
        public ExpressionComposer(
            Func<SignalEventBase<TKey>, SignalTemplateBase<TKey>, List<Subscriber<TKey>>, List<SignalDispatchBase<TKey>>> signalBuilder)
            : base()
        {
            _signalBuilder = signalBuilder;
        }


        //методы
        protected override List<SignalDispatchBase<TKey>> BuildDispatches(
            SignalEventBase<TKey> signalEvent, SignalTemplateBase<TKey> template
            , List<Subscriber<TKey>> subscribers)
        {
            return _signalBuilder(signalEvent, template, subscribers);
        }
    }
}
