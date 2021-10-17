using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;

namespace Zadatak4.BTreeDictionary
{
    [Serializable]
    class BTreeDictionary<TKey, TValue> : IDictionary<TKey, TValue> where TKey : IComparable<TKey>
    {
        public int Degree { get; private set; }
        public int Height { get; private set; }

        public BTreeNode<TKey, TValue> Root { get; set; }

        public BTreeDictionary(int degree)
        {
            this.Degree = degree;
            this.Root = new BTreeNode<TKey, TValue>(degree);
            this.Height = 1;
        }

        public TValue this[TKey key] { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

        public ICollection<TKey> Keys => throw new NotImplementedException();

        public ICollection<TValue> Values => throw new NotImplementedException();

        public int Count => throw new NotImplementedException();

        public bool IsReadOnly => false;

        public void Add(TKey key, TValue value)
        {
            if (!Root.HasReachedMaxEntries)
            {
                InsertNonFull(Root, key, value);
            }
            else
            {
                BTreeNode<TKey, TValue> oldRoot = Root;
                Root = new BTreeNode<TKey, TValue>(Degree);
                Root.Children.Add(oldRoot);
                SplitNode(Root, 0, oldRoot);
                InsertNonFull(Root, key, value);
                Height++;
            }
        }

        public void Add(KeyValuePair<TKey, TValue> item)
        {
            throw new NotImplementedException();
        }

        public void Clear()
        {
            throw new NotImplementedException();
        }

        public bool Contains(KeyValuePair<TKey, TValue> item)
        {
            return SearchInternal(Root, item.Key) != null ? true : false;
        }

        public bool ContainsKey(TKey key)
        {
            return SearchInternal(Root, key) != null ? true : false;
        }

        public void CopyTo(KeyValuePair<TKey, TValue>[] array, int arrayIndex)
        {
            throw new NotImplementedException();
        }

        public IEnumerator<KeyValuePair<TKey, TValue>> GetEnumerator()
        {
            throw new NotImplementedException();
        }

        public bool Remove(TKey key)
        {
            this.DeleteInternal(this.Root, key);

            // if root's last entry was moved to a child node, remove it
            if (this.Root.Entries.Count == 0 && !this.Root.IsLeaf)
            {
                this.Root = this.Root.Children.Single();
                this.Height--;
            }
            return true;
        }

        private void DeleteInternal(BTreeNode<TKey, TValue> node, TKey key)
        {
            int i = node.Entries.TakeWhile(entry => key.CompareTo(entry.Key) > 0).Count();

            // found key in node, so delete if from it
            if (i < node.Entries.Count && node.Entries[i].Key.CompareTo(key) == 0)
            {
                this.DeleteKeyFromNode(node, key, i);
                return;
            }

            // delete key from subtree
            if (!node.IsLeaf)
            {
                this.DeleteKeyFromSubtree(node, key, i);
            }
        }

        private void DeleteKeyFromSubtree(BTreeNode<TKey, TValue> parentNode, TKey key, int i)
        {
            BTreeNode<TKey, TValue> childNode = parentNode.Children[i];

            // node has reached min # of entries, and removing any from it will break the btree property,
            // so this block makes sure that the "child" has at least "degree" # of nodes by moving an 
            // entry from a sibling node or merging nodes
            if (childNode.HasReachedMinEntries)
            {
                int leftIndex = i - 1;
                BTreeNode<TKey, TValue> leftSibling = i > 0 ? parentNode.Children[leftIndex] : null;

                int rightIndex = i + 1;
                BTreeNode<TKey, TValue> rightSibling = i < parentNode.Children.Count - 1
                                                ? parentNode.Children[rightIndex]
                                                : null;

                if (leftSibling != null && leftSibling.Entries.Count > this.Degree - 1)
                {
                    // left sibling has a node to spare, so this moves one node from left sibling 
                    // into parent's node and one node from parent into this current node ("child")
                    childNode.Entries.Insert(0, parentNode.Entries[i]);
                    parentNode.Entries[i] = leftSibling.Entries.Last();
                    leftSibling.Entries.RemoveAt(leftSibling.Entries.Count - 1);

                    if (!leftSibling.IsLeaf)
                    {
                        childNode.Children.Insert(0, leftSibling.Children.Last());
                        leftSibling.Children.RemoveAt(leftSibling.Children.Count - 1);
                    }
                }
                else if (rightSibling != null && rightSibling.Entries.Count > this.Degree - 1)
                {
                    // right sibling has a node to spare, so this moves one node from right sibling 
                    // into parent's node and one node from parent into this current node ("child")
                    childNode.Entries.Add(parentNode.Entries[i]);
                    parentNode.Entries[i] = rightSibling.Entries.First();
                    rightSibling.Entries.RemoveAt(0);

                    if (!rightSibling.IsLeaf)
                    {
                        childNode.Children.Add(rightSibling.Children.First());
                        rightSibling.Children.RemoveAt(0);
                    }
                }
                else
                {
                    // this block merges either left or right sibling into the current node "child"
                    if (leftSibling != null)
                    {
                        childNode.Entries.Insert(0, parentNode.Entries[i]);
                        var oldEntries = childNode.Entries;
                        childNode.Entries = leftSibling.Entries;
                        childNode.Entries.AddRange(oldEntries);
                        if (!leftSibling.IsLeaf)
                        {
                            var oldChildren = childNode.Children;
                            childNode.Children = leftSibling.Children;
                            childNode.Children.AddRange(oldChildren);
                        }

                        parentNode.Children.RemoveAt(leftIndex);
                        parentNode.Entries.RemoveAt(i);
                    }
                    else
                    {
                        //Debug.Assert(rightSibling != null, "Node should have at least one sibling");
                        childNode.Entries.Add(parentNode.Entries[i]);
                        childNode.Entries.AddRange(rightSibling.Entries);
                        if (!rightSibling.IsLeaf)
                        {
                            childNode.Children.AddRange(rightSibling.Children);
                        }

                        parentNode.Children.RemoveAt(rightIndex);
                        parentNode.Entries.RemoveAt(i);
                    }
                }
            }

            // at this point, we know that "child" has at least "degree" nodes, so we can
            // move on - this guarantees that if any node needs to be removed from it to
            // guarantee BTree's property, we will be fine with that
            this.DeleteInternal(childNode, key);
        }

        private void DeleteKeyFromNode(BTreeNode<TKey, TValue> node, TKey key, int i)
        {
            // if leaf, just remove it from the list of entries (we're guaranteed to have
            // at least "degree" # of entries, to BTree property is maintained
            if (node.IsLeaf)
            {
                node.Entries.RemoveAt(i);
                return;
            }

            BTreeNode<TKey, TValue> predecessorChild = node.Children[i];
            if (predecessorChild.Entries.Count >= this.Degree)
            {
                BTreeEntry<TKey, TValue> predecessor = this.DeletePredecessor(predecessorChild);
                node.Entries[i] = predecessor;
            }
            else
            {
                BTreeNode<TKey, TValue> successorChild = node.Children[i + 1];
                if (successorChild.Entries.Count >= this.Degree)
                {
                    BTreeEntry<TKey, TValue> successor = this.DeleteSuccessor(predecessorChild);
                    node.Entries[i] = successor;
                }
                else
                {
                    predecessorChild.Entries.Add(node.Entries[i]);
                    predecessorChild.Entries.AddRange(successorChild.Entries);
                    predecessorChild.Children.AddRange(successorChild.Children);

                    node.Entries.RemoveAt(i);
                    node.Children.RemoveAt(i + 1);

                    this.DeleteInternal(predecessorChild, key);
                }
            }
        }

        private BTreeEntry<TKey, TValue> DeleteSuccessor(BTreeNode<TKey, TValue> node)
        {
            if (node.IsLeaf)
            {
                var result = node.Entries[0];
                node.Entries.RemoveAt(0);
                return result;
            }

            return this.DeletePredecessor(node.Children.First());
        }

        private BTreeEntry<TKey, TValue> DeletePredecessor(BTreeNode<TKey, TValue> node)
        {
            if (node.IsLeaf)
            {
                var result = node.Entries[node.Entries.Count - 1];
                node.Entries.RemoveAt(node.Entries.Count - 1);
                return result;
            }

            return this.DeletePredecessor(node.Children.Last());
        }

        public bool Remove(KeyValuePair<TKey, TValue> item)
        {
            throw new NotImplementedException();
        }

        public bool TryGetValue(TKey key, [MaybeNullWhen(false)] out TValue value)
        {
            BTreeEntry<TKey, TValue> entry = SearchInternal(Root, key);
            if(entry != null)
            {
                value = entry.Value;
                return true;
            }
            value = default(TValue);
            return false;
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            throw new NotImplementedException();
        }

        private void SplitNode(BTreeNode<TKey, TValue> parentNode, int nodeToBeSplitIndex, BTreeNode<TKey, TValue> nodeToBeSplit)
        {
            var newNode = new BTreeNode<TKey, TValue>(Degree);

            parentNode.Entries.Insert(nodeToBeSplitIndex, nodeToBeSplit.Entries[Degree - 1]);
            parentNode.Children.Insert(nodeToBeSplitIndex + 1, newNode);

            newNode.Entries.AddRange(nodeToBeSplit.Entries.GetRange(Degree, Degree - 1));
            nodeToBeSplit.Entries.RemoveRange(Degree - 1, Degree);

            if (!nodeToBeSplit.IsLeaf)
            {
                newNode.Children.AddRange(nodeToBeSplit.Children.GetRange(Degree, Degree));
                nodeToBeSplit.Children.RemoveRange(Degree, Degree);
            }
        }

        private void InsertNonFull(BTreeNode<TKey, TValue> node, TKey key, TValue value)
        {
            int position = node.Entries.TakeWhile(entry => key.CompareTo(entry.Key) >= 0).Count();

            //Insert in leaf node
            if(node.IsLeaf)
            {
                node.Entries.Insert(position, new BTreeEntry<TKey, TValue>() {Key = key, Value = value });
                return;
            }
            //Insert in non-leaf node
            BTreeNode<TKey, TValue> child = node.Children[position];
            if(child.HasReachedMaxEntries)
            {
                SplitNode(node, position, child);
                if(key.CompareTo(node.Entries[position].Key) > 0)
                {
                    position++;
                }
            }
            InsertNonFull(node.Children[position], key, value);
        }

        private BTreeEntry<TKey, TValue> SearchInternal(BTreeNode<TKey, TValue> node, TKey key)
        {
            int i = node.Entries.TakeWhile(entry => key.CompareTo(entry.Key) > 0).Count();

            if (i < node.Entries.Count && node.Entries[i].Key.CompareTo(key) == 0)
            {
                return node.Entries[i];
            }

            return node.IsLeaf ? null : this.SearchInternal(node.Children[i], key);
        }
    }
}
