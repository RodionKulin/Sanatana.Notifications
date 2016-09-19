using System.Collections.Generic;
using SignaloBot.DAL;

namespace SignaloBot.DAL
{
    public abstract class SignalTemplateBase<TKey>
        where TKey : struct
    {
        public virtual int DeliveryType { get; set; }
        public virtual int? ReceivePeriodsGroupID { get; set; }
      

        public abstract List<SignalDispatchBase<TKey>> Build(
            List<Subscriber<TKey>> subscribers, Dictionary<string, string> data);
    }
}