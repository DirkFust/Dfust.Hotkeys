#region copyright

/* The MIT License (MIT)
// Copyright (c) 2016 Dirk Fust
//
// Permission is hereby granted, free of charge, to any person obtaining a copy of this software and
// associated documentation files (the "Software"), to deal in the Software without restriction,
// including without limitation the rights to use, copy, modify, merge, publish, distribute,
// sublicense, and/or sell copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
//
// The above copyright notice and this permission notice shall be included in all copies or
// substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT
// NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM,
// DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT
// OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
*/

#endregion copyright

using System;
using System.Collections.Generic;
using System.Linq;

namespace Dfust.Hotkeys.Util {

    public enum Succes : byte {
        ValueFound_NoChilds = 0,
        ValueFound_ChildsPresent,
        NoValueFound_NoChilds,
        NoValueFound_ChildsPresent
    }

    /// <summary>
    /// This class represents a tree with leafs of type TValue and branches of type TKey. It is
    /// possible to have
    /// </summary>
    /// <typeparam name="TKey"></typeparam>
    /// <typeparam name="TValue"></typeparam>
    public class NestedDictionary<TKey, TValue> {

        //The root of the tree
        private readonly Dictionary<TKey, NestedDictionaryNode> m_root;

        private int m_longestPathCount;

        public NestedDictionary() {
            m_root = new Dictionary<TKey, NestedDictionaryNode>();
        }

        public Dictionary<TKey, NestedDictionaryNode> InnerDictionary => m_root;

        public int LongestPathCount { get { return m_longestPathCount; } }

        /// <summary>
        /// Adds a path of type TKey to the value of type TValue to the Dictionary
        /// </summary>
        /// <param name="path">Path to the value</param>
        /// <param name="value">Value</param>
        public void Add(IList<TKey> path, TValue value) {
            var currentLevel = m_root;
            for (var i = 0; i < path.Count(); i++) {
                var key = path[i];
                if (i != path.Count() - 1) {
                    //Add new level, if necessary
                    if (!currentLevel.ContainsKey(key)) {
                        currentLevel.Add(key, new NestedDictionaryNode());
                    }
                    currentLevel = currentLevel[key].Dictionary;
                } else {
                    //Add leaf value
                    if (!currentLevel.ContainsKey(key)) {
                        currentLevel.Add(key, new NestedDictionaryNode());
                    }
                    var node = currentLevel[key];
                    if (node.IsDefaultValue) {
                        node.Value = value;
                    } else {
                        throw new ArgumentException("Path already present. Can't add the same path twice.");
                    }
                }
            }
            if (path.Count > m_longestPathCount) {
                m_longestPathCount = path.Count;
            }
        }

        /// <summary>
        /// Clears this instance.
        /// </summary>
        public void Clear() {
            m_root.Clear();
        }

        /// <summary>
        /// Checks if the dictionary contains the specified path
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        public bool ContainsPath(IList<TKey> path) {
            var result = TryGetValue(path).Item2;
            return result == Succes.ValueFound_ChildsPresent | result == Succes.ValueFound_NoChilds;
        }

        /// <summary>
        /// Determines whether the specified subpath exists in the NestedDictionary.
        /// </summary>
        /// <param name="subpath">The subpath.</param>
        /// <returns>
        /// <c>true</c> if the specified subpath exists in the NestedDictionary; otherwise, <c>false</c>.
        /// </returns>
        public bool ContainsSubpath(IEnumerable<TKey> subpath) {
            var path = subpath.ToList();
            var success = true;
            var level = m_root;
            for (var i = 0; i < subpath.Count(); i++) {
                var key = path[i];
                if (i < path.Count) {
                    if (level.ContainsKey(key)) {
                        var node = level[key];
                        level = node.Dictionary;
                    } else {
                        success = false;
                        break;
                    }
                }
            }

            return success;
        }

