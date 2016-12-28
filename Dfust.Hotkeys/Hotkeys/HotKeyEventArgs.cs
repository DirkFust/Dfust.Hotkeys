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

        public HotKeyEventArgs(object sender,
                               IList<Keys> keys,
                               int count,
                               int countConsecutive,
                               int? countLastModifierEnvelope,
                               string description,
                               bool continuously = false,
                               bool followUp = false) {
            Sender = sender;
            Keys = keys;
            Count = count;
            //we can only be continuously if we are a follow up
            Continuously = continuously && followUp;
            FollowUp = followUp;
            Description = description;
            DirectlyConsecutiveCount = countConsecutive;
            LastModifierEnvelopeCount = countLastModifierEnvelope;
        }

        public HotKeyEventArgs(object sender,
                               Keys key,
                               int count,
                               int countConsecutive,
                               int? countLastModifierEnvelope,
                               string description,
                               bool continuously = false)
            : this(sender,
                   new List<Keys>(new[] { key }),
                   count,
                   countConsecutive,
                   countLastModifierEnvelope,
                   description,
                   continuously) {
        }

        /// <summary>
        /// Returns a human readable description of the hotkey/chord.
        /// </summary>
        /// <value>The name of the chord.</value>
        public string ChordName { get { return Keys2String.ChordToString(Keys); } }

        /// <summary>
        /// Returns whether this hotkey/chord was triggered continuously with the hotkey before it.
        ///
        /// A second hotkey is continuously with the first, if FollowUp is true and at least one
        /// modifier was never released between the two hotkeys
        /// </summary>
        /// <value><c>true</c> if continuously; otherwise, <c>false</c>.</value>
        public bool Continuously { get; }

        /// <summary>
        /// Returns how often this hotkey was triggered without any other hotkey in between. The
        /// first triggering is 1, every consecutive triggering increments by one. This count
        /// increases, if other (non hotkey) keys were pressed between the triggerings.
        /// </summary>
        /// <value>The count.</value>
        public int Count { get; }

        /// <summary>
        /// Return the description of the hotkey.
        /// </summary>
        /// <value>The description.</value>
        public string Description { get; }

        /// <summary>
        /// Returns how often this hotkey was triggered without any other hotkey or non-hotkey in
        /// between. The first triggering is 1, every consecutive triggering increments by one. This
        /// count does increase, if other (non hotkey) keys were pressed between the triggerings.
        /// </summary>
        /// <value>The count.</value>
        public int DirectlyConsecutiveCount { get; }

        /// <summary>
        /// Returns whether this hotkey/chord was a follow up to the hotkey before it.
        ///
        /// A hotkey is a follow up to another hotkey, if it is triggered directly after the first
        /// hotkey without any key presses in between that belong to neither of the two hotkeys.
        /// </summary>
        /// <value><c>true</c> if [follow up]; otherwise, <c>false</c>.</value>
        public bool FollowUp { get; }

        /// <summary>
        /// Returns the Keys of the hotkey/chord.
        /// </summary>
        /// <value>The keys.</value>
        public IList<Keys> Keys { get; }

        /// <summary>
        /// Returns how often this hotkey was triggered in a "modifier envelope", that means between
        /// the first press and the last release of hotkey's the modifiers. This value is null when
        /// at least one modifier key is still pressed. This changes with the
        /// <c>AllModifiersReleasedAfterHotkey</c> event after the hotkey was pressed, so this
        /// Property is only interesting for the <c>AllModifiersReleasedAfterHotkey</c>.
        /// </summary>
        /// <value>The count.</value>
        public int? LastModifierEnvelopeCount { get; }

        /// <summary>
        /// Returns the sender of the KeyEventArgs that triggered the hotkey.
        /// </summary>
        /// <value>The sender.</value>
        public object Sender { get; }
    }
}