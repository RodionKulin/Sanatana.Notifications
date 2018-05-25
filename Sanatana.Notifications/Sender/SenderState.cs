using Sanatana.Timers.Switchables;
using Sanatana.Notifications.Monitoring;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sanatana.Notifications.Sender
{
    public class SenderState<TKey>
        where TKey : struct
    {
        //fields
        protected IMonitor<TKey> _eventSink;
        protected SwitchState _state = SwitchState.Stopped;


        //properties
        public virtual SwitchState State
        {
            get { return _state; }
            set
            {
                _eventSink.SenderSwitched(value);
                _state = value;
            }
        }


        //init
        public SenderState(IMonitor<TKey> eventSink)
        {
            _eventSink = eventSink;
        }
    }
}
