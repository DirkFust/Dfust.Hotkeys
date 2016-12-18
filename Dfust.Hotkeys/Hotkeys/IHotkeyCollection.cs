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
using System.Windows.Forms;

namespace Dfust.Hotkeys {

    public interface IHotkeyCollection {

        /// <summary>
        /// Return all registered hotkeys as sequences of keystrokes.
        /// </summary>
        /// <returns></returns>
        IEnumerable<Keys[]> GetHotkeys();

        /// <summary>
        /// Returns a human readable list of all registered hotkeys.
        /// </summary>
        /// <returns></returns>
        string HotkeyDescription();

        /// <summary>
        /// Registers a hotkey with one key (and possibly some modifiers). "ctrl+f" is an example for
        /// a hotkey, "alt+shift+c" is another.
        /// </summary>
        /// <param name="key">The hotkey.</param>
        /// <param name="action">The function to call when hotkey is detected.</param>
        /// <param name="actionDescription">A description of action (optional).</param>
        /// <param name="handled">
        /// Sets whether the corresponding KeyEvent will be handled after a hotkey was recognized.
        /// </param>
        void RegisterHotkey(Keys key, Action<HotKeyEventArgs> action, string actionDescription = null, bool handled = true);

        /// <summary>
        /// Registers a chord with two or more hotkeys. "ctrl+a, ctrl+b" is an example.
        /// </summary>
        /// <param name="chord">The chord.</param>
        /// <param name="action">The function to call when hotkey is detected.</param>
        /// <param name="actionDescription">A description of action (optional).</param>
        /// <param name="handled">
        /// Sets whether the corresponding KeyEvent will be handled after a hotkey was recognized.
        /// </param>
        void RegisterHotkey(IEnumerable<Keys> chord, Action<HotKeyEventArgs> action, string actionDescription = null, bool handled = true);
    }
}