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
using System.Windows.Forms;
using static Dfust.Hotkeys.Enums;

namespace Dfust.Hotkeys.TestGui {

    internal static class ExampleCode {

        public static void EventsExample() {
            //create a new hotkey collection for the application.
            var hotkeyCollection = new HotkeyCollection(Scope.Application); //Scope.Global would tell the HotkeyCollection to listen for keystrokes globally

            //Let's create a hotkey with a single key.

            //Define the key, in this case ctrl+G.
            const Keys hotkey = Keys.G | Keys.Control; //More modifiers could be added, for example "Keys.G | Keys.Control | Keys.Shift | Keys.Alt"

            //Register the hotkey. Triggering the hotkey will not call any action itself, because we passed null as action.
            hotkeyCollection.RegisterHotkey(hotkey, null, handled: true);

            //Define the chord. In this case we want to define "ctrl+k, ctrl+c", a chord that is used in Visual Studio to comment the selected text
            var chord = new Keys[] { Keys.K | Keys.Control, Keys.C | Keys.Control };

            //Register the chord without an action
            hotkeyCollection.RegisterHotkey(chord, null, handled: true);

            //Register to the event that tells us whenever a hotkey was triggered. Show the hotkey name in the console.
            hotkeyCollection.HotkeyTriggered += e => Console.WriteLine($"HotkeyTriggered: {e.ChordName}");

            //Register to the event that tells us when the beginning of a chord was recognized.
            hotkeyCollection.ChordStartRecognized += e => Console.WriteLine($"ChordStartRecognized: ({e.ChordSubpath}) was pressed. Waiting for the next key in the chord.");

            //Register to the event that tells us when we released the last modifier key was released after one or more hotkeys were triggered.
            //This is useful when you want to react to the number of times a hotkey was pressed. If you want to count the hotkey "Control+J" and react differently
            //to the number of times it was pressed you have to wait until the last of those hotkeys is done.
            hotkeyCollection.AllModifiersReleasedAfterHotkey += e => Console.WriteLine("AllModifiersReleasedAfterHotkey: " + string.Join("; ", Enumerable.Repeat($"{e.ChordName} ", e.DirectlyConsecutiveCount)) + $"(x {e.DirectlyConsecutiveCount})");

            //Finally: Dispose hotkey collection
            hotkeyCollection.Dispose();
        }

        /// <summary>
        /// Example code for working with the HotkeyCollection class.
        /// </summary>
        public static void Example() {
            //create a new hotkey collection for the application.
            var hotkeyCollection = new HotkeyCollection(Scope.Application); //Scope.Global would tell the HotkeyCollection to listen for keystrokes globally

            //Let's create a hotkey with a single key.

            //Define the key, in this case ctrl+G.
            const Keys hotkey = Keys.G | Keys.Control; //More modifiers could be added, for example "Keys.G | Keys.Control | Keys.Shift | Keys.Alt"

            //Register the hotkey. In this case triggering the hotkey will write to console.
            hotkeyCollection.RegisterHotkey(hotkey, (e) => Console.WriteLine($"hello {e.Keys.First()}"));

            //Now let's crate a chord, a hotkey consisting of a sequence of keys

            //Define the chord. In this case we want to define "ctrl+k, ctrl+c", a chord that is used in Visual Studio to comment the selected text
            var chord = new Keys[] { Keys.K | Keys.Control, Keys.C | Keys.Control };

            //Register the chord. In this case triggering will call the OnChord function.
            hotkeyCollection.RegisterHotkey(chord, OnChordTriggered);

            //Finally: Dispose hotkey collection
            hotkeyCollection.Dispose();
        }

        /// <summary>
        /// Handles the "ctrl+k, ctrl+c" hotkey.
        /// </summary>
        /// <param name="e">The HotKeyEventArgs instance containing the event data.</param>
        private static void OnChordTriggered(HotKeyEventArgs e) {
            var keys = e.Keys; //Which key (sequence) has triggered the hotkey?
            var count = e.Count; //How many times has this hotkey been triggered without any other triggered hotkeys in between? Other (non-hotkey) keys don't reset the counter
            var counterConsecutive = e.DirectlyConsecutiveCount; //How many times has this hotkey been triggered without any other triggered hotkeys in between? Other (non-hotkey) keys do reset the counter
            var sender = e.Sender; //Where did the KeyEventArg that was used in the HotkeyCollection originate from?
            var isFollowUp = e.FollowUp; //Is the triggered hotkey a followUp of another hotkey/chord? This is the case when no keys were pressed that do not belong to any hotkey/chord.
            var isContinuously = e.Continuously; //A hotkey/chord is continuously, if it is the followUp of another, and if at least one modifier was not released between the two.
            var description = e.Description; //Returns the description that was given when registering the hotkey/chord
            var name = e.ChordName; //Returns a string representation of the hotkey/chord (e.g. "Control+Alt+a, Shift+c")

            Console.WriteLine($"OnChordTriggered:  {e.ChordName}");
        }
    }
}