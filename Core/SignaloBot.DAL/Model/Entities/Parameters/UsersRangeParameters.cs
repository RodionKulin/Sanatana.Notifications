using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SignaloBot.DAL
{ 
    public class UsersRangeParameters<TKey>
        where TKey : struct
    {
        public virtual List<TKey> FromUserIDs { get; set; }
        public virtual TKey? UserIDRangeFromExcludingSelf { get; set; }
        public virtual TKey? UserIDRangeToIncludingSelf { get; set; }
        public virtual int? Limit { get; set; }
        public virtual TKey? GroupID { get; set; }
    }
}
