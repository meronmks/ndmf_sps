using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace com.meronmks.ndmfsps
{
    internal class MultiMap<TKey, TValue, TCollection> : IEnumerable<KeyValuePair<TKey, TValue>> where TCollection : ICollection<TValue>, new()
    {
        private readonly Dictionary<TKey, TCollection> data = new();
        
        public TCollection Get(TKey key) {
            return data.TryGetValue(key, out var list) ? list : new TCollection();
        }

        public void Put(TKey key, TValue value)
        {
            if (data.TryGetValue(key, out var list))
            {
                list.Add(value);
            }
            else
            {
                data[key] = new TCollection { value };
            }
        }
        
        public IEnumerator<KeyValuePair<TKey, TValue>> GetEnumerator()
        {
            foreach (var (dKey, dValue) in data)
            {
                foreach (var value in dValue)
                {
                    yield return new KeyValuePair<TKey, TValue>(dKey, value);
                }
            }
        }

        public IEnumerable<TKey> GetKeys() => data.Keys;
        public bool ContainsKey(TKey key) => data.ContainsKey(key);
        public bool ContainsValue(TValue value) => data.Values.Any(v => v.Contains(value));
        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }

    internal class MultiMapHashSet<TA, TB> : MultiMap<TA, TB, HashSet<TB>>
    {
        
    }
}