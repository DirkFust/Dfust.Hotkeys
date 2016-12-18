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

using System.Linq;
using System.Windows.Forms;
using Dfust.Hotkeys;
using static Dfust.Hotkeys.Enums;

namespace Dfust.Hotkeys.TestGui {

    internal static class ExampleCode {

        public static void Example() {
            //create a new hotkey collection for the application.
            var hotkeyCollection = new HotkeyCollection(Scope.Application); //Scope.Global would tell the HotkeyCollection to listen for keystrokes globally

            //Now let's create a hotkey with a single key.

            //Define the key, in this case ctrl+G.
            const Keys hotkey = Keys.G | Keys.Control; //More modifiers could be added, for example "Keys.G | Keys.Control | Keys.Shift | Keys.Alt"

            //Register the hotkey. In this case triggering the hotkey will show a message box.
            hotkeyCollection.RegisterHotkey(hotkey, (e) => MessageBox.Show($"hello {e.Keys.First()}"));

            //Now let's crate a chord, a hotkey consisting of a sequence of keys

            //Define the chord. In this case we want to define "ctrl+k, ctrl+c", a chord that is used in Visual Studio to comment the selected text
            var chord = new Keys[] { Keys.K | Keys.Control, Keys.C | Keys.Control };

            //Register the chord. In this case triggering will call the OnChord function.
            hotkeyCollection.RegisterHotkey(hotkey, OnChordTriggered);

            //Finally: Dispose hotkey collection
            hotkeyCollection.Dispose();
        }

        /// <summary>
        /// Handles the "ctrl+k, ctrl+c" hotkey.
        /// </summary>
        /// <param name="e">The HotKeyEventArgs instance containing the event data.</param>
        private static void OnChordTriggered(HotKeyEventArgs e) {
            var keys = e.Keys; //which key (sequence) has triggered the hotkey?
            var count = e.Count; //How many times has this hotkey been triggered without any other triggered hotkeys in between?
            var sender = e.Sender; //Where did the KeyEventArg that was used in the HotkeyCollection originate from?

            //Do something here!
        }
    }
}