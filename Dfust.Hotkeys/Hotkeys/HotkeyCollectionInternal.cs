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
using System.Text;
using System.Windows.Forms;
using Dfust.Hotkeys.Util;

namespace Dfust.Hotkeys {

    internal class HotkeyCollectionInternal : IHotkeyCollection {

        //Hotkeys with a fixed sequence of key strokes. For example "ctrl+v", or "ctrl+alt+g, ctrl+alt+p"
        private readonly NestedDictionary<Keys, Dictionary<string, HotkeyAction>> m_registeredHotkeys = new NestedDictionary<Keys, Dictionary<string, HotkeyAction>>();

        private readonly Keys[] m_modifierKeys = { Keys.Control, Keys.Alt, Keys.Shift, Keys.LWin };
        private LimitedQueue<KeysState> m_keyBuffer;
        private Tuple<string, int> m_lastExecutedChord;

        /// <summary>
        /// Return all registered hotkeys.
        /// </summary>
        /// <returns></returns>
        public IEnumerable<Keys[]> GetHotkeys() {
            return m_registeredHotkeys.GetAllPaths();
        }

        /// <summary>
        /// Returns a human readable list of all registered hotkeys.
        /// </summary>
        /// <returns></returns>
        public string HotkeyDescription() {
            var sb = new StringBuilder();

            //Write Prefix
            sb.Append("Registered Hotkeys:");
            sb.Append(Environment.NewLine);

            //Sort hotkeys: First by length, then by alphabet
            var hotkeys = GetHotkeys().Select(chord => new { Name = Keys2String.ChordToString(chord), Chord = chord }).
                                       OrderBy(chord => chord.Chord.Count()).
                                       ThenBy(chord => chord.Name).
                                       ToArray();

            for (int i = 0; i < hotkeys.Count(); i++) {
                var hotkey = hotkeys[i];

                foreach (var xxx in m_registeredHotkeys.TryGetValue(hotkey.Chord).Item1) {
                    sb.Append("- ");
                    sb.Append(hotkey.Name);

                    var desc = xxx.Value.Description;

                    if (!string.IsNullOrWhiteSpace(desc)) {
                        sb.Append($" ({desc})");
                    }
                    if (i < hotkeys.Count() - 1) {
                        sb.Append(Environment.NewLine);
                    }
                }
            }

            return sb.ToString();
        }

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
        public void RegisterHotkey(Keys key, Action<HotKeyEventArgs> action, string actionDescription = null, bool handled = true) {
            RegisterHotkey(new[] { key }, action, actionDescription, handled);
        }

        /// <summary>
        /// Registers a chord with two or more hotkeys. "ctrl+a, ctrl+b" is an example.
        /// </summary>
        /// <param name="chord">The chord.</param>
        /// <param name="action">The function to call when hotkey is detected.</param>
        /// <param name="actionDescription">A description of action (optional).</param>
        /// <param name="handled">
        /// Sets whether the corresponding KeyEvent will be handled after a hotkey was recognized.
        /// </param>
        public void RegisterHotkey(IEnumerable<Keys> chord, Action<HotKeyEventArgs> action, string actionDescription = null, bool handled = true) {
            var keys = chord.ToArray();

            if (!m_registeredHotkeys.ContainsPath(keys)) {
                m_registeredHotkeys.Add(keys, new Dictionary<string, HotkeyAction>());
            }

            UpdateKeyBuffer();

            var funcs = m_registeredHotkeys.TryGetValue(keys);

            var hotkeyAction = new HotkeyAction(action, keys, handled) { Description = actionDescription };

            var registeredActions = funcs.Item1;

            var key = hotkeyAction.ChordName + actionDescription;

            if (!registeredActions.ContainsKey(key)) {
                registeredActions.Add(key, hotkeyAction);
            } else {
                throw new ArgumentException($"Two actions on the same chord have to differ in the description. chord: {hotkeyAction.ChordName}, description: '{ actionDescription }'.");
            }
        }

