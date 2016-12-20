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
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using NUnit.Framework;

namespace Dfust.Hotkeys.Util.Tests {

    [TestFixture]
    [ExcludeFromCodeCoverage]
    public class NestedDictionaryTests {

        [Test]
        public void NestedDictionary_InnerDictopnaryShouldNotBeNull() {
            var dict = new NestedDictionary<int, int>();
            Assert.IsNotNull(dict.InnerDictionary);
            Assert.That(dict.InnerDictionary.Count, Is.EqualTo(0));
        }

        [Test]
        public void NestedDictionary_ShouldClearEmptyDictionary() {
            //--- Assemble
            var dict = new NestedDictionary<int, int>();

            //--- Act
            dict.Clear();

            //---Assert
            Assert.IsNotNull(dict.InnerDictionary);
            Assert.That(dict.InnerDictionary.Count, Is.EqualTo(0));
        }

        [Test]
        public void NestedDictionary_ShouldCreateDictWhereOnePathIsACompletePrefixOfAnotherPath([Values(true, false)] bool doReverse) {
            //--- Assemble
            var paths = new List<int[]>();
            var values = new List<int>();

            paths.Add(new int[] { 1, 2, 3, 4 });
            paths.Add(new int[] { 1, 2, 3 });

            values.Add(10);
            values.Add(20);

            if (doReverse) {
                values.Reverse();
                paths.Reverse();
            }

            var dict = new NestedDictionary<int, int>();

            //--- Act
            dict.Add(paths.First(), values.First());
            dict.Add(paths.Last(), values.Last());

            //---Assert
            Assert.IsTrue(dict.ContainsPath(paths.First()));
            Assert.IsTrue(dict.ContainsPath(paths.Last()));

            Assert.That(dict.TryGetValue(paths.First()).Item1, Is.EqualTo(values.First()));
            Assert.That(dict.TryGetValue(paths.Last()).Item1, Is.EqualTo(values.Last()));
        }

        [Test]
        public void NestedDictionary_ShouldCreateDictWithMultiplePathsOfDepthN([Values(1, 2, 5, 10)] int n) {
            //---Assemble
            var paths = new Dictionary<int, int[]>();
            for (var path = 1; path <= 4; path++) {
                var keys = new int[n];
                for (var i = 1; i <= n; i++)
                    keys[i - 1] = i * (path + 2);
                paths.Add(path, keys);
            }

            //---Act
            var dict = new NestedDictionary<int, double>();
            foreach (var item in paths)
                dict.Add(item.Value, item.Key * 1000);

            //---Assert
            Assert.That(dict.InnerDictionary.Count, Is.EqualTo(4));
            foreach (var item in paths) {
                var keys = item.Value;
                Assert.IsTrue(dict.ContainsPath(keys));
                var path = item.Key;
                var current = dict.InnerDictionary;
                for (var i = 0; i < keys.Count(); i++) {
                    var key = keys[i];
                    var node = current[key];

                    if (i < keys.Count() - 1) {
                        Assert.IsTrue(current.ContainsKey(key));
                        current = node.Dictionary;
                    } else {
                        Assert.That(node.Value, Is.EqualTo(path * 1000));
                        Assert.IsFalse(node.IsDefaultValue);
                    }
                }
            }
        }

        [Test]
        public void NestedDictionary_ShouldCreateDictWithOnePathOfDepthN([Values(1, 2, 5, 10)] int n) {
            //---Assemble
            var keys = new int[n];
            for (var i = 1; i <= n; i++) {
                keys[i - 1] = i * 2;
            }

            //---Act

            var dict = new NestedDictionary<int, List<int>>();
            dict.Add(keys, new List<int>());

            //---Assert
            Assert.IsTrue(dict.ContainsPath(keys));
            var current = dict.InnerDictionary;
            foreach (var key in keys)
                if (key < n * 2) {
                    Assert.IsTrue(current.ContainsKey(key));
                    current = current[key].Dictionary;
                } else {
                    Assert.That(current.Count, Is.EqualTo(1));
                    Assert.IsTrue(current.Values.First().Value is List<int>);
                }
        }

