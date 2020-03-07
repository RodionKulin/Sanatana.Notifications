using System;
using System.Collections.Generic;
using System.Text;

namespace Sanatana.Notifications.DAL.MongoDbSpecs.TestTools
{
    public static class NumbersExtensions
    {
        /// <summary>
        /// Check is number is between start and end inclusive both
        /// </summary>
        /// <param name="target"></param>
        /// <param name="start"></param>
        /// <param name="end"></param>
        /// <returns></returns>
        public static bool IsInRange(this long target, int start, int end)
        {
            return start <= target && target <= end;
        }

        /// <summary>
        /// Check is number outside of range exclusive both
        /// </summary>
        /// <param name="target"></param>
        /// <param name="start"></param>
        /// <param name="end"></param>
        /// <returns></returns>
        public static bool IsOutsideRange(this long target, int start, int end)
        {
            return target < start && end < target;
        }
    }
}
