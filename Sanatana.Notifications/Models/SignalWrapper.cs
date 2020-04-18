using Sanatana.Notifications.DAL;
using Sanatana.Notifications.Queues;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sanatana.Notifications.Models
{
    public class SignalWrapper<T>
    {
        //properties
        /// <summary>
        /// Dispatch or Event entity
        /// </summary>
        public T Signal { get; set; }
        /// <summary>
        /// Is stored in database already. Different actions occure on returning to database. Insert or Update existing for example.
        /// </summary>
        public bool IsPermanentlyStored { get; set; }
        /// <summary>
        /// Were any changes made to Signal during processing
        /// </summary>
        public bool IsUpdated { get; set; }
        /// <summary>
        /// If existing items were consolidated into single one and can be deleted from database.
        /// </summary>
        public bool IsConsolidationCompleted { get; set; }
        /// <summary>
        /// Identifier used as a name to store in temporary file storage before storing in database
        /// </summary>
        public Guid? TempStorageId { get; set; }
        /// <summary>
        /// Signals should be handled together and consolidated. 
        /// Additional items to consolidate with may still be present in database.
        /// </summary>
        public SignalWrapper<T>[] ConsolidatedSignals { get; set; }


        //init
        public SignalWrapper(T signal, bool isPermanentlyStored)
        {
            Signal = signal;
            IsPermanentlyStored = isPermanentlyStored;
        }
    }

    public static class SignalWrapper
    {
        public static SignalWrapper<TSignal> Create<TSignal>(TSignal signal, bool isPermanentlyStored)
        {
            return new SignalWrapper<TSignal>(signal, isPermanentlyStored);
        }
    }
}
