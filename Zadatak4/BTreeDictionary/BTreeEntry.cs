using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Text;

namespace Zadatak4.BTreeDictionary
{
    [Serializable]
    public class BTreeEntry<TKey, TValue> : IEquatable<BTreeEntry<TKey, TValue>>//, IComparable<BTreeEntry<TKey, TValue>>
    {
        public TKey Key { get; set; }
        public TValue Value { get; set; }

        public bool Equals([AllowNull] BTreeEntry<TKey, TValue> other)
        {
            return other.Key.Equals(Key);
        }
    }
}
