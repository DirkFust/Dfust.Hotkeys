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
using System.Windows.Forms;

namespace Dfust.Hotkeys {

    /// <summary>
    /// Cleans repetitive Key Events, so that only one down event per Key fires
    /// </summary>
    public class KeyUpDownCleaner {

        //All known modifiers that are encoded in Keys ("WindowsKey" is not encoded in Keys!)
        private const Keys MODIFIERS = Keys.Alt | Keys.Control | Keys.Shift;

        private readonly Dictionary<Keys, HashSet<Keys>> m_keysDown = new Dictionary<Keys, HashSet<Keys>>();

        //This keys are keys that are not "normal".
        private readonly HashSet<Keys> m_specialKeys = new HashSet<Keys> { Keys.LControlKey,   //Control
                                                                           Keys.RControlKey,
                                                                           Keys.ControlKey,
                                                                           Keys.LWin,          //Win
                                                                           Keys.RWin,
                                                                           Keys.LShiftKey,     //Shift
                                                                           Keys.RShiftKey,
                                                                           Keys.ShiftKey,
                                                                           Keys.LMenu,         //Alt
                                                                           Keys.RMenu,
                                                                           Keys.Menu,
                                                                           Keys.None};         //No Key

        public delegate void KeyEventHandler(object sender, KeyEventArgs e);

        public event KeyEventHandler KeyDown;

        public event KeyEventHandler KeyUp;

        /// <summary>
        /// Notifies the KeyUpDownCleaner of a key up event.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="KeyEventArgs"/> instance containing the event data.</param>
        public void OnKeyDown(object sender, KeyEventArgs e) {
            //strip key from modifiers
            var rawKey = GetRawKey(e);
            if (rawKey != null) {
                if (!m_keysDown.ContainsKey(rawKey.ReplacementKey)) {
                    m_keysDown.Add(rawKey.ReplacementKey, new HashSet<Keys> { rawKey.Key });
                    KeyDown?.Invoke(sender, new KeyEventArgs(rawKey.ReplacementKey));
                } else {
                    m_keysDown[rawKey.ReplacementKey].Add(rawKey.Key);
                }
            }
        }

        /// <summary>
        /// Notifies the KeyUpDownCleaner of a key up event.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="KeyEventArgs"/> instance containing the event data.</param>
        public void OnKeyUp(object sender, KeyEventArgs e) {
            var rawKey = GetRawKey(e);
            if (rawKey != null) {
                var count = m_keysDown[rawKey.ReplacementKey].Count();
                if (count == 1) {
                    m_keysDown.Remove(rawKey.ReplacementKey);
                    KeyUp?.Invoke(sender, new KeyEventArgs(rawKey.ReplacementKey));
                } else {
                    m_keysDown[rawKey.ReplacementKey].Remove(rawKey.Key);
                }
            }
        }

        /// <summary>
        /// Gets the raw key without modifiers.
        /// </summary>
        /// <param name="e">The <see cref="KeyEventArgs"/> instance containing the event data.</param>
        /// <returns></returns>
        private KeysResult GetRawKey(KeyEventArgs e) {
            //strip key from modifiers
            var rawKey = (e.KeyCode | MODIFIERS) ^ MODIFIERS;

            if (m_specialKeys.Contains(rawKey)) {
                switch (rawKey) {
                    case Keys.ShiftKey:
                    case Keys.LShiftKey:
                    case Keys.RShiftKey:
                        return new KeysResult { ReplacementKey = Keys.Shift, Key = rawKey };

                    case Keys.ControlKey:
                    case Keys.LControlKey:
                    case Keys.RControlKey:
                        return new KeysResult { ReplacementKey = Keys.Control, Key = rawKey };

                    case Keys.Menu:
                    case Keys.LMenu:
                    case Keys.RMenu:
                        return new KeysResult { ReplacementKey = Keys.Alt, Key = rawKey };

                    case Keys.LWin:
                    case Keys.RWin:
                        return new KeysResult { ReplacementKey = Keys.LWin, Key = rawKey };

                    default:
                        return null;
                }
            }
            return new KeysResult { ReplacementKey = rawKey, Key = rawKey };
        }

        private class KeysResult {
            public Keys Key;
            public Keys ReplacementKey;
        }
    }
}