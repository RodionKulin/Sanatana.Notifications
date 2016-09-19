using SignaloBot.DAL;
using SignaloBot.Sender.Queue;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SignaloBot.Sender
{
    public class SignalWrapper<T>
    {
        //свойства
        public T Signal { get; set; }
        public bool IsStored { get; set; }
        public bool IsUpdated { get; set; }


        //инициализация
        public SignalWrapper(T signal, bool isStored)
        {
            Signal = signal;
            IsStored = isStored;
        }
        
    }
}
