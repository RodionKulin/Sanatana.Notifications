using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SignaloBot.DAL.Entities.Parameters
{
    internal class UpdateUserParameter
    {
        public Guid UserID { get; set; }
        public int SendCount { get; set; }
        public int DeliveryType { get; set; }
        public int CategoryID { get; set; }
        public int? TopicID { get; set; }
    }
}