        /// <summary>
        /// Unregisters a chord.
        /// </summary>
        /// <param name="chord">The chord.</param>
        /// <param name="actionDescription">
        /// A description of the action (optional). If left empty, all actions of this hotkey will be removed.
        /// </param>
        public void UnregisterHotkey(IEnumerable<Keys> chord, string actionDescription = null) {
            var keys = chord.ToArray();

            var hotkeyActions = m_registeredHotkeys.TryGetValue(keys);
            if (hotkeyActions != null && hotkeyActions.Item1 != null) {
                var key = Keys2String.ChordToString(chord) + actionDescription;
                if (hotkeyActions.Item1.ContainsKey(key)) {
                    if (hotkeyActions.Item1.Count == 1) {
                        m_registeredHotkeys.Remove(keys);
                    } else {
                        hotkeyActions.Item1.Remove(key);
                    }
                } else {
                    if (actionDescription == null) {
                        m_registeredHotkeys.Remove(keys);
                    } else {
                        throw new ArgumentException($"None of the actions for the hotkey '{Keys2String.ChordToString(keys)}' does have the description '{actionDescription}'.");
                    }
                }
            }
        }

        /// <summary>
        /// Unregisters a hotkey.
        /// </summary>
        /// <param name="key">The hotkey.</param>
        /// <param name="actionDescription">
        /// A description of the action (optional). If left empty, all actions of this hotkey will be removed.
        /// </param>
        public void UnregisterHotkey(Keys key, string actionDescription = null) {
            UnregisterHotkey(new[] { key }, actionDescription);
        }

        /// <summary>
        /// Returns a <see cref="System.String"/> that represents this instance.
        /// </summary>
        /// <returns>A <see cref="System.String"/> that represents this instance.</returns>
        public override string ToString() {
            return HotkeyDescription();
        }

        /// <summary>
        /// Notifies the HotkeyCollection of a key down event.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="KeyEventArgs"/> instance containing the event data.</param>
        internal void OnKeyDown(object sender, KeyEventArgs e) {
            if (m_keyBuffer != null) {
                var state = GetCurrentKeysState();

                if (IsModifierKey(e)) {
                    //If we detect a new modifier, add it to the state
                    state.AddModifier(e.KeyData);
                } else {
                    //we detected a non modifier key

                    var modifiers = state.Modifiers.Aggregate(Keys.None, (a, b) => a | b);
                    state.AddKey((e.KeyCode | modifiers));
                    var currentPath = GetCurrentPath();

                    //search in the key stroke history for chords/hotkeys.
                    for (int i = currentPath.Count - 1; i >= 0; i--) {
                        //Start with the last key and one by one take more keys
                        var path = currentPath.Skip(i).ToList();

                        var actions = m_registeredHotkeys.TryGetValue(path).Item1;
                        if (actions != null) {
                            UpdateLastExecutedHotkey(path);

                            e.Handled = actions.Values.Select(a => a.Handled).Aggregate(false, (a, b) => a || b);

                            foreach (var action in actions.Values) {
                                action.Action(new HotKeyEventArgs(sender, path, count: m_lastExecutedChord.Item2));
                            }
                            //if we found a valid chord, clear the buffer...
                            m_keyBuffer.Clear();
                            //...and create a new state that contains the still active modifiers, if any
                            CreateAndEnqueueKeysState(state);

                            break;
                        }
                    }
                }
            }
        }

#pragma warning disable RECS0154 // Parameter is never used

        /// <summary>
        /// Notifies the HotkeyCollection of a key up event.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="KeyEventArgs"/> instance containing the event data.</param>
        internal void OnKeyUp(object sender, KeyEventArgs e) {
            var currentState = m_keyBuffer.Last(1);
            if (currentState != null && IsModifierKey(e)) {
                var keysState = currentState[0];
                keysState.RemoveModifier(e.KeyData);
            }
        }

#pragma warning restore RECS0154 // Parameter is never used

        private KeysState CreateAndEnqueueKeysState(KeysState previousState = null) {
            //If we have a previous state, then we need the active modifiers from that state in the new one.
            var state = previousState != null ? new KeysState(previousState) : new KeysState();
            m_keyBuffer.Enqueue(state);
            return state;
        }

