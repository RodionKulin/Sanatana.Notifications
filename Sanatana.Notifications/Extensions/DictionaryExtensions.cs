using System;
using System.Collections.Generic;
using System.Text;

namespace Sanatana.Notifications.Extensions
{
    public static class DictionaryExtensions
    {
        /// <summary>
        /// Add all values from source dictionary to target dictionary. If key does not exist, create it. If key exists, overwrite it.
        /// </summary>
        /// <typeparam name="TKey"></typeparam>
        /// <typeparam name="TValue"></typeparam>
        /// <param name="target"></param>
        /// <param name="source"></param>
        public static void Merge<TKey, TValue>(this Dictionary<TKey, TValue> target, Dictionary<TKey, TValue> source)
        {
            if(source == null)
            {
                return;
            }

            foreach (KeyValuePair<TKey, TValue> item in source)
            {
                target[item.Key] = item.Value;
            }
        }
    }
}
