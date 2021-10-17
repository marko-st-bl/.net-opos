using System;
using System.Collections.Generic;
using System.Text;

namespace Zadatak4.BTreeDictionary
{
    [Serializable]
    public class BTreeNode<TKey, TValue>
    {
        private readonly int _degree;
        public List<BTreeNode<TKey, TValue>> Children {get; set;}
        public List<BTreeEntry<TKey, TValue>> Entries { get; set; }

        public BTreeNode(int degree)
        {
            this._degree = degree;
            this.Children = new List<BTreeNode<TKey, TValue>>(degree);
            this.Entries = new List<BTreeEntry<TKey, TValue>>(degree);
        }

        public bool IsLeaf => Children.Count == 0;
        public bool HasReachedMaxEntries => Entries.Count == (2 * _degree) - 1;
        public bool HasReachedMinEntries => Entries.Count == _degree - 1;
    }
}
