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

using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;

namespace Dfust.Hotkeys.Util.Tests {

    [TestFixture]
    public class SetOperationsTests {

        [Test]
        public void CartesianProductTest1() {
            var input = new List<List<int>>();
            input.Add(new List<int> { 5 });
            input.Add(new List<int> { 7 });

            var result = SetOperations.CartesianProduct(input);

            Assert.That(result.Count, Is.EqualTo(1));
            Assert.That(result.First().Count(), Is.EqualTo(2));
            Assert.IsTrue(result.First().Contains(5));
            Assert.IsTrue(result.First().Contains(7));
        }

        [Test]
        public void CartesianProductTest2() {
            var input = new List<List<int>>();
            input.Add(new List<int> { 4, 5 });
            input.Add(new List<int> { 6, 7 });

            var result = SetOperations.CartesianProduct(input);

            Assert.That(result.Count, Is.EqualTo(4));
            Assert.That(result.First().Count(), Is.EqualTo(2));

            Assert.IsTrue(result[0].Contains(4));
            Assert.IsTrue(result[0].Contains(6));

            Assert.IsTrue(result[1].Contains(4));
            Assert.IsTrue(result[1].Contains(7));

            Assert.IsTrue(result[2].Contains(5));
            Assert.IsTrue(result[2].Contains(6));

            Assert.IsTrue(result[3].Contains(5));
            Assert.IsTrue(result[3].Contains(7));
        }
    }
}