        [Test]
        public void NestedDictionary_ShouldCreateDictWithTwoPathsOfDifferentDepth() {
            //---Assemble
            var paths = new Dictionary<int, int[]>();
            //One path with depth 5 (all even numbers)
            paths.Add(1, new int[] { 2, 4, 6, 8, 10 });
            //the other path has only a depth of 2 (all odd numbers)
            paths.Add(2, new int[] { 3, 17 });

            //---Act
            var dict = new NestedDictionary<int, double>();

            foreach (var item in paths)
                dict.Add(item.Value, item.Key * 1000);

            //---Assert
            Assert.That(dict.InnerDictionary.Count, Is.EqualTo(2));
            foreach (var item in paths) {
                var keys = item.Value;
                Assert.IsTrue(dict.ContainsPath(keys));
                var path = item.Key;
                var current = dict.InnerDictionary;
                for (var i = 0; i < keys.Count(); i++) {
                    var key = keys[i];
                    var node = current[key];
                    if (i < keys.Count() - 1) {
                        Assert.IsTrue(current.ContainsKey(key));
                        current = node.Dictionary;
                    } else {
                        Assert.That(node.Value, Is.EqualTo(path * 1000));
                    }
                }
            }
        }

        [Test]
        public void NestedDictionary_ShouldCreateDictWithTwoPathsOfDifferentDepthAndPartiallySharedKeys() {
            //---Assemble
            var paths = new Dictionary<int, int[]>();
            //One path with depth 5 (all even numbers). The first key (=> 2) appears in the second path, too!
            paths.Add(1, new int[] { 2, 4, 6, 8, 10 });
            //the other path has only a depth of 3. The first key (=> 2) appears in the first path, too!
            paths.Add(2, new int[] { 2, 3, 17 });

            //---Act
            var dict = new NestedDictionary<int, double>();

            foreach (var item in paths)
                dict.Add(item.Value, item.Key * 1000);

            //---Assert
            Assert.That(dict.InnerDictionary.Count, Is.EqualTo(1));
            foreach (var item in paths) {
                var keys = item.Value;
                Assert.IsTrue(dict.ContainsPath(keys));
                var path = item.Key;
                var current = dict.InnerDictionary;
                for (var i = 0; i < keys.Count(); i++) {
                    var key = keys[i];
                    var node = current[key];
                    if (i < keys.Count() - 1) {
                        Assert.IsTrue(current.ContainsKey(key));
                        current = node.Dictionary;
                    } else {
                        Assert.That(node.Value, Is.EqualTo(path * 1000));
                    }
                }
            }
        }

        [Test]
        public void NestedDictionary_ShouldFindPathAndSubPath() {
            //--- Assemble
            var path1 = new int[] { 1, 2, 3 };
            const int value1 = 158;

            //path2 is a sub-path of path1
            var path2 = new int[] { 1, 2 };
            const int value2 = 589;

            var dict = new NestedDictionary<int, int>();
            dict.Add(path1, value1);
            dict.Add(path2, value2);

            //--- Act

            var allPaths = dict.GetAllPaths();

            //---Assert

            Assert.That(allPaths.Count, Is.EqualTo(2));

            foreach (var path in allPaths) {
                for (int i = 1; i <= path.Count(); i++) {
                    Assert.That(path.Contains(i));
                }
            }
        }

        [Test]
        public void NestedDictionary_ShouldFindPathsAndSubPath() {
            //--- Assemble
            var path1 = new int[] { 1, 2, 3 };
            const int value1 = 158;

            var path3 = new int[] { 1, 2, 5, 6 };
            const int value3 = 978;

            //path2 is a sub-path of path1 and path3
            var path2 = new int[] { 1, 2 };
            const int value2 = 589;

            var dict = new NestedDictionary<int, int>();
            dict.Add(path1, value1);
            dict.Add(path2, value2);
            dict.Add(path3, value3);

            //--- Act

            var allPaths = dict.GetAllPaths();

            //---Assert

            Assert.That(allPaths.Count, Is.EqualTo(3));

            foreach (var path in allPaths) {
                if (path.Count() <= 3) {
                    for (int i = 1; i <= path.Count(); i++) {
                        Assert.That(path.Contains(i));
                    }
                } else {
                    foreach (var number in path3) {
                        Assert.That(path.Contains(number));
                    }
                }
            }
        }