        private KeysState GetCurrentKeysState() {
#pragma warning disable CC0013 // Use ternary operator
            //Get the last latest entry of the buffer
            var lastState = (m_keyBuffer.Last(1));
            if (lastState == null) {
                //no latest entry? Crate a new state!
                return CreateAndEnqueueKeysState();
            } else {
                //If we already added a "real" key (not a modifier), then we need a new state. If we only have added modifiers (or nothing) the lastState is fine
                return lastState[0].KeyAdded ? CreateAndEnqueueKeysState(lastState.First()) : lastState.First();
            }
#pragma warning restore CC0013 // Use ternary operator
        }

        private List<Keys> GetCurrentPath() {
            return m_keyBuffer.Select((s) => s.Key).ToList();
        }

        private bool IsModifierKey(KeyEventArgs e) {
            return m_modifierKeys.Contains(e.KeyData);
        }

        private void UpdateKeyBuffer() {
            var bufferSize = m_registeredHotkeys.LongestPathCount;
            m_keyBuffer = new LimitedQueue<KeysState>(bufferSize);
        }

        private void UpdateLastExecutedHotkey(List<Keys> path) {
#pragma warning disable CC0014 // Use ternary operator
            var chordName = Keys2String.ChordToString(path);
            if (m_lastExecutedChord?.Item1 == chordName) {
                m_lastExecutedChord = new Tuple<string, int>(chordName, m_lastExecutedChord.Item2 + 1);
            } else {
                m_lastExecutedChord = new Tuple<string, int>(chordName, 1);
            }
#pragma warning restore CC0014 // Use ternary operator
        }

        private class HotkeyAction {
            private readonly bool m_handled;

            /// <summary>
            /// A Description of the action
            /// </summary>
            public string Description;

            private readonly Action<HotKeyEventArgs> m_action;
            private readonly Keys[] m_chord;

            /// <summary>
            /// Initializes a new instance of the <see cref="HotkeyAction"/> class.
            /// </summary>
            /// <param name="action">The action to perform on hotkey.</param>
            /// <param name="chord">The chord.</param>
            public HotkeyAction(Action<HotKeyEventArgs> action, Keys[] chord, bool handled) {
                m_chord = chord;
                m_action = action;
                m_handled = handled;
            }

            /// <summary>
            /// Gets the action.
            /// </summary>
            /// <value>The action.</value>
            public Action<HotKeyEventArgs> Action {
                get {
                    return m_action;
                }
            }

            /// <summary>
            /// Gets the chord.
            /// </summary>
            /// <value>The chord.</value>
            public Keys[] Chord {
                get {
                    return m_chord;
                }
            }

            /// <summary>
            /// Gets a human readable textual interpretation of the chord.
            /// </summary>
            /// <value>The name of the chord.</value>
            public string ChordName {
                get { return Keys2String.ChordToString(m_chord); }
            }

            public bool Handled {
                get { return m_handled; }
            }
        }

        private class KeysState {
            private readonly Dictionary<Keys, int> m_modifiersDown;
            private Keys m_key;

            public KeysState(KeysState state) : this() {
                foreach (var kvp in state.m_modifiersDown) {
                    m_modifiersDown.Add(kvp.Key, kvp.Value);
                }
            }

            public KeysState() {
                m_key = Keys.None;
                m_modifiersDown = new Dictionary<Keys, int>();
            }

            public Keys Key { get { return m_key; } }

            public bool KeyAdded {
                get { return m_key != Keys.None; }
            }

            public IEnumerable<Keys> Modifiers { get { return m_modifiersDown.Keys; } }

            public void AddKey(Keys key) {
                m_key = key;
            }

            public void AddModifier(Keys key) {
                if (!m_modifiersDown.ContainsKey(key)) {
                    m_modifiersDown.Add(key, 1);
                } else {
                    var newValue = Math.Max(0, m_modifiersDown[key] += 1);
                    m_modifiersDown[key] = newValue;
                }
            }

            public void RemoveModifier(Keys key) {
                if (m_modifiersDown.ContainsKey(key)) {
                    m_modifiersDown[key] -= 1;
                    if (m_modifiersDown[key] <= 0) {
                        m_modifiersDown.Remove(key);
                    }
                }
            }
        }
    }
}