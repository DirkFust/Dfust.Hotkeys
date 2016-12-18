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
using System.Windows.Forms;
using Dfust.Hotkeys.Util;

namespace Dfust.Hotkeys {

    public class HotKeyEventArgs {

        public HotKeyEventArgs(object sender, IList<Keys> keys, int count) {
            Sender = sender;
            Keys = keys;
            Count = count;
        }

        public HotKeyEventArgs(object sender, Keys key, int count) {
            Sender = sender;
            Keys = new List<Keys>(new[] { key });
            Count = count;
        }

        /// <summary>
        /// Returns a human readable description of the hotkey/chord.
        /// </summary>
        /// <value>The name of the chord.</value>
        public string ChordName { get { return Keys2String.ChordToString(Keys); } }

        /// <summary>
        /// Returns how often this hotkey was consecutively pressed. The first triggering is 1, every
        /// consecutive triggering increments by one.
        /// </summary>
        /// <value>The count.</value>
        public int Count { get; }

        /// <summary>
        /// Returns the Keys of the hotkey/chord.
        /// </summary>
        /// <value>The keys.</value>
        public IList<Keys> Keys { get; }

        /// <summary>
        /// Returns the sender of the KeyEventArgs that triggered the hotkey.
        /// </summary>
        /// <value>The sender.</value>
        public object Sender { get; }
    }
}