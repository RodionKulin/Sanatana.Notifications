using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace SignaloBot.DAL
{
    public class UserReceivePeriod<TKey>
        where TKey : struct
    {
        public TKey UserID { get; set; }
        public int ReceivePeriodsGroupID { get; set; }
        public int PeriodOrder { get; set; }

        public TimeSpan PeriodBegin { get; set; }
        public TimeSpan PeriodEnd { get; set; }

        
        //зависимые свойства
        public string PeriodBeginString
        {
            get
            {
                return string.Format("{0:00}:{1:00}", PeriodBegin.Hours, PeriodBegin.Minutes);
            }
        }
        public string PeriodEndString
        {
            get
            {
                return string.Format("{0:00}:{1:00}", PeriodEnd.Hours, PeriodEnd.Minutes);
            }
        }

    }
}