        [Test]
        public void NestedDictionary_ShouldFindSinglePath([Values(1, 2, 3, 4, 10)] int pathLength) {
            //--- Assemble
            var path = Enumerable.Range(1, pathLength).ToArray();
            const int value = 158;

            var dict = new NestedDictionary<int, int>();
            dict.Add(path, value);

            //--- Act

            var allPaths = dict.GetAllPaths();

            //---Assert

            Assert.That(allPaths.Count, Is.EqualTo(1));
            var foundPath = allPaths.First();
            Assert.That(foundPath.Count(), Is.EqualTo(pathLength));
            for (int i = 1; i <= pathLength; i++) {
                Assert.That(foundPath.Contains(i));
            }
        }

        [Test]
        public void NestedDictionary_ShouldFindSubpathsInDictionary() {
            //--- Assemble
            var dict = new NestedDictionary<int, int>();

            var path1 = new int[] { 1, 2, 3, 4, 5, 6 };
            var path2 = new int[] { 1, 2, 3, 10, 20, 30 };
            var path3 = new int[] { 21, 22, 23, 24, 25, 26 };

            dict.Add(path1, 1000);
            dict.Add(path2, 2000);
            dict.Add(path3, 3000);

            //--- Act

            foreach (var path in new int[][] { path1, path2, path3 }) {
                for (int i = 0; i < path.Length; i++) {
                    var subpath = path.Take(i);
                    //---Assert
                    Assert.IsTrue(dict.ContainsSubpath(subpath));
                }
            }
        }

        [Test]
        public void NestedDictionary_ShouldInitializeDictionary() {
            //---Act
            var dict = new NestedDictionary<int, double>();
            //---Assert
            Assert.IsNotNull(dict.InnerDictionary);
            Assert.That(dict.InnerDictionary.Count, Is.EqualTo(0));
        }

        [Test]
        public void NestedDictionary_ShouldNotContainDifferentPathWithSameLength() {
            //---Assemble
            var path = new List<int> { 1, 5, 9 };
            var dict = new NestedDictionary<int, double>();
            const double expected = 8.9;
            dict.Add(path, expected);

            //---Assert
            Assert.IsTrue(dict.ContainsPath(path));
            Assert.That(dict.TryGetValue(path).Item1, Is.EqualTo(expected));
            path.Reverse();
            Assert.IsFalse(dict.ContainsPath(path));
        }

        [Test]
        public void NestedDictionary_ShouldNotContainLongerPath() {
            //---Assemble
            var path = new List<int> { 1, 5, 9 };
            var dict = new NestedDictionary<int, double>();
            const double expected = 8.9;
            dict.Add(path, expected);

            //---Assert
            Assert.IsTrue(dict.ContainsPath(path));
            Assert.That(dict.TryGetValue(path).Item1, Is.EqualTo(expected));

            path.Add(89);

            Assert.IsFalse(dict.ContainsPath(path));
        }

        [Test]
        public void NestedDictionary_ShouldNotContainShorterPath() {
            //---Assemble
            var path = new List<int> { 1, 5, 9 };
            var dict = new NestedDictionary<int, double>();
            const double expected = 8.9;
            dict.Add(path, expected);

            //---Assert
            Assert.IsTrue(dict.ContainsPath(path));
            Assert.That(dict.TryGetValue(path).Item1, Is.EqualTo(expected));

            path.RemoveAt(path.Count - 1);

            Assert.IsFalse(dict.ContainsPath(path));
            Assert.That(dict.TryGetValue(path).Item2, Is.EqualTo(Succes.NoValueFound_ChildsPresent));
        }

        [Test]
        public void NestedDictionary_ShouldNotFindSubpathsInDictionary() {
            //--- Assemble
            var dict = new NestedDictionary<int, int>();

            var path1 = new int[] { 1, 2, 3, 4, 5, 6 };

            dict.Add(path1, 1000);

            var subpaths = new List<int[]>();
            subpaths.Add(new int[] { 2, 3 });
            subpaths.Add(new int[] { 1, 2, 3, 5, 6 });
            subpaths.Add(new int[] { 1, 2, 3, 4, 5, 6, 7 });

            //---Assert
            foreach (var subpath in subpaths) {
                Assert.IsFalse(dict.ContainsSubpath(subpath));
            }
        }

