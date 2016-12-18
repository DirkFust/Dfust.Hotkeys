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
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Dfust.Hotkeys.Util {

    public class LimitedQueue<T> : IEnumerable<T> {
        private readonly int m_limit;
        private readonly List<T> m_queue = new List<T>();

        public LimitedQueue(int limit) {
            if (limit < 1) {
                throw new ArgumentException($"{nameof(limit)} must greater than 0");
            }
            m_limit = limit;
        }

        public int Count {
            get { return m_queue.Count; }
        }

        public int Limit {
            get {
                return m_limit;
            }
        }

        public void Clear() {
            m_queue.Clear();
        }

        public void Dequeue() {
            m_queue.RemoveAt(0);
        }

        public void Enqueue(T item) {
            while (Count >= Limit) {
                Dequeue();
            }
            m_queue.Add(item);
        }

        public IEnumerator<T> GetEnumerator() {
            return ((IEnumerable<T>)m_queue).GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator() {
            return ((IEnumerable<T>)m_queue).GetEnumerator();
        }

        public T[] Last(int count) {
            if (count < 1) {
                throw new ArgumentException($"{nameof(count)} must be greater than 0");
            }

            var length = m_queue.Count();
            if (length == 0) {
                return null;
            }

            var last = new List<T>();

            for (int i = Math.Max(0, length - count); i < length; i++) {
                last.Add(m_queue[i]);
            }

            return last.ToArray();
        }
    }
}