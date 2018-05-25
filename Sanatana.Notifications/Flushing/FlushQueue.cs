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
        //properties
        public Task FlushTask { get; set; }
        public int ItemsTaken { get; set; }
        public BlockingCollection<SignalWrapper<T>> Queue { get; set; }


        //init
        public FlushQueue()
        {
            Queue = new BlockingCollection<SignalWrapper<T>>();
        }


        //methods
        public virtual void Flush(Func<List<T>, Task> flushQuery)
        {
            List<T> list = Queue.Select(p => p.Signal).ToList();
            ItemsTaken = list.Count;

            if (list.Count > 0)
            {
                FlushTask = flushQuery(list);
            }
            else
            {
                FlushTask = Task.FromResult(0);
            }
        }

        public virtual IEnumerable<SignalWrapper<T>> Clear()
        {
            if (FlushTask != null && ItemsTaken > 0)
            {
                for (int i = 0; i < ItemsTaken; i++)
                {
                    SignalWrapper<T> item = Queue.Take();
                    yield return item;
                }
            }
        }
    }
}
