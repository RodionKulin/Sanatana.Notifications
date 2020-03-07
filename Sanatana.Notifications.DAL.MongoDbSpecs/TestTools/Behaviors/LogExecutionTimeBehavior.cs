using SpecsFor.Core;
using SpecsFor.Core.Configuration;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

namespace Sanatana.Notifications.DAL.MongoDbSpecs.TestTools.Behaviors
{
    public class LogExecutionTimeBehavior : Behavior<ISpecs>
    {
        private Stopwatch _stopwatch;
        private static Dictionary<Type, TimeSpan> _elapsedTime = new Dictionary<Type, TimeSpan>();


        public override void SpecInit(ISpecs instance)
        {
            _stopwatch = Stopwatch.StartNew();
        }

        public override void AfterSpec(ISpecs instance)
        {
            _stopwatch.Stop();
            Type type = instance.GetType();

            Debug.WriteLine($"{type.Name} - {_stopwatch.Elapsed}");
            _elapsedTime[type] = _stopwatch.Elapsed;
        }

        public static void PrintAll()
        {
            Debug.WriteLine($"All results");
            foreach (KeyValuePair<Type, TimeSpan> record in _elapsedTime)
            {
                Debug.WriteLine($"{record.Key.Name} - {record.Value}");
            }
        }
    }
}
