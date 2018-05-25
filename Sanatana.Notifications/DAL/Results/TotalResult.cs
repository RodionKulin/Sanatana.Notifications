using System;
using System.Collections.Generic;
using System.Text;

namespace Sanatana.Notifications.DAL.Results
{
    public class TotalResult<T>
    {
        public T Data { get; set; }
        public long Total { get; set; }


        //init
        public TotalResult()
        {

        }
        public TotalResult(T data, long total)
        {
            Data = data;
            Total = total;
        }
    }
}
