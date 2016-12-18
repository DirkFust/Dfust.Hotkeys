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
using Gma.System.MouseKeyHook;
using static Dfust.Hotkeys.Enums;

namespace Dfust.Hotkeys {

    /// <summary>
    /// This class listens for hotkeys and chords (sequences of multiple key presses that together
    /// form a hotkey)
    /// </summary>
    /// <seealso cref="System.IDisposable"/>
    public class HotkeyCollection : IDisposable, IHotkeyCollection {
        private readonly Scope m_scope;
        private KeyUpDownCleaner m_cleaner;
        private HotkeyCollectionInternal m_hotkeys;
        private IKeyboardMouseEvents m_keyboardHook;

        /// <summary>
        /// Initializes a new instance of the <see cref="HotkeyCollection"/> class and starts
        /// listening for hotkeys.
        /// </summary>
        /// <param name="scope">The scope to listen for hotkeys (global or application).</param>
        public HotkeyCollection(Scope scope) {
            m_hotkeys = new HotkeyCollectionInternal();
            m_cleaner = new KeyUpDownCleaner();
            m_scope = scope;
            Subscribe(scope);
        }

        /// <summary>
        /// Gets the scope of the hotkeys (global or application).
        /// </summary>
        /// <value>The scope.</value>
        public Scope Scope {
            get { return m_scope; }
        }

        /// <summary>
        /// Return all registered hotkeys as sequences of keystrokes.
        /// </summary>
        /// <returns></returns>
        public IEnumerable<Keys[]> GetHotkeys() {
            return m_hotkeys.GetHotkeys();
        }

        /// <summary>
        /// Returns a human readable list of all registered hotkeys.
        /// </summary>
        /// <returns></returns>
        public string HotkeyDescription() {
            return m_hotkeys.HotkeyDescription();
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
            m_hotkeys.RegisterHotkey(key, action, actionDescription, handled);
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
            m_hotkeys.RegisterHotkey(chord, action, actionDescription, handled);
        }

        /// <summary>
        /// Starts listening for hotkeys.
        /// </summary>
        public void StartListening() {
            if (m_keyboardHook == null) {
                Subscribe(Scope);
            }
        }

        /// <summary>
        /// Stops listening for hotkeys.
        /// </summary>
        public void StopListening() {
            if (m_keyboardHook != null) {
                Unsubscribe();
            }
        }

        /// <summary>
        /// Sets up the event subscriptions.
        /// </summary>
        /// <param name="scope">The scope.</param>
        private void Subscribe(Scope scope) {
            //Set scope: Global or application?
            m_keyboardHook = (scope == Scope.Application ? Hook.AppEvents() : Hook.GlobalEvents());

            //The keyboard hook feeds into the cleaner
            m_keyboardHook.KeyDown += m_cleaner.OnKeyDown;
            m_keyboardHook.KeyUp += m_cleaner.OnKeyUp;

            //The cleaner in turn feeds into the HotkeyCollectionInternal
            m_cleaner.KeyDown += m_hotkeys.OnKeyDown;
            m_cleaner.KeyUp += m_hotkeys.OnKeyUp;
        }

        /// <summary>
        /// Unsubscribes all events.
        /// </summary>
        private void Unsubscribe() {
            //Unsubscribe from keyboard hook
            m_keyboardHook.KeyDown -= m_cleaner.OnKeyDown;
            m_keyboardHook.KeyUp -= m_cleaner.OnKeyUp;

            //Dispose keyboard hook
            m_keyboardHook.Dispose();
            m_keyboardHook = null;

            //Unsubscribe from cleaner
            m_cleaner.KeyDown += m_hotkeys.OnKeyDown;
            m_cleaner.KeyUp += m_hotkeys.OnKeyUp;
        }

        #region IDisposable Support

        private bool disposedValue; // To detect redundant calls

        ~HotkeyCollection() {
            // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
            Dispose(false);
        }

        // This code added to correctly implement the disposable pattern.
        public void Dispose() {
            // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        public void UnregisterHotkey(Keys key, string actionDescription = null) {
            ((IHotkeyCollection)m_hotkeys).UnregisterHotkey(key, actionDescription);
        }

        public void UnregisterHotkey(IEnumerable<Keys> chord, string actionDescription = null) {
            ((IHotkeyCollection)m_hotkeys).UnregisterHotkey(chord, actionDescription);
        }

        protected virtual void Dispose(bool disposing) {
            if (!disposedValue) {
                if (disposing) {
                    Unsubscribe();
                }

                m_keyboardHook = null;
                m_hotkeys = null;
                m_cleaner = null;

                disposedValue = true;
            }
        }

        #endregion IDisposable Support
    }
}