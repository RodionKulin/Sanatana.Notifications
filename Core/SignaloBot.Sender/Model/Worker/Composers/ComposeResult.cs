using SignaloBot.DAL;
using SignaloBot.Sender.Processors;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SignaloBot.Sender.Composers
{
    public class ComposeResult<T>
    {
        //свойства
        public bool IsFinished { get; set; }
        public ProcessingResult Result { get; set; }
        public List<T> Items { get; set; }


        //инициализация
        public static ComposeResult<T> FromResult(ProcessingResult result)
        {
            return new ComposeResult<T>()
            {
                Result = result
            };
        }
    }
}