        [Test]
        public void NestedDictionary_ShouldRemoveLongPathsAndCheckForShortPathsToStillExist([Values(2, 3, 5, 10)]int maxPathLength) {
            //--- Assemble
            var dict = new NestedDictionary<int, double>();
            for (int i = 1; i <= maxPathLength; i++) {
                //Build a dictionary with all paths {1}, {1,2},{1,2,3},...,{1,2,3,4,5,6,7,8,9,10}
                dict.Add(Enumerable.Range(1, i).ToArray(), 111);
            }

            for (int i = maxPathLength; i > 0; i--) {
                //--- Act

                //Remove longest path
                var path = Enumerable.Range(1, i).ToArray();
                dict.Remove(path);

                //---Assert
                Assert.IsFalse(dict.ContainsPath(path));

                //assert that all shorter paths are still there
                for (int j = 1; j < i; j++) {
                    var assertPath = Enumerable.Range(1, j).ToArray();
                    Assert.IsTrue(dict.ContainsPath(assertPath));
                }
            }
        }

        [Test]
        public void NestedDictionary_ShouldRemovePath1([Values(1, 2, 5, 10)] int pathLength) {
            //--- Assemble
            var dict = new NestedDictionary<int, int>();

            var path = Enumerable.Range(1, pathLength).ToArray();
            const int value = 10;

            dict.Add(path, value);

            Assert.IsTrue(dict.ContainsPath(path));
            Assert.That(dict.TryGetValue(path).Item1, Is.EqualTo(value));

            //--- Act
            dict.Remove(path);

            //---Assert
            Assert.IsFalse(dict.ContainsPath(path));
            Assert.That(dict.InnerDictionary.Count, Is.EqualTo(0));
        }

        [Test]
        public void NestedDictionary_ShouldRemovePath2([Values(true, false)] bool toggle) {
            //--- Assemble
            var dict = new NestedDictionary<int, int>();

            var path1 = new int[] { 1, 2, 3, 4 };
            const int value1 = 10;

            var path2 = new int[] { 1, 2, 3, 44 };
            const int value2 = 20;

            var path3 = new int[] { 1, 2, 3 };
            const int value3 = 30;

            //Add two paths with same first sequence
            dict.Add(path1, value1);
            dict.Add(path2, value2);
            //Add a path that is the common sequence of paths 1 and 2
            dict.Add(path3, value3);

            Assert.IsTrue(dict.ContainsPath(path1));
            Assert.That(dict.TryGetValue(path1).Item1, Is.EqualTo(value1));

            Assert.IsTrue(dict.ContainsPath(path2));
            Assert.That(dict.TryGetValue(path2).Item1, Is.EqualTo(value2));

            Assert.IsTrue(dict.ContainsPath(path3));
            Assert.That(dict.TryGetValue(path3).Item1, Is.EqualTo(value3));

            //--- Act

            var removePaths = new List<int[]> { path1, path3 };
            if (toggle)
                removePaths.Reverse();

            dict.Remove(removePaths.First());
            dict.Remove(removePaths.Last());

            //---Assert
            Assert.IsFalse(dict.ContainsPath(path1));
            Assert.IsFalse(dict.ContainsPath(path3));
            Assert.IsTrue(dict.ContainsPath(path2));
            Assert.That(dict.TryGetValue(path2).Item1, Is.EqualTo(value2));
        }

        [Test]
        public void NestedDictionary_ShouldRemovePath3([Values(true, false)] bool toggle) {
            //--- Assemble
            var dict = new NestedDictionary<int, int>();

            var path1 = new int[] { 1, 2 };
            const int value1 = 10;

            var path2 = new int[] { 1 };
            const int value2 = 20;

            //Add two paths with same first sequence
            dict.Add(path1, value1);
            dict.Add(path2, value2);

            Assert.IsTrue(dict.ContainsPath(path1));
            Assert.That(dict.TryGetValue(path1).Item1, Is.EqualTo(value1));

            Assert.IsTrue(dict.ContainsPath(path2));
            Assert.That(dict.TryGetValue(path2).Item1, Is.EqualTo(value2));

            //We put both paths/values into a list. The first is to be removed, the second stays.
            var removePaths = new List<int[]> { path1, path2 };
            var values = new List<int> { value1, value2 };
            if (toggle) {
                removePaths.Reverse();
                values.Reverse();
            }

            //--- Act
            dict.Remove(removePaths.First());

            //---Assert

            Assert.IsFalse(dict.ContainsPath(removePaths.First()));

            Assert.IsTrue(dict.ContainsPath(removePaths.Last()));
            Assert.That(dict.TryGetValue(removePaths.Last()).Item1, Is.EqualTo(values.Last()));
        }

