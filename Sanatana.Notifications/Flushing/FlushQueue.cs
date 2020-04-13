using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace Sanatana.Notifications.Flushing
{
    public class FlushQueue<T>
    {
        //fields
        protected Func<List<T>, Task> _flushQuery;


        //properties
        public int ItemsTaken { get; set; }
        public BlockingCollection<T> Queue { get; set; }


        //init
        public FlushQueue(Func<List<T>, Task> flushQuery)
        {
            Queue = new BlockingCollection<T>();
            _flushQuery = flushQuery;
        }


        //methods
        public virtual Task Flush()
        {
            if (Queue.Count == 0)
            {
                return Task.CompletedTask;
            }

            List<T> list = Queue.ToList();
            ItemsTaken = list.Count;
            return _flushQuery(list);
        }

        public virtual IEnumerable<T> Clear()
        {
            for (int i = 0; i < ItemsTaken; i++)
            {
                T item = Queue.Take();
                yield return item;
            }
            ItemsTaken = 0;
        }
    }
}
