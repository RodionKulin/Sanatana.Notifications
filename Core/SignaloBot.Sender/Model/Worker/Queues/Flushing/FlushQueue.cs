using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace SignaloBot.Sender.Queue
{
    public class FlushQueue<T>
    {
        //свойства
        public Task<bool> FlushTask { get; set; }
        public int Taken { get; set; }
        public BlockingCollection<SignalWrapper<T>> Queue { get; set; }


        //инициализация
        public FlushQueue()
        {
            Queue = new BlockingCollection<SignalWrapper<T>>();
        }


        //методы
        public void Flush(Func<List<T>, Task<bool>> flushQuery)
        {
            List<T> list = Queue.Select(p => p.Signal).ToList();
            Taken = list.Count;

            if (list.Count > 0)
            {
                FlushTask = flushQuery(list);
            }
            else
            {
                FlushTask = Task.FromResult(false);
            }
        }

        public void Remove()
        {
            if (FlushTask != null && FlushTask.Result && Taken > 0)
            {
                for (int i = 0; i < Taken; i++)
                {
                    Queue.Take();
                }
            }
        }
    }
}
