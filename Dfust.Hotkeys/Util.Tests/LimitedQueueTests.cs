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
using System.Linq;
using NUnit.Framework;

namespace Dfust.Hotkeys.Util.Tests {

    [TestFixture]
    public class LimitedQueueTests {

        [Test]
        public void LimitedQueueShouldBeInstantiable() {
            Assert.DoesNotThrow(() => new LimitedQueue<int>(2));
        }

        [Test]
        public void ShouldClearLimitedQueue() {
            //--- Assemble
            var lq = new LimitedQueue<int>(5);

            for (int i = 0; i < 5; i++) {
                lq.Enqueue(i);
            }

            Assert.That(lq.Count, Is.EqualTo(5));

            //--- Act

            lq.Clear();

            //---Assert
            Assert.That(lq.Count, Is.EqualTo(0));
        }

        /// <summary>
        /// Should never grow beyond limit and contain the correct values.
        /// </summary>
        /// <param name="limit">The limit.</param>
        [Test]
        public void ShouldNeverGrowBeyondLimit([Values(1, 2, 5, 10, 50)] int limit) {
            //--- Assemble
            var lq = new LimitedQueue<int>(limit);
            Assert.That(lq.Count, Is.EqualTo(0));

            for (int i = 1; i < (2 * limit); i++) {
                //--- Act
                lq.Enqueue(i);

                //---Assert
                var expected = Math.Min(i, limit);
                Assert.That(lq.Count, Is.EqualTo(expected));

                var expectedValues = Enumerable.Range(i - lq.Count + 1, lq.Count).ToArray();
                var j = 0;

                foreach (var item in lq) {
                    Assert.That(item, Is.EqualTo(expectedValues[j]));
                    j++;
                }
            }
        }

        [Test]
        public void ShouldRetrieveCorrectLimit() {
            //--- Assemble
            const int limit = 5;

            //--- Act
            var lq = new LimitedQueue<int>(limit);
            //---Assert

            Assert.That(lq.Limit, Is.EqualTo(limit));
        }

        [Test]
        public void ShouldReturnLastElements() {
            //--- Assemble
            const int limit = 5;
            var lq = new LimitedQueue<int>(limit);

            Assert.IsNull(lq.Last(5));

            //--- Act

            for (int i = 0; i < limit * 2; i++) {
                lq.Enqueue(i);

                Assert.That(lq.Last(), Is.EqualTo(i));

                for (int j = 1; j <= limit; j++) {
                    var expectedValues = Enumerable.Range(0, i + 1).Skip(Math.Max(0, i - j + 1)).Take(j).ToArray();

                    var k = 0;
                    foreach (var item in lq.Last(j)) {
                        Assert.That(item, Is.EqualTo(expectedValues[k]));
                        k++;
                    }
                }
            }
        }

        [Test]
        public void ShouldReturnNullWhenCallingLastOnEmptyQueue() {
            //--- Assemble
            var lq = new LimitedQueue<int>(5);
            //--- Act
            var result = lq.Last(1);
            //---Assert
            Assert.IsNull(result);
        }

        [Test]
        public void ShouldThrowOnLastWithArgumentSmaller1() {
            var lq = new LimitedQueue<int>(2);
            Assert.Throws<ArgumentException>(() => lq.Last(0));
            Assert.Throws<ArgumentException>(() => lq.Last(-1));

            lq.Enqueue(2);
            lq.Enqueue(5);

            Assert.Throws<ArgumentException>(() => lq.Last(0));
            Assert.Throws<ArgumentException>(() => lq.Last(-1));
        }

        [Test]
        public void ShouldThrowOnNegativeLimitOrZero() {
            Assert.Throws<ArgumentException>(() => new LimitedQueue<int>(0));
            Assert.Throws<ArgumentException>(() => new LimitedQueue<int>(-1));
        }
    }
}