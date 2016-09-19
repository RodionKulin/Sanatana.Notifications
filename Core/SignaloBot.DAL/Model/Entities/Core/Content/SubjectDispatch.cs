using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SignaloBot.DAL
{
    public class SubjectDispatch<TKey> : SignalDispatchBase<TKey>
        where TKey :struct
    {
                
        public virtual string SenderAddress { get; set; }
        public virtual string SenderDisplayName { get; set; }

        public virtual string MessageSubject { get; set; }
        public virtual string MessageBody { get; set; }
        public virtual bool IsBodyHtml { get; set; }
    }
}