        [Test]
        public void NestedDictionary_ShouldRemovePath4([Values(true, false)] bool toggle, [Values(2, 3, 4, 10)] int pathLength) {
            //--- Assemble
            var dict = new NestedDictionary<int, int>();

            var path1 = Enumerable.Range(1, pathLength).ToArray();
            const int value1 = 10;

            var path2 = new int[] { 1 };
            const int value2 = 20;

            //Add two paths with same first sequence
            dict.Add(path1, value1);
            dict.Add(path2, value2);

            Assert.IsTrue(dict.ContainsPath(path1));
            Assert.That(dict.TryGetValue(path1).Item1, Is.EqualTo(value1));

            Assert.IsTrue(dict.ContainsPath(path2));
            Assert.That(dict.TryGetValue(path2).Item1, Is.EqualTo(value2));

            //We put both paths/values into a list. The first is to be removed, the second stays.
            var removePaths = new List<int[]> { path1, path2 };
            var values = new List<int> { value1, value2 };
            if (toggle) {
                removePaths.Reverse();
                values.Reverse();
            }

            //--- Act
            dict.Remove(removePaths.First());

            //---Assert

            Assert.IsFalse(dict.ContainsPath(removePaths.First()));

            Assert.IsTrue(dict.ContainsPath(removePaths.Last()));
            Assert.That(dict.TryGetValue(removePaths.Last()).Item1, Is.EqualTo(values.Last()));
        }

        [Test]
        public void NestedDictionary_ShouldRemoveShortPathsAndCheckForLongPathsToStillExist() {
            //--- Assemble
            var dict = new NestedDictionary<int, double>();
            for (int i = 1; i <= 10; i++) {
                //Build a dictionary with all paths {1}, {1,2},{1,2,3},...,{1,2,3,4,5,6,7,8,9,10}
                dict.Add(Enumerable.Range(1, i).ToArray(), 0);
            }

            for (int i = 1; i <= 10; i++) {
                //--- Act

                //Remove shortest path
                var path = Enumerable.Range(1, i).ToArray();
                dict.Remove(path);

                //---Assert
                Assert.IsFalse(dict.ContainsPath(path));

                //assert that all longer  paths are still there
                for (int j = 1 + i; j <= 10; j++) {
                    var assertPath = Enumerable.Range(1, j).ToArray();
                    Assert.IsTrue(dict.ContainsPath(assertPath), $"i:{i}, j:{j}");
                }
            }
        }

