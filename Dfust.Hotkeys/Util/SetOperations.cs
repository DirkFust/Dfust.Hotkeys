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

    public static class SetOperations {
        private static readonly Random m_random = new Random();

        public static List<List<T>> CartesianProduct<T>(List<List<T>> input) {
            var result = new List<List<T>>();
            CartesianProductInternal(input, new T[input.Count()], 0, result);
            return result;
        }

        /// <summary>
        /// Creates a power set of the sequence.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="sequence">The sequence.</param>
        /// <returns></returns>
        /// <remarks>Algorithm from http://stackoverflow.com/a/19891145</remarks>
        public static IEnumerable<IEnumerable<T>> GetPowerset<T>(IEnumerable<T> sequence) {
            var seq = sequence.ToArray();
            var powerSet = new T[1 << seq.Length][];
            powerSet[0] = new T[0]; // starting only with empty set
            for (var i = 0; i < seq.Length; i++) {
                var cur = seq[i];
                var count = 1 << i; // doubling list each time
                for (int j = 0; j < count; j++) {
                    var source = powerSet[j];
                    var destination = powerSet[count + j] = new T[source.Length + 1];
                    for (int q = 0; q < source.Length; q++)
                        destination[q] = source[q];
                    destination[source.Length] = cur;
                }
            }
            return powerSet;
        }

        /// <summary>
        /// Shuffles the sequence.
        /// </summary>
        /// <typeparam name="T">element type.</typeparam>
        /// <param name="seq">Sequence to shuffle.</param>
        public static IEnumerable<T> Shuffle<T>(IEnumerable<T> seq) {
            var array = seq.ToArray();
            var n = array.Length;
            for (int i = 0; i < n; i++) {
                // NextDouble returns a random number between 0 and 1.
                var r = i + (int)(m_random.NextDouble() * (n - i));
                var t = array[r];
                array[r] = array[i];
                array[i] = t;
            }
            return array;
        }

        private static void CartesianProductInternal<T>(List<List<T>> input, T[] current, int k, List<List<T>> result) {
            if (k == input.Count()) {
                result.Add(current.ToList());
            } else {
                for (int j = 0; j < input[k].Count(); j++) {
                    current[k] = input[k][j];
                    CartesianProductInternal(input, current, k + 1, result);
                }
            }
        }
    }
}