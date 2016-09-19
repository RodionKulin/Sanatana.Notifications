using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SignaloBot.DAL.SQL
{
    internal class UpdateUserParameter
    {
        public virtual Guid UserID { get; set; }
        public virtual int SendCount { get; set; }
        public virtual int DeliveryType { get; set; }
        public virtual int CategoryID { get; set; }
        public virtual string TopicID { get; set; }
    }
}