        [Test]
        public void NestedDictionary_ShouldReturnCorrectResultForTryGetValue() {
            //---Assemble

            var dict = new NestedDictionary<int, double>();
            var path1 = new List<int> { 1, 5, 9, 63 };
            var path2 = new List<int> { 1, 5, 9 };
            var path3 = new List<int> { 1, 5, -96 };

            const double expected1 = 8.9;
            const double expected2 = -99;
            const double expected3 = 33.33;
            dict.Add(path1, expected1);
            dict.Add(path2, expected2);
            dict.Add(path3, expected3);

            //---Assert

            Assert.That(dict.GetAllPaths().Count(), Is.EqualTo(3));

            //Path is not in Dictionary in any way- it's neither a valid path nor a sub path of a valid path
            var currentPath = new int[] { 33 };
            var result = dict.TryGetValue(currentPath);
            Assert.IsFalse(dict.ContainsPath(currentPath));
            Assert.That(result.Item1, Is.EqualTo(0.0)); //DefaultValue of double, since the TValue of NestedDictionary is of type double
            Assert.That(result.Item2, Is.EqualTo(Succes.NoValueFound_NoChilds));

            //Path is not a valid path, but it's a sub path of a valid path
            currentPath = new int[] { 1 };
            result = dict.TryGetValue(currentPath);
            Assert.IsFalse(dict.ContainsPath(currentPath));
            Assert.That(result.Item1, Is.EqualTo(0.0)); //DefaultValue of double, since the TValue of NestedDictionary is of type double
            Assert.That(result.Item2, Is.EqualTo(Succes.NoValueFound_ChildsPresent));

            //Path is not a valid path, but it's a sub path of a valid path
            currentPath = new int[] { 1, 5 };
            result = dict.TryGetValue(currentPath);
            Assert.IsFalse(dict.ContainsPath(currentPath));
            Assert.That(result.Item1, Is.EqualTo(0.0)); //DefaultValue of double, since the TValue of NestedDictionary is of type double
            Assert.That(result.Item2, Is.EqualTo(Succes.NoValueFound_ChildsPresent));

            //One side of the branch: Path is a valid path, and it is not a sub path of another valid path
            currentPath = new int[] { 1, 5, -96 };
            result = dict.TryGetValue(currentPath);
            Assert.IsTrue(dict.ContainsPath(currentPath));
            Assert.That(result.Item1, Is.EqualTo(expected3));
            Assert.That(result.Item2, Is.EqualTo(Succes.ValueFound_NoChilds));

            //The other side of the branch: Path is a valid path, and it is a sub path of another valid path
            currentPath = new int[] { 1, 5, 9 };
            result = dict.TryGetValue(currentPath);
            Assert.IsTrue(dict.ContainsPath(currentPath));
            Assert.That(result.Item1, Is.EqualTo(expected2));
            Assert.That(result.Item2, Is.EqualTo(Succes.ValueFound_ChildsPresent));

            //The other side of the branch: Path is a valid path, and it is not a sub path of another valid path
            currentPath = new int[] { 1, 5, 9, 63 };
            result = dict.TryGetValue(currentPath);
            Assert.IsTrue(dict.ContainsPath(currentPath));
            Assert.That(result.Item1, Is.EqualTo(expected1));
            Assert.That(result.Item2, Is.EqualTo(Succes.ValueFound_NoChilds));

            //Path is not in Dictionary in any way- it's neither a valid path nor a sub path of a valid path.
            //path1 and path2 are sub paths of this path
            currentPath = new int[] { 1, 5, 9, 63, 999 };
            result = dict.TryGetValue(currentPath);
            Assert.IsFalse(dict.ContainsPath(currentPath));
            Assert.That(result.Item1, Is.EqualTo(0.0)); //DefaultValue of double, since the TValue of NestedDictionary is of type double
            Assert.That(result.Item2, Is.EqualTo(Succes.NoValueFound_NoChilds));
        }

        [Test]
        public void NestedDictionary_ShouldReturnCountOfLongestPath1() {
            //--- Assemble
            var dict = new NestedDictionary<int, int>();

            Assert.That(dict.LongestPathCount, Is.EqualTo(0));

            for (int i = 1; i <= 10; i++) {
                var currentPath = Enumerable.Range(1, i).ToArray();
                //add path
                dict.Add(currentPath, 1);
                //check length
                Assert.That(dict.LongestPathCount, Is.EqualTo(i));

                //remover path
                dict.Remove(currentPath);
                //check length
                Assert.That(dict.LongestPathCount, Is.EqualTo(0));
            }
        }

        [Test]
        public void NestedDictionary_ShouldReturnCountOfLongestPath2() {
            //--- Assemble
            var dict = new NestedDictionary<int, int>();
            const int max = 3;
            for (int i = 1; i <= max; i++) {
                var currentPath = Enumerable.Range(1, i).ToArray();
                dict.Add(currentPath, 1);
            }

            Assert.That(dict.LongestPathCount, Is.EqualTo(max));

            for (int i = max; i > 0; i--) {
                var currentPath = Enumerable.Range(1, i).ToArray();
                dict.Remove(currentPath);
                Assert.That(dict.LongestPathCount, Is.EqualTo(i - 1));
            }
        }

        [Test]
        public void NestedDictionary_ShouldThrowWhenAddingTheSamePathTwice() {
            //--- Assemble
            var path = new int[] { 1, 2, 3, 4 };
            const int value = 158;

            var dict = new NestedDictionary<int, int>();

            //--- Act

            //Add path once
            dict.Add(path, value);

            //---Assert

            //Add the same path with different value. This should fail
            Assert.Throws<ArgumentException>(() => dict.Add(path, value + 1));
            //Add the same path with same value. This should fail
            Assert.Throws<ArgumentException>(() => dict.Add(path, value));
        }
    }
}