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

using System.Diagnostics.CodeAnalysis;
using System.Windows.Forms;
using NUnit.Framework;

namespace Dfust.Hotkeys.Util.Tests {

    [TestFixture, ExcludeFromCodeCoverage]
    public class Keys2StringTests {

        [Test]
        public void ShouldCreateSingleKey() {
            Assert.That(Keys2String.KeyToString(Keys.A), Is.EqualTo("a"));
        }

        [Test]
        public void ShouldCreateSingleKeyWithModifier([Values(Keys.Shift, Keys.Alt, Keys.Control)] Keys modifier) {
            Assert.That(Keys2String.KeyToString(Keys.A | modifier), Is.EqualTo($"{modifier}+a"));
        }

        [Test]
        public void ShouldCreateSingleKeyWithModifiers() {
            const Keys key = Keys.Shift | Keys.A | Keys.Alt;
            Assert.That(Keys2String.KeyToString(key), Is.EqualTo($"{Keys.Alt}+{Keys.Shift}+a"));
        }
    }
}