        /// <summary>
        /// Finds all paths in the dictionary that lead to a value.
        /// </summary>
        /// <returns></returns>
        public IEnumerable<TKey[]> GetAllPaths() {
            var results = new List<TKey[]>();
            Traverse(m_root, new List<TKey>(), results);
            return results;
        }

        /// <summary>
        /// Removes the specified path.
        /// </summary>
        /// <param name="path">The path.</param>
        public void Remove(TKey[] path) {
            var levels = new List<Dictionary<TKey, NestedDictionaryNode>> { m_root };

            //Collect nodes on path
            for (var i = 0; i < path.Count(); i++) {
                var key = path[i];
                if (levels.Last().ContainsKey(key)) {
                    var node = levels.Last()[key];
                    if (i < path.Count()) {
                        levels.Add(node.Dictionary);
                    }
                }
            }

            var maxIndex = levels.Count() - 2;
            for (int index = maxIndex; index >= 0; index--) {
                var node = levels[index];
                var key = path[index];

                var hasChildren = node[key].Dictionary.Any();

                if (index == maxIndex) {
                    if (!hasChildren) {
                        node.Remove(key);
                    } else {
                        node[key].ResetValue();
                    }
                } else {
                    if (!hasChildren && node[key].IsDefaultValue) {
                        node.Remove(key);
                    } else {
                        break;
                    }
                }
            }

            m_longestPathCount = GetAllPaths().Select((p) => p.Count())
                                              .Union(new int[] { 0 })
                                              .Max();
        }

        /// <summary>
        /// Retrieves the value for the path.
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        public Tuple<TValue, Succes> TryGetValue(IList<TKey> path) {
            var success = Succes.NoValueFound_NoChilds;
            var value = default(TValue);
            var level = m_root;
            for (var i = 0; i < path.Count(); i++) {
                var key = path[i];
                if (i < path.Count) {
                    if (level.ContainsKey(key)) {
                        var node = level[key];
                        if (i < path.Count - 1) {
                            level = node.Dictionary;
                        } else {
                            if (!node.IsDefaultValue) {
                                success = Succes.ValueFound_NoChilds;
                                if (node.Dictionary.Keys.Any()) {
                                    success = Succes.ValueFound_ChildsPresent;
                                }
                                value = node.Value;
                            } else if (node.Dictionary.Keys.Any()) {
                                success = Succes.NoValueFound_ChildsPresent;
                            }
                            break;
                        }
                    } else {
                        success = Succes.NoValueFound_NoChilds;
                        break;
                    }
                }
            }
            return new Tuple<TValue, Succes>(value, success);
        }

        /// <summary>
        /// Traverses the specified root recursively and collects all found paths.
        /// </summary>
        /// <param name="root">The root node from which to start.</param>
        /// <param name="currentPath">The current path.</param>
        /// <param name="foundPaths">All found paths.</param>
        private void Traverse(Dictionary<TKey, NestedDictionaryNode> root, IList<TKey> currentPath, IList<TKey[]> foundPaths) {
            foreach (var key in root.Keys) {
                //update path
                var newPath = new List<TKey>(currentPath);
                newPath.Add(key);

                //collect valid paths
                if (!root[key].IsDefaultValue) {
                    foundPaths.Add(newPath.ToArray());
                }

                //Recursively traverse children
                Traverse(root[key].Dictionary, newPath, foundPaths);
            }
        }

        /// <summary>
        /// A node in the NestedDictionary
        /// </summary>
        public class NestedDictionaryNode {
            private readonly Dictionary<TKey, NestedDictionaryNode> m_dict = new Dictionary<TKey, NestedDictionaryNode>();
            private bool m_isDefaultValue = true;
            private TValue m_value;

            public Dictionary<TKey, NestedDictionaryNode> Dictionary {
                get { return m_dict; }
            }

            public bool IsDefaultValue {
                get { return m_isDefaultValue; }
            }

            public TValue Value {
                get { return m_value; }
                set {
                    m_isDefaultValue = false;
                    m_value = value;
                }
            }

            public void ResetValue() {
                m_value = default(TValue);
                m_isDefaultValue = true;
            }
        }
    }
}