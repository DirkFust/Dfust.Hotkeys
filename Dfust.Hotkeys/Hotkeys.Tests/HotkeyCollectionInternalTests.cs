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
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using Dfust.Hotkeys.Util;
using NUnit.Framework;

namespace Dfust.Hotkeys.Tests {

    [TestFixture, ExcludeFromCodeCoverage]
    public class HotkeyCollectionInternalTests {
        private const string loremIpsum = "Lorem ipsum dolor sit amet, consetetur sadipscing elitr, sed diam nonumy eirmod tempor invidunt ut labore et dolore magna aliquyam erat, sed diam voluptua. At vero eos et accusam et justo duo dolores et ea rebum. Stet clita kasd gubergren";

        [Test]
        public void HotkeyCollectionInternal_HotkeyCollectionShouldBeInstantiable() {
            Assert.DoesNotThrow(() => new HotkeyCollectionInternal());
        }

        [Test]
        public void HotkeyCollectionInternal_HotkeyCollectionShouldNotFireOnWrongModifier() {
            //--- Assemble
            var counter = 0;
            var hc = new HotkeyCollectionInternal();
            const Keys key = Keys.A;

            hc.RegisterHotkey(key, e => counter++);

            var modifiers = new Keys[]
            {
                 Keys.Control,
                 Keys.Shift ,
                 Keys.Alt     ,
                 Keys.LWin
            };

            //--- Act / Assert
            foreach (var item in modifiers) {
                Assert.That(counter, Is.EqualTo(0));

                //Modifier key down
                hc.OnKeyDown(sender: null, e: new KeyEventArgs(item));
                Assert.That(counter, Is.EqualTo(0));

                //A+modifier key down
                hc.OnKeyDown(sender: null, e: new KeyEventArgs(key));
                Assert.That(counter, Is.EqualTo(0), $"key={item}");

                //A+modifier key up
                hc.OnKeyUp(sender: null, e: new KeyEventArgs(key));
                Assert.That(counter, Is.EqualTo(0));

                //Modifier key up
                hc.OnKeyUp(sender: null, e: new KeyEventArgs(item));
                Assert.That(counter, Is.EqualTo(0));
            }
        }

        [Test]
        public void HotkeyCollectionInternal_HotkeyCollectionShouldNotThrowWhenTryingToUnregisterNotExistingHotkey() {
            //--- Assemble
            var hc = new HotkeyCollectionInternal();

            const Keys key = Keys.A;

            //--- Act /Assert

            //Unregister not existing hotkey with default value for actionDescription
            Assert.DoesNotThrow(() => hc.UnregisterHotkey(key));
            //Unregister not existing hotkey with explicitly provided default value for actionDescription
            Assert.DoesNotThrow(() => hc.UnregisterHotkey(key, actionDescription: null));
            //Unregister not existing hotkey with explicitly provided non-default value for actionDescription
            Assert.DoesNotThrow(() => hc.UnregisterHotkey(key, actionDescription: "aaaaaaa"));
        }

        [Test]
        public void HotkeyCollectionInternal_HotkeyCollectionShouldRegisterSimpleHotkeyAndFireWhenDetected() {
            //--- Assemble
            var counter = 0;
            var hc = new HotkeyCollectionInternal();
            const Keys key = Keys.A;

            //--- Act
            hc.RegisterHotkey(key, e => counter++);

            Assert.That(counter, Is.EqualTo(0));

            //---Assert

            //Try twice, so we see we can chain hotkeys
            for (int i = 0; i < 2; i++) {
                counter = 0;

                hc.OnKeyDown(sender: null, e: new KeyEventArgs(Keys.A));
                Assert.That(counter, Is.EqualTo(1));

                hc.OnKeyUp(sender: null, e: new KeyEventArgs(Keys.A));
                Assert.That(counter, Is.EqualTo(1));
            }
        }

        [Test]
        public void HotkeyCollectionInternal_HotkeyCollectionShouldThrowWhenTryingToUnregisterHotkeysWithNonExistingActionDescription() {
            //--- Assemble
            var counter = 0;
            var counter1 = 0;
            var counter2 = 0;

            var hc = new HotkeyCollectionInternal();
            const Keys key = Keys.A;

            hc.RegisterHotkey(key, e => { counter++; counter1++; }, "action1");
            hc.RegisterHotkey(key, e => { counter++; counter2++; }, "action2");

            //trigger hotkey
            hc.OnKeyDown(sender: null, e: new KeyEventArgs(Keys.A));
            hc.OnKeyUp(sender: null, e: new KeyEventArgs(Keys.A));
            Assert.That(counter, Is.EqualTo(2));
            Assert.That(counter1, Is.EqualTo(1));
            Assert.That(counter2, Is.EqualTo(1));
            Assert.IsTrue(hc.GetHotkeys().First().First() == key);

            //--- Act / Assert

            //we try to unregister with a description that does not exist for this hotkey
            Assert.Throws<ArgumentException>(() => hc.UnregisterHotkey(key, "xxxxxxx"));
        }

        [Test]
        public void HotkeyCollectionInternal_HotkeyCollectionShouldUnregisterHotkey() {
            //--- Assemble
            var counter = 0;
            var hc = new HotkeyCollectionInternal();
            const Keys key = Keys.A;

            hc.RegisterHotkey(key, e => counter++);

            //trigger hotkey
            Assert.That(counter, Is.EqualTo(0));
            hc.OnKeyDown(sender: null, e: new KeyEventArgs(Keys.A));
            hc.OnKeyUp(sender: null, e: new KeyEventArgs(Keys.A));
            Assert.That(counter, Is.EqualTo(1));
            Assert.IsTrue(hc.GetHotkeys().First().First() == key);

            //--- Act

            hc.UnregisterHotkey(key);

            //---Assert

            //same key that triggered the hotkey does not trigger it again
            hc.OnKeyDown(sender: null, e: new KeyEventArgs(Keys.A));
            hc.OnKeyUp(sender: null, e: new KeyEventArgs(Keys.A));
            Assert.That(counter, Is.EqualTo(1));
            Assert.IsFalse(hc.GetHotkeys().Any());
        }

        [Test]
        public void HotkeyCollectionInternal_HotkeyCollectionShouldUnregisterHotkeyAndActionSpecifiedByDescription() {
            //--- Assemble
            var counter = 0;
            var counter1 = 0;
            var counter2 = 0;

            var hc = new HotkeyCollectionInternal();
            const Keys key = Keys.A;

            const string description1 = "action1";
            hc.RegisterHotkey(key, e => { counter++; counter1++; }, description1);
            hc.RegisterHotkey(key, e => { counter++; counter2++; }, "action2");

            //trigger hotkey
            hc.OnKeyDown(sender: null, e: new KeyEventArgs(Keys.A));
            hc.OnKeyUp(sender: null, e: new KeyEventArgs(Keys.A));
            Assert.That(counter, Is.EqualTo(2));
            Assert.That(counter1, Is.EqualTo(1));
            Assert.That(counter2, Is.EqualTo(1));
            Assert.IsTrue(hc.GetHotkeys().First().First() == key);

            //--- Act

            hc.UnregisterHotkey(key, description1);

            //---Assert

            //same key that triggered the hotkey does not trigger it again
            hc.OnKeyDown(sender: null, e: new KeyEventArgs(Keys.A));
            hc.OnKeyUp(sender: null, e: new KeyEventArgs(Keys.A));
            Assert.That(counter, Is.EqualTo(3));
            Assert.That(counter1, Is.EqualTo(1));
            Assert.That(counter2, Is.EqualTo(2));
            Assert.IsTrue(hc.GetHotkeys().First().First() == key);
        }

        [Test]
        public void HotkeyCollectionInternal_HotkeyCollectionShouldUnregisterHotkeyAndAllActionsWhenNoDescriptionGiven() {
            //--- Assemble
            var counter = 0;
            var counter1 = 0;
            var counter2 = 0;

            var hc = new HotkeyCollectionInternal();
            const Keys key = Keys.A;

            hc.RegisterHotkey(key, e => { counter++; counter1++; }, "action1");
            hc.RegisterHotkey(key, e => { counter++; counter2++; }, "action2");

            //trigger hotkey
            hc.OnKeyDown(sender: null, e: new KeyEventArgs(Keys.A));
            hc.OnKeyUp(sender: null, e: new KeyEventArgs(Keys.A));
            Assert.That(counter, Is.EqualTo(2));
            Assert.That(counter1, Is.EqualTo(1));
            Assert.That(counter2, Is.EqualTo(1));
            Assert.IsTrue(hc.GetHotkeys().First().First() == key);

            //--- Act

            //unregister with no actionDescription given
            hc.UnregisterHotkey(key);

            //---Assert

            //same key that triggered the hotkey does not trigger it again
            hc.OnKeyDown(sender: null, e: new KeyEventArgs(Keys.A));
            hc.OnKeyUp(sender: null, e: new KeyEventArgs(Keys.A));
            Assert.That(counter, Is.EqualTo(2));
            Assert.That(counter1, Is.EqualTo(1));
            Assert.That(counter2, Is.EqualTo(1));
            Assert.IsFalse(hc.GetHotkeys().Any());
        }

        [Test]
        public void HotkeyCollectionInternal_ShouldBeAbleToChangeTheHandledPropertyPerHotkey() {
            //--- Assemble
            var counter = 0;
            var hc = new HotkeyCollectionInternal();

            hc.RegisterHotkey(Keys.C | Keys.Control, eventArgs => counter++, handled: false);
            hc.RegisterHotkey(Keys.V | Keys.Control, eventArgs => counter++, handled: true);

            //--- Act

            //handled on non-hotkey key (V without control) stays false
            var e = new KeyEventArgs(Keys.V);
            Assert.IsFalse(e.Handled);
            hc.OnKeyDown(null, e);
            Assert.IsFalse(e.Handled);

            Assert.That(counter, Is.EqualTo(0));

            //handled on modifier stays false
            e = new KeyEventArgs(Keys.Control);
            Assert.IsFalse(e.Handled);
            hc.OnKeyDown(null, e);
            Assert.IsFalse(e.Handled);
            Assert.That(counter, Is.EqualTo(0));

            //handled on ctrl+C stays false (we said so during registration)
            e = new KeyEventArgs(Keys.C);
            Assert.IsFalse(e.Handled);
            hc.OnKeyDown(null, e);
            Assert.IsFalse(e.Handled);
            Assert.That(counter, Is.EqualTo(1));

            //handled on ctrl+V toggles (we said so during registration)
            e = new KeyEventArgs(Keys.V);
            Assert.IsFalse(e.Handled);
            hc.OnKeyDown(null, e);
            Assert.That(counter, Is.EqualTo(2));
            Assert.IsTrue(e.Handled);
        }

        [Test]
        public void HotkeyCollectionInternal_ShouldCountNumberOfConsecutivelyTriggersOfSameHotkey([Range(1, 10)] int pressCount, [Values(1, 5, 10)]int chordLength) {
            //--- Assemble
            var count = 1;
            var hc = new HotkeyCollectionInternal();

            //Starting from Keys.G because why not?
            var hotkeys = Enumerable.Range(71, chordLength).Select(i => (Keys)i).ToList();

            var chord = hotkeys.Select(key => key | Keys.Control);

            //Assert is hidden in the function to call on hotkey
            hc.RegisterHotkey(chord, e => Assert.That(e.Count, Is.EqualTo(count)));
            //--- Act

            for (int i = 1; i <= pressCount; i++) {
                hc.OnKeyDown(null, new KeyEventArgs(Keys.Control));

                foreach (var key in hotkeys) {
                    hc.OnKeyDown(null, new KeyEventArgs(key));
                    hc.OnKeyUp(null, new KeyEventArgs(key));
                }

                hc.OnKeyUp(null, new KeyEventArgs(Keys.Control));
                count++;

                //Type lorem ipsum
                foreach (var c in loremIpsum) {
                    hc.OnKeyDown(null, new KeyEventArgs(Char2Keys(c)));
                    hc.OnKeyUp(null, new KeyEventArgs(Char2Keys(c)));
                }
            }
        }

        /// <summary>
        /// This test takes creates chords of a given length. It creates all possible permutations of
        /// active/inactive modifiers on each of the key in the chord. It will create
        /// </summary>
        /// <param name="chordLength">Length of the chord.</param>
        [Test]
        public void HotkeyCollectionInternal_ShouldDetectChordOfDifferentKeysInAnyPossibleCombinationOfKeystrokes([Values(1, 2, 3, 4)] int chordLength) {
            //--- Assemble
            var counter = 0;
            //The first chordLength+2 letters of the alphabet. +2 so we have some letters that are not used in the chord
            var allKeys = Enumerable.Range(65, chordLength + 2).Select(x => (Keys)x).ToArray();
            //Take chordLength-many  keys. We will use these to generate our chords
            var usedKeys = allKeys.Take(chordLength).ToList();
            var unusedKeys = allKeys.Skip(chordLength).ToList();

            //These are the possible modifiers
            var modifiers = new Keys[] { Keys.Control, Keys.Shift, Keys.Alt }; // TODO:, Keys.LWin };
            //Create the power set of all possible combinations of modifiers
            var allPossibleModifiers = SetOperations.GetPowerset(modifiers);

            var chords = new List<List<Keys>>();

            for (int i = 0; i < chordLength; i++) {
                var key = usedKeys[i];
                var allKeyModifierPermutations = allPossibleModifiers.Select(x => x.Aggregate(Keys.None, (a, b) => a | b) | key);

                chords.Add(allKeyModifierPermutations.ToList());
            }

            //Create all possible combinations of hotkeys derived from the given key and the given modifiers
            var allPossibleHotkeys = SetOperations.CartesianProduct(chords);

            //--- Act

            //Try every chord
            foreach (var chord in allPossibleHotkeys) {
                counter = 0;
                var hc = new HotkeyCollectionInternal();
                hc.RegisterHotkey(chord, e => counter++);

                var keySequence = new StringBuilder();

                for (int round = 1; round < 4; round++) {
                    //create all sequences of key presses that should trigger the chord
                    for (int i = 0; i < chordLength; i++) {
                        var currentKey = chord[i];

                        //Get all modifiers on current key
                        var activeModifiers = GetAllActiveModifiers(currentKey, modifiers);

                        //Create a random sequence of modifiers to press the modifiers key...
                        var shuffledModifiersKeyDown = SetOperations.Shuffle(activeModifiers);
                        //... and another random sequence to release them
                        var shuffledModifiersKeyUp = SetOperations.Shuffle(activeModifiers);

                        //Get the key without modifiers
                        var rawKey = GetRawKey(currentKey, activeModifiers);

                        //press the modifier keys...
                        foreach (var modifier in shuffledModifiersKeyDown) {
                            hc.OnKeyDown(null, new KeyEventArgs(modifier));
                            keySequence.Append($"↓{modifier}");
                        }

                        keySequence.Append($" ");
                        //press the chord key
                        hc.OnKeyDown(null, new KeyEventArgs(rawKey));
                        keySequence.Append($"↓{rawKey}");
                        hc.OnKeyUp(null, new KeyEventArgs(rawKey));
                        keySequence.Append($"↑{rawKey}");
                        keySequence.Append($" ");

                        //release the modifier keys...
                        foreach (var modifier in shuffledModifiersKeyUp) {
                            hc.OnKeyUp(null, new KeyEventArgs(modifier));
                            keySequence.Append($"↑{modifier}");
                        }
                        keySequence.Append($"    ");
                    }
                    //---Assert
                    Assert.That(counter, Is.EqualTo(round), $"chord: {Keys2String.ChordToString(chord)}, keySequence: {keySequence}, round {round}");
                    keySequence.Append($"  ->  ");
                }
            }
        }

        [Test]
        public void HotkeyCollectionInternal_ShouldFireEventWhenStartOfChordIsRecognized() {
            //--- Assemble
            var counterSubpath = 0;
            var counterTrigger = 0;

            var hc = new HotkeyCollectionInternal();
            hc.ChordStartRecognized += (e => counterSubpath++);

            hc.RegisterHotkey(Keys.A, e => counterTrigger++);
            hc.RegisterHotkey(new Keys[] { Keys.A, Keys.B }, e => counterTrigger++);
            hc.RegisterHotkey(new Keys[] { Keys.C, Keys.D, Keys.E }, e => counterTrigger++);
            hc.RegisterHotkey(new Keys[] { Keys.C, Keys.D, Keys.X }, e => counterTrigger++);

            //--- Act

            //Trigger the hotkey "A". Since it is complete with only one key, we have had no subpath
            hc.OnKeyDown(null, Keys.A);
            Assert.That(counterSubpath, Is.EqualTo(0));
            Assert.That(counterTrigger, Is.EqualTo(1));

            //Type "B". "A,B" would be a chord, but since "A" alone is a complete hotkey, we can never reach "A,B". So nothing happens with our counters
            hc.OnKeyDown(null, Keys.B);
            Assert.That(counterSubpath, Is.EqualTo(0));
            Assert.That(counterTrigger, Is.EqualTo(1));

            //Type "C". This is the first letter of two chords, so the event should fire
            hc.OnKeyDown(null, Keys.C);
            Assert.That(counterSubpath, Is.EqualTo(1));
            Assert.That(counterTrigger, Is.EqualTo(1));

            //Type "D". This is the second letter of two chords, so the event should fire again
            hc.OnKeyDown(null, Keys.D);
            Assert.That(counterSubpath, Is.EqualTo(2));
            Assert.That(counterTrigger, Is.EqualTo(1));

            //Type "X". This is the last letter of a chord, so the chord should trigger
            hc.OnKeyDown(null, Keys.X);
            Assert.That(counterSubpath, Is.EqualTo(2));
            Assert.That(counterTrigger, Is.EqualTo(2));

            //Type "D". This is the last letter of a chord, but we just triggered the other chord with the same beginning, so this is just a stray letter...
            hc.OnKeyDown(null, Keys.D);
            Assert.That(counterSubpath, Is.EqualTo(2));
            Assert.That(counterTrigger, Is.EqualTo(2));

            //Type "C". This is the first letter of two chords, so the event should fire
            hc.OnKeyDown(null, Keys.C);
            Assert.That(counterSubpath, Is.EqualTo(3));
            Assert.That(counterTrigger, Is.EqualTo(2));

            //Type "D". This is the second letter of two chords, so the event should fire again
            hc.OnKeyDown(null, Keys.D);
            Assert.That(counterSubpath, Is.EqualTo(4));
            Assert.That(counterTrigger, Is.EqualTo(2));

            //Type "E". This is the last letter of a chord, so the chord should trigger
            hc.OnKeyDown(null, Keys.X);
            Assert.That(counterSubpath, Is.EqualTo(4));
            Assert.That(counterTrigger, Is.EqualTo(3));

            //Type "K". This letter is not in any hotkey, so nothing happens
            hc.OnKeyDown(null, Keys.K);
            Assert.That(counterSubpath, Is.EqualTo(4));
            Assert.That(counterTrigger, Is.EqualTo(3));
        }

        [Test]
        public void HotkeyCollectionInternal_ShouldFireSameChordMultipleTimes([Values(1, 2, 3, 4, 5)] int chordLength, [Values(true, false)]bool useOnlyOneKeyForChords) {
            //--- Assemble
            var counter = 0;

            //These are the possible modifiers
            var modifiers = new Keys[] { Keys.Control, Keys.Shift, Keys.Alt }; // TODO:, Keys.LWin };
            //Create the power set of all possible combinations of modifiers
            var allPossibleModifiers = SetOperations.GetPowerset(modifiers);

            //how many times do we want to consecutively trigger the chord?
            for (int i = 0; i < 10; i++) {
                foreach (var modifier in allPossibleModifiers) {
                    counter = 0;
                    var mods = modifier.Aggregate(Keys.None, (a, b) => a | b);

                    //The keys Z,X,W,H,F do not appear in lorem ipsum, so writing it does not trigger the chords...
                    var possibleHotkeys = new Keys[] { Keys.Z, Keys.X, Keys.W, Keys.H, Keys.F };

                    var chord = Enumerable.Repeat(possibleHotkeys[0] | mods, chordLength);
                    if (!useOnlyOneKeyForChords) {
                        chord = possibleHotkeys.Take(chordLength).Select(k => k | mods);
                    }

                    var hc = new HotkeyCollectionInternal();
                    hc.RegisterHotkey(chord, e => counter++);

                    //Type lorem ipsum
                    foreach (var c in loremIpsum) {
                        hc.OnKeyDown(null, new KeyEventArgs(Char2Keys(c)));
                        hc.OnKeyUp(null, new KeyEventArgs(Char2Keys(c)));
                    }

                    for (int j = 0; j <= i; j++) {
                        //press modifier keys
                        foreach (var mod in modifier) {
                            hc.OnKeyDown(null, new KeyEventArgs(mod));
                        }

                        //press chord keys
                        foreach (var key in chord) {
                            hc.OnKeyDown(null, new KeyEventArgs(key));
                            hc.OnKeyUp(null, new KeyEventArgs(key));
                        }

                        //release modifier keys
                        foreach (var mod in modifier) {
                            hc.OnKeyUp(null, new KeyEventArgs(mod));
                        }
                    }
                    //---Assert
                    Assert.That(counter, Is.EqualTo(i + 1), $"chord: {Keys2String.ChordToString(chord)}, i: {i}");
                }
            }
        }

        [Test]
        public void HotkeyCollectionInternal_ShouldHaveCountOf1WhenTriggeringOtherHotkeyBetweenTwoTimesSameHotkey() {
            //--- Assemble

            var hc = new HotkeyCollectionInternal();

            const Keys hotkey1 = Keys.G;
            const Keys hotkey2 = Keys.P;

            //Assert is hidden in the function to call on hotkey
            hc.RegisterHotkey(hotkey1 | Keys.Control, e => Assert.That(e.Count, Is.EqualTo(1)));
            hc.RegisterHotkey(hotkey2 | Keys.Control, e => Assert.That(e.Count, Is.EqualTo(1)));
            //--- Act

            //we trigger hotkey1 and expect a count of 1 (see Assert when registering hotkey)
            hc.OnKeyDown(null, new KeyEventArgs(Keys.Control));
            hc.OnKeyDown(null, new KeyEventArgs(hotkey1));
            hc.OnKeyUp(null, new KeyEventArgs(hotkey1));
            hc.OnKeyUp(null, new KeyEventArgs(Keys.Control));

            //we trigger another hotkey (hotkey2) and expect a count of 1, because it was triggered the first time consecutively (see Assert when registering hotkey)
            hc.OnKeyDown(null, new KeyEventArgs(Keys.Control));
            hc.OnKeyDown(null, new KeyEventArgs(hotkey2));
            hc.OnKeyUp(null, new KeyEventArgs(hotkey2));
            hc.OnKeyUp(null, new KeyEventArgs(Keys.Control));

            //we trigger hotkey1 again and expect a count of 1 again, because it was not triggered directly after it's first triggering
            hc.OnKeyDown(null, new KeyEventArgs(Keys.Control));
            hc.OnKeyDown(null, new KeyEventArgs(hotkey1));
            hc.OnKeyUp(null, new KeyEventArgs(hotkey1));
            hc.OnKeyUp(null, new KeyEventArgs(Keys.Control));
        }

        [Test]
        public void HotkeyCollectionInternal_ShouldListAllRegisteredHotkeys() {
            //--- Assemble
            var counter = 0;
            var hc = new HotkeyCollectionInternal();

            const Keys shift_A = Keys.A | Keys.Shift;
            const Keys shift_B = Keys.B | Keys.Shift;
            const Keys ctrl_A = Keys.A | Keys.Control;
            const Keys alt_B = Keys.B | Keys.Alt;
            const Keys ctrl_Alt_A = Keys.A | Keys.Alt | Keys.Control;
            const Keys a = Keys.A;

            var chord1 = new Keys[] { shift_B, shift_B };
            var chord2 = new Keys[] { ctrl_A, alt_B };
            var chord3 = new Keys[] { ctrl_A, ctrl_Alt_A };

            hc.RegisterHotkey(shift_A, e => counter++);
            hc.RegisterHotkey(a, e => counter++);
            hc.RegisterHotkey(chord1, e => counter++);
            hc.RegisterHotkey(chord2, e => counter++);
            hc.RegisterHotkey(chord3, e => counter++);

            //--- Act

            var hotkeys = hc.GetHotkeys();

            //---Assert
            Assert.That(hotkeys.Count(), Is.EqualTo(5));
        }

        [Test]
        public void HotkeyCollectionInternal_ShouldNotFireWhenReleasingAllModifiersBetweenChord() {
            //--- Assemble
            var counter = 0;
            var hc = new HotkeyCollectionInternal();

            var modifiers = new Keys[]
            {
                 Keys.Control,
                 Keys.Shift ,
                 Keys.Alt     ,
                 Keys.LWin
            };

            foreach (var modifier in modifiers) {
                var key1 = Keys.A | modifier;
                var key2 = Keys.B | modifier;

                var chord = new Keys[] { key1, key2 };
                //--- Act
                hc.RegisterHotkey(chord, e => counter++);

                //---Assert

                //Press modifier and A and release both. No hotkey should trigger.
                Assert.That(counter, Is.EqualTo(0));
                hc.OnKeyDown(sender: null, e: new KeyEventArgs(modifier));
                Assert.That(counter, Is.EqualTo(0));
                hc.OnKeyDown(sender: null, e: new KeyEventArgs(Keys.A));
                Assert.That(counter, Is.EqualTo(0));
                hc.OnKeyUp(sender: null, e: new KeyEventArgs(modifier));
                Assert.That(counter, Is.EqualTo(0));

                //Press modifier and B and release both.  No hotkey should trigger.
                hc.OnKeyDown(sender: null, e: new KeyEventArgs(modifier));
                Assert.That(counter, Is.EqualTo(0));
                hc.OnKeyUp(sender: null, e: new KeyEventArgs(Keys.B));
                Assert.That(counter, Is.EqualTo(0));
                hc.OnKeyUp(sender: null, e: new KeyEventArgs(modifier));
                Assert.That(counter, Is.EqualTo(0));
            }
        }

        [Test]
        public void HotkeyCollectionInternal_ShouldRegisterComplexHotkeyWithMultipleKeyStrokesAndFire1() {
            //--- Assemble
            var counter = 0;
            var hc = new HotkeyCollectionInternal();
            var modifiers = new Keys[]
            {
                 Keys.Control,
                 Keys.Shift ,
                 Keys.Alt     ,
                 Keys.LWin
            };

            foreach (var modifier in modifiers) {
                var key1 = Keys.A | modifier;
                var key2 = Keys.B | modifier;

                var chord = new Keys[] { key1, key2 };
                //--- Act
                hc.RegisterHotkey(chord, e => counter++);

                //---Assert

                //Try twice, so we see we can chain hotkeys
                for (int i = 0; i < 2; i++) {
                    counter = 0;

                    //Keys.A with Keys.Control alone is not a hotkey...
                    Assert.That(counter, Is.EqualTo(0));
                    hc.OnKeyDown(sender: null, e: new KeyEventArgs(modifier));
                    Assert.That(counter, Is.EqualTo(0));
                    hc.OnKeyDown(sender: null, e: new KeyEventArgs(Keys.A));
                    Assert.That(counter, Is.EqualTo(0));
                    hc.OnKeyUp(sender: null, e: new KeyEventArgs(Keys.A));
                    Assert.That(counter, Is.EqualTo(0));

                    //But with Keys.B and Control it is
                    hc.OnKeyDown(sender: null, e: new KeyEventArgs(Keys.B));
                    Assert.That(counter, Is.EqualTo(1), $"modifier: {modifier}, round:{i}");
                    hc.OnKeyUp(sender: null, e: new KeyEventArgs(Keys.B));
                    Assert.That(counter, Is.EqualTo(1));
                    hc.OnKeyUp(sender: null, e: new KeyEventArgs(modifier));
                    Assert.That(counter, Is.EqualTo(1));
                }
            }
        }

        [Test]
        public void HotkeyCollectionInternal_ShouldRegisterComplexHotkeyWithMultipleKeyStrokesAndFire2() {
            //--- Assemble
            var counter = 0;
            var hc = new HotkeyCollectionInternal();
            var modifiers = new Keys[]
            {
                 Keys.Control,
                 Keys.Shift ,
                 Keys.Alt     ,
                 Keys.LWin
            };

            foreach (var modifier in modifiers) {
                var key1 = Keys.A | modifier;
                const Keys key2 = Keys.B;

                var chord = new Keys[] { key1, key2 };
                //--- Act
                hc.RegisterHotkey(chord, e => counter++);

                //---Assert

                //Try twice, so we see we can chain hotkeys
                for (int i = 0; i < 2; i++) {
                    counter = 0;
                    //Keys.A with modifier alone is not a hotkey...
                    Assert.That(counter, Is.EqualTo(0));
                    hc.OnKeyDown(sender: null, e: new KeyEventArgs(modifier));
                    Assert.That(counter, Is.EqualTo(0));
                    hc.OnKeyDown(sender: null, e: new KeyEventArgs(Keys.A));
                    Assert.That(counter, Is.EqualTo(0));
                    hc.OnKeyUp(sender: null, e: new KeyEventArgs(Keys.A));
                    Assert.That(counter, Is.EqualTo(0));
                    hc.OnKeyUp(sender: null, e: new KeyEventArgs(modifier));
                    Assert.That(counter, Is.EqualTo(0));

                    //But with Keys.B it is
                    hc.OnKeyDown(sender: null, e: new KeyEventArgs(Keys.B));
                    Assert.That(counter, Is.EqualTo(1));
                    hc.OnKeyUp(sender: null, e: new KeyEventArgs(Keys.B));
                    Assert.That(counter, Is.EqualTo(1));
                }
            }
        }

        [Test]
        public void HotkeyCollectionInternal_ShouldRegisterComplexHotkeyWithMultipleKeyStrokesAndFire3() {
            //--- Assemble
            var counter = 0;
            var hc = new HotkeyCollectionInternal();
            var modifiers = new Keys[]
            {
                 Keys.Control,
                 Keys.Shift ,
                 Keys.Alt     ,
                 Keys.LWin
            };

            foreach (var modifier in modifiers) {
                const Keys key1 = Keys.A;
                var key2 = Keys.B | modifier;

                var chord = new Keys[] { key1, key2 };
                //--- Act
                hc.RegisterHotkey(chord, e => counter++);

                //---Assert
                //Try twice, so we see we can chain hotkeys
                for (int i = 0; i < 2; i++) {
                    counter = 0;
                    //Keys.A alone is not a hotkey...
                    Assert.That(counter, Is.EqualTo(0));
                    hc.OnKeyDown(sender: null, e: new KeyEventArgs(Keys.A));
                    Assert.That(counter, Is.EqualTo(0));
                    hc.OnKeyUp(sender: null, e: new KeyEventArgs(Keys.A));
                    Assert.That(counter, Is.EqualTo(0));

                    //But with Keys.B and modifier it is
                    hc.OnKeyDown(sender: null, e: new KeyEventArgs(modifier));
                    Assert.That(counter, Is.EqualTo(0));
                    hc.OnKeyDown(sender: null, e: new KeyEventArgs(Keys.B));
                    Assert.That(counter, Is.EqualTo(1), $"{modifier}");
                    hc.OnKeyUp(sender: null, e: new KeyEventArgs(Keys.B));
                    Assert.That(counter, Is.EqualTo(1));
                    hc.OnKeyUp(sender: null, e: new KeyEventArgs(modifier));
                    Assert.That(counter, Is.EqualTo(1));
                }
            }
        }

        [Test]
        public void HotkeyCollectionInternal_ShouldRegisterHotkeyWithModifiers() {
            //--- Assemble
            var counter = 0;
            var hc = new HotkeyCollectionInternal();
            const Keys key = Keys.A | Keys.Control | Keys.Alt;

            //--- Act
            hc.RegisterHotkey(key, e => counter++);

            //---Assert

            //Just Keys.A is not a hotkey without modifiers
            Assert.That(counter, Is.EqualTo(0));
            hc.OnKeyDown(sender: null, e: new KeyEventArgs(Keys.A));
            Assert.That(counter, Is.EqualTo(0));
            hc.OnKeyUp(sender: null, e: new KeyEventArgs(Keys.A));
            Assert.That(counter, Is.EqualTo(0));

            //Keys.A and just Control is not a hotkey
            hc.OnKeyDown(sender: null, e: new KeyEventArgs(Keys.Control));
            hc.OnKeyDown(sender: null, e: new KeyEventArgs(Keys.A));
            Assert.That(counter, Is.EqualTo(0));
            hc.OnKeyUp(sender: null, e: new KeyEventArgs(Keys.A));
            hc.OnKeyUp(sender: null, e: new KeyEventArgs(Keys.Control));
            Assert.That(counter, Is.EqualTo(0));

            //Keys.A and just Alt is not a hotkey
            hc.OnKeyDown(sender: null, e: new KeyEventArgs(Keys.Alt));
            hc.OnKeyDown(sender: null, e: new KeyEventArgs(Keys.A));
            Assert.That(counter, Is.EqualTo(0));
            hc.OnKeyUp(sender: null, e: new KeyEventArgs(Keys.A));
            hc.OnKeyUp(sender: null, e: new KeyEventArgs(Keys.Alt));
            Assert.That(counter, Is.EqualTo(0));

            //With Control and Alt Keys.A is a hotkey. Sequence1 of pressing and releasing ctrl and alt
            hc.OnKeyDown(sender: null, e: new KeyEventArgs(Keys.Control));
            hc.OnKeyDown(sender: null, e: new KeyEventArgs(Keys.Alt));
            hc.OnKeyDown(sender: null, e: new KeyEventArgs(Keys.A));
            Assert.That(counter, Is.EqualTo(1), "Sequence1 failed");
            hc.OnKeyUp(sender: null, e: new KeyEventArgs(Keys.A));
            hc.OnKeyUp(sender: null, e: new KeyEventArgs(Keys.Alt));
            hc.OnKeyUp(sender: null, e: new KeyEventArgs(Keys.Control));
            Assert.That(counter, Is.EqualTo(1), "Sequence1 failed");

            //With Control and Alt Keys.A is a hotkey. Sequence2 of pressing and releasing ctrl and alt
            hc.OnKeyDown(sender: null, e: new KeyEventArgs(Keys.Alt));
            hc.OnKeyDown(sender: null, e: new KeyEventArgs(Keys.Control));
            hc.OnKeyDown(sender: null, e: new KeyEventArgs(Keys.A));
            Assert.That(counter, Is.EqualTo(2), "Sequence2 failed");
            hc.OnKeyUp(sender: null, e: new KeyEventArgs(Keys.A));
            hc.OnKeyUp(sender: null, e: new KeyEventArgs(Keys.Alt));
            hc.OnKeyUp(sender: null, e: new KeyEventArgs(Keys.Control));
            Assert.That(counter, Is.EqualTo(2), "Sequence2 failed");

            //With Control and Alt Keys.A is a hotkey. Sequence3 of pressing and releasing ctrl and alt
            hc.OnKeyDown(sender: null, e: new KeyEventArgs(Keys.Control));
            hc.OnKeyDown(sender: null, e: new KeyEventArgs(Keys.Alt));
            hc.OnKeyDown(sender: null, e: new KeyEventArgs(Keys.A));
            Assert.That(counter, Is.EqualTo(3), "Sequence3 failed");
            hc.OnKeyUp(sender: null, e: new KeyEventArgs(Keys.A));
            hc.OnKeyUp(sender: null, e: new KeyEventArgs(Keys.Control));
            hc.OnKeyUp(sender: null, e: new KeyEventArgs(Keys.Alt));
            Assert.That(counter, Is.EqualTo(3), "Sequence3 failed");

            //With Control and Alt Keys.A is a hotkey. Sequence4 of pressing and releasing ctrl and alt
            hc.OnKeyDown(sender: null, e: new KeyEventArgs(Keys.Alt));
            hc.OnKeyDown(sender: null, e: new KeyEventArgs(Keys.Control));
            hc.OnKeyDown(sender: null, e: new KeyEventArgs(Keys.A));
            Assert.That(counter, Is.EqualTo(4), "Sequence4 failed");
            hc.OnKeyUp(sender: null, e: new KeyEventArgs(Keys.A));
            hc.OnKeyUp(sender: null, e: new KeyEventArgs(Keys.Control));
            hc.OnKeyUp(sender: null, e: new KeyEventArgs(Keys.Alt));
            Assert.That(counter, Is.EqualTo(4), "Sequence4 failed");
        }

        [Test]
        public void HotkeyCollectionInternal_ShouldRegisterTwoActionsUnderSamePath() {
            //--- Assemble

            var counter = 0;
            var counter1 = 0;
            var counter2 = 0;

            var hc = new HotkeyCollectionInternal();

            //Register two different actions for the same hotkey
            hc.RegisterHotkey(Keys.G | Keys.Control, e => { counter++; counter1++; }, actionDescription: "action1");
            hc.RegisterHotkey(Keys.G | Keys.Control, e => { counter++; counter2++; }, actionDescription: "action2");

            //--- Act

            hc.OnKeyDown(null, new KeyEventArgs(Keys.Control));
            hc.OnKeyDown(null, new KeyEventArgs(Keys.G));
            hc.OnKeyUp(null, new KeyEventArgs(Keys.G));
            hc.OnKeyUp(null, new KeyEventArgs(Keys.Control));

            //---Assert

            Assert.That(counter, Is.EqualTo(2));
            Assert.That(counter1, Is.EqualTo(1));
            Assert.That(counter2, Is.EqualTo(1));
        }

        /// <summary>
        /// Test that it is possible to add and remove modifiers from any key
        /// </summary>
        [Test]
        public void HotkeyCollectionInternal_ShouldRemoveModifiersFromKey() {
            //--- Assemble
            //These are the possible modifiers
            var modifiers = new Keys[] { Keys.Control, Keys.Shift, Keys.Alt };
            //Create the power set of all possible combinations of modifiers
            var allPossibleModifiers = SetOperations.GetPowerset(modifiers);

            //--- Act

            var allKeys = new List<Keys>(Enum.GetValues(typeof(Keys)) as Keys[]);
            //somehow this gets a strange key (Keys)-65536 that does not exist in the Keys enum.
            //it messes up the test, and since it shouldn't be there in the first place, remove it.
            allKeys.RemoveAt(allKeys.Count - 1);

            //remove all modifier keys.
            var allKeysWithoutModifiers = allKeys.Where(key => !modifiers.Contains(key)).ToList();

            foreach (Keys key in allKeysWithoutModifiers) {
                foreach (var mod in allPossibleModifiers) {
                    var modKeys = mod.Aggregate(Keys.None, (a, b) => a | b);

                    var keyWithModifiers = key | modKeys;

                    var keyWithModifiersRemoved = keyWithModifiers ^ modKeys;
                    //---Assert
                    Assert.That(key, Is.EqualTo(keyWithModifiersRemoved), $"key: {key}");
                }
            }
        }

        [Test]
        public void HotkeyCollectionInternal_ShouldReturnAllHotkeysInHumanReadableForm() {
            //--- Assemble
            var counter = 0;
            var hc = new HotkeyCollectionInternal();
            const Keys keyA = Keys.A;
            const Keys keyB = Keys.B;
            const Keys ctrlA = Keys.A | Keys.Control;

            const Keys ctrlZ = Keys.Z | Keys.Control;

            //--- Act

            hc.RegisterHotkey(new Keys[] { keyA, keyB }, e => counter++);
            hc.RegisterHotkey(new Keys[] { keyB }, e => counter++, "desc B");
            hc.RegisterHotkey(new Keys[] { ctrlA, ctrlA, ctrlA }, e => counter++, "desc 3xCtrl+A");
            hc.RegisterHotkey(new Keys[] { keyA }, e => counter++, "desc A");

            //register two actions to same hotkey
            hc.RegisterHotkey(new Keys[] { ctrlZ }, e => counter++, "desc Z1");
            hc.RegisterHotkey(new Keys[] { ctrlZ }, e => counter++, "desc Z2");

            //---Assert
            var desc = hc.ToString();

            var expected = "Registered Hotkeys:" + Environment.NewLine +
                           "- a (desc A)" + Environment.NewLine +
                           "- b (desc B)" + Environment.NewLine +
                           "- Control+z (desc Z1)" + Environment.NewLine +
                           "- Control+z (desc Z2)" + Environment.NewLine +
                           "- a, b" + Environment.NewLine +
                           "- Control+a, Control+a, Control+a (desc 3xCtrl+A)";

            Assert.That(desc, Is.EqualTo(expected));
            Console.WriteLine(desc);
        }

        [Test]
        public void HotkeyCollectionInternal_ShouldReturnCurrentlyRecognizedKeys() {
            //--- Assemble
            var counter = 0;
            var hc = new HotkeyCollectionInternal();
            hc.RegisterHotkey(new Keys[] { Keys.A | Keys.Control, Keys.B | Keys.Control, Keys.C | Keys.Control }, e => counter++);
            hc.RegisterHotkey(new Keys[] { Keys.A, Keys.X | Keys.Control }, e => counter++);

            //--- Act

            //before we start typing there should be no recognized keys
            var currentlyRecognized = hc.GetCurrentlyRecognizedPartialChord();
            Assert.IsFalse(currentlyRecognized.Any());

            //Type lorem ipsum. Since none of it is the start of a hotkey,
            foreach (var c in loremIpsum) {
                //press key
                hc.OnKeyDown(null, new KeyEventArgs(Char2Keys(c)));
                currentlyRecognized = hc.GetCurrentlyRecognizedPartialChord();

                //if key was 'a', this is the start of a Chord (namely "A, ctrl+x"). Since we never press control here, this chord is never finished and triggered
                var expected = (char.ToLower(c) == 'a' ? 1 : 0);
                Assert.That(currentlyRecognized.Count(), Is.EqualTo(expected));

                hc.OnKeyUp(null, new KeyEventArgs(Char2Keys(c)));
                currentlyRecognized = hc.GetCurrentlyRecognizedPartialChord();
                Assert.That(currentlyRecognized.Count(), Is.EqualTo(expected), $"char '{c}'");

                //assert that no chord was triggered
                Assert.That(counter, Is.EqualTo(0));
            }

            //-------trigger chord, key by key

            //press Control, no chord triggered and no start of chord recognized
            hc.OnKeyDown(null, Keys.Control);
            currentlyRecognized = hc.GetCurrentlyRecognizedPartialChord();
            Assert.IsFalse(currentlyRecognized.Any());
            Assert.That(counter, Is.EqualTo(0));

            //press and release A, no chord triggered , but "ctrl+a" is recognized as the start of a chord
            hc.OnKeyDown(null, Keys.A);
            currentlyRecognized = hc.GetCurrentlyRecognizedPartialChord();
            Assert.That(currentlyRecognized.Count(), Is.EqualTo(1));
            Assert.That(currentlyRecognized.Last(), Is.EqualTo(Keys.A | Keys.Control));
            Assert.That(counter, Is.EqualTo(0));
            hc.OnKeyUp(null, Keys.A);
            currentlyRecognized = hc.GetCurrentlyRecognizedPartialChord();
            Assert.That(currentlyRecognized.Count(), Is.EqualTo(1));
            Assert.That(currentlyRecognized.Last(), Is.EqualTo(Keys.A | Keys.Control));
            Assert.That(counter, Is.EqualTo(0));

            //press and release B, no chord triggered , but "ctrl+a, ctrl+b" is recognized as the start of a chord
            hc.OnKeyDown(null, Keys.B);
            currentlyRecognized = hc.GetCurrentlyRecognizedPartialChord();
            Assert.That(currentlyRecognized.Count(), Is.EqualTo(2));
            Assert.That(currentlyRecognized.First(), Is.EqualTo(Keys.A | Keys.Control));
            Assert.That(currentlyRecognized.Last(), Is.EqualTo(Keys.B | Keys.Control));
            Assert.That(counter, Is.EqualTo(0));
            hc.OnKeyUp(null, Keys.B);
            currentlyRecognized = hc.GetCurrentlyRecognizedPartialChord();
            Assert.That(currentlyRecognized.Count(), Is.EqualTo(2));
            Assert.That(currentlyRecognized.First(), Is.EqualTo(Keys.A | Keys.Control));
            Assert.That(currentlyRecognized.Last(), Is.EqualTo(Keys.B | Keys.Control));
            Assert.That(counter, Is.EqualTo(0));

            //press and release C, chord is finally triggered, so there is again no recognized chord start
            hc.OnKeyDown(null, Keys.C);
            currentlyRecognized = hc.GetCurrentlyRecognizedPartialChord();
            Assert.That(currentlyRecognized.Count(), Is.EqualTo(0));
            Assert.That(counter, Is.EqualTo(1));
            hc.OnKeyUp(null, Keys.C);
            currentlyRecognized = hc.GetCurrentlyRecognizedPartialChord();
            Assert.That(currentlyRecognized.Count(), Is.EqualTo(0));
            Assert.That(counter, Is.EqualTo(1));

            hc.OnKeyUp(null, Keys.Control);
            currentlyRecognized = hc.GetCurrentlyRecognizedPartialChord();
            Assert.That(currentlyRecognized.Count(), Is.EqualTo(0));
            Assert.That(counter, Is.EqualTo(1));
        }

        [Test]
        public void HotkeyCollectionInternal_ShouldTellIfChordWasTriggeredAsFollowUp1() {
            //--- Assemble
            var counter = 0;
            HotKeyEventArgs eventArgs = null;

            var testCases = new List<Tuple<Keys, bool>> { new Tuple<Keys, bool>(Keys.None,true),     //No key pressed in between the hotkeys: followUp=true
                                                          new Tuple<Keys, bool>(Keys.A,false),       //non modifier key pressed in between hotkeys: followUp=false
                                                          new Tuple<Keys, bool>(Keys.Control,true),  //modifier key pressed in between hotkeys: followUp=true
                                                          new Tuple<Keys, bool>(Keys.Alt,true),
                                                          new Tuple<Keys, bool>(Keys.Shift,true)};

            foreach (var testcase in testCases) {
                counter = 0;
                var hc = new HotkeyCollectionInternal();
                hc.RegisterHotkey(Keys.G, e => { counter++; eventArgs = e; });

                //Trigger the hotkey
                hc.OnKeyDown(null, Keys.G);
                hc.OnKeyUp(null, Keys.G);

                //The hotkey was triggered without any hotkey before it, so it can't be a follow up of another hotkey
                Assert.That(counter, Is.EqualTo(1));
                Assert.IsFalse(eventArgs.FollowUp);

                var key = testcase.Item1;
                if (key != Keys.None) {
                    //Press a key that does not belong to the hotkey(s)
                    hc.OnKeyDown(null, key);
                    hc.OnKeyUp(null, key);
                }

                //Trigger the hotkey again
                hc.OnKeyDown(null, Keys.G);
                hc.OnKeyUp(null, Keys.G);

                //Now there was a hotkey before this, so whether it is a follow up depends on the key(s) pressed between them
                Assert.That(counter, Is.EqualTo(2));
                Assert.That(eventArgs.FollowUp, Is.EqualTo(testcase.Item2));
            }
        }

        [Test]
        public void HotkeyCollectionInternal_ShouldTellIfChordWasTriggeredAsFollowUp2() {
            //--- Assemble
            var counter = 0;
            HotKeyEventArgs eventArgs = null;

            var testCases = new List<Tuple<Keys, bool>> { new Tuple<Keys, bool>(Keys.None,true),     //No key pressed in between the hotkeys: followUp=true
                                                          new Tuple<Keys, bool>(Keys.A,false),       //non modifier key pressed in between hotkeys: followUp=false
                                                          new Tuple<Keys, bool>(Keys.Control,true),  //modifier key pressed in between hotkeys: followUp=true
                                                          new Tuple<Keys, bool>(Keys.Alt,true),
                                                          new Tuple<Keys, bool>(Keys.Shift,true)};

            foreach (var testcase in testCases) {
                counter = 0;
                var hc = new HotkeyCollectionInternal();
                hc.RegisterHotkey(Keys.G | Keys.Alt, e => { counter++; eventArgs = e; });
                hc.RegisterHotkey(Keys.H | Keys.Shift, e => { counter++; eventArgs = e; });

                //Trigger the first hotkey
                hc.OnKeyDown(null, Keys.Alt);
                hc.OnKeyDown(null, Keys.G);
                hc.OnKeyUp(null, Keys.G);
                hc.OnKeyUp(null, Keys.Alt);

                //The hotkey was triggered without any hotkey before it, so it can't be a follow up of another hotkey
                Assert.That(counter, Is.EqualTo(1));
                Assert.IsFalse(eventArgs.FollowUp);

                var key = testcase.Item1;
                if (key != Keys.None) {
                    //Press a key that does not belong to the hotkey(s)
                    hc.OnKeyDown(null, key);
                    hc.OnKeyUp(null, key);
                }

                //Trigger the other hotkey
                hc.OnKeyDown(null, Keys.Shift);
                hc.OnKeyDown(null, Keys.H);
                hc.OnKeyUp(null, Keys.H);
                hc.OnKeyUp(null, Keys.Shift);
                //Now there was a hotkey before this, so whether it is a follow up depends on the key(s) pressed between them
                Assert.That(counter, Is.EqualTo(2));
                Assert.That(eventArgs.FollowUp, Is.EqualTo(testcase.Item2));
            }
        }

        [Test]
        public void HotkeyCollectionInternal_ShouldTellIfChordWasTriggeredAsFollowUp3() {
            //--- Assemble
            var counter = 0;
            HotKeyEventArgs eventArgs = null;

            var testCases = new List<Tuple<Keys, bool>> { new Tuple<Keys, bool>(Keys.None,true),     //No key pressed in between the hotkeys: followUp=true
                                                          new Tuple<Keys, bool>(Keys.A,false),       //non modifier key pressed in between hotkeys: followUp=false
                                                          new Tuple<Keys, bool>(Keys.Control,true),  //modifier key pressed in between hotkeys: followUp=true
                                                          new Tuple<Keys, bool>(Keys.Alt,true),
                                                          new Tuple<Keys, bool>(Keys.Shift,true)};

            foreach (var testcase in testCases) {
                counter = 0;
                var hc = new HotkeyCollectionInternal();
                hc.RegisterHotkey(new Keys[] { Keys.G | Keys.Alt, Keys.A }, e => { counter++; eventArgs = e; });
                hc.RegisterHotkey(new Keys[] { Keys.H | Keys.Shift, Keys.H | Keys.Shift }, e => { counter++; eventArgs = e; });

                //Trigger the first chord
                hc.OnKeyDown(null, Keys.Alt);
                hc.OnKeyDown(null, Keys.G);
                hc.OnKeyUp(null, Keys.G);
                hc.OnKeyUp(null, Keys.Alt);
                hc.OnKeyDown(null, Keys.A);
                hc.OnKeyUp(null, Keys.A);

                //The hotkey was triggered without any hotkey before it, so it can't be a follow up of another hotkey
                Assert.That(counter, Is.EqualTo(1));
                Assert.IsFalse(eventArgs.FollowUp);

                var key = testcase.Item1;
                if (key != Keys.None) {
                    //Press a key that does not belong to the hotkey(s)
                    hc.OnKeyDown(null, key);
                    hc.OnKeyUp(null, key);
                }

                //Trigger the other chord
                hc.OnKeyDown(null, Keys.Shift);
                hc.OnKeyDown(null, Keys.H);
                hc.OnKeyUp(null, Keys.H);
                hc.OnKeyDown(null, Keys.H);
                hc.OnKeyUp(null, Keys.H);
                hc.OnKeyUp(null, Keys.Shift);

                //Now there was a hotkey before this, so whether it is a follow up depends on the key(s) pressed between them
                Assert.That(counter, Is.EqualTo(2));
                Assert.That(eventArgs.FollowUp, Is.EqualTo(testcase.Item2));
            }
        }

        [Test]
        public void HotkeyCollectionInternal_ShouldTellIfChordWasTriggeredContiniously1() {
            //--- Assemble
            var counter = 0;
            HotKeyEventArgs eventArgs = null;

            var testCases = new List<Keys> {Keys.None,    //No key pressed in between the hotkeys: followUp=true
                                            Keys.A,       //non modifier key pressed in between hotkeys: followUp=false
                                            Keys.Control, //modifier key pressed in between hotkeys: followUp=true
                                            Keys.Alt,
                                            Keys.Shift};

            foreach (var key in testCases) {
                counter = 0;
                var hc = new HotkeyCollectionInternal();

                //single key hotkey, no modifiers...
                hc.RegisterHotkey(Keys.G, e => { counter++; eventArgs = e; });

                //Trigger the hotkey
                hc.OnKeyDown(null, Keys.G);
                hc.OnKeyUp(null, Keys.G);

                //The hotkey was triggered without any hotkey before it, so it can't be continuously
                Assert.That(counter, Is.EqualTo(1));
                Assert.IsFalse(eventArgs.Continuously);

                if (key != Keys.None) {
                    //Press a key that does not belong to the hotkey(s)
                    hc.OnKeyDown(null, key);
                    hc.OnKeyUp(null, key);
                }

                //Trigger the hotkey again
                hc.OnKeyDown(null, Keys.G);
                hc.OnKeyUp(null, Keys.G);

                //Now there was a hotkey before this, so whether it is a follow up depends on the key(s) pressed between them
                Assert.That(counter, Is.EqualTo(2));
                Assert.IsFalse(eventArgs.Continuously);
            }
        }

        [Test]
        public void HotkeyCollectionInternal_ShouldTellIfChordWasTriggeredContiniously2([Values(true, false)] bool isContiuously, [Values(true, false)] bool insertKeyBetweenHotkeys) {
            //--- Assemble
            var counter = 0;
            HotKeyEventArgs eventArgs = null;

            counter = 0;
            var hc = new HotkeyCollectionInternal();

            //single key hotkey, no modifiers...
            hc.RegisterHotkey(Keys.G | Keys.Control, e => { counter++; eventArgs = e; });

            //Trigger the hotkey

            hc.OnKeyDown(null, Keys.Control);

            hc.OnKeyDown(null, Keys.G);
            hc.OnKeyUp(null, Keys.G);

            if (!isContiuously) {
                hc.OnKeyUp(null, Keys.Control);
            }

            //The hotkey was triggered without any hotkey before it, so it can't be continuously
            Assert.That(counter, Is.EqualTo(1));
            Assert.IsFalse(eventArgs.Continuously);

            if (insertKeyBetweenHotkeys) {
                hc.OnKeyDown(null, Keys.A);
                hc.OnKeyUp(null, Keys.A);
            }

            if (!isContiuously) {
                hc.OnKeyDown(null, Keys.Control);
            }

            //Trigger the hotkey again
            hc.OnKeyDown(null, Keys.G);
            hc.OnKeyUp(null, Keys.G);

            hc.OnKeyUp(null, Keys.Control);

            //Now there was a hotkey before this, so whether it is continuously depends on whether a key was pressed between them
            Assert.That(counter, Is.EqualTo(2));
            Assert.IsFalse(eventArgs.Continuously);

            Assert.That(eventArgs.Continuously, Is.EqualTo(isContiuously && !insertKeyBetweenHotkeys));
        }

        [Test]
        public void HotkeyCollectionInternal_ShouldThrowWhenTryingToRegisterTwoActionsUnderSamePathWithSameDescription() {
            //--- Assemble

            var counter = 0;
            var counter1 = 0;
            var counter2 = 0;

            var hc = new HotkeyCollectionInternal();

            //Register two different actions for the same hotkey using the same description
            const string actionDescription = "action1";
            hc.RegisterHotkey(Keys.G | Keys.Control, e => { counter++; counter1++; }, actionDescription: actionDescription);
            Assert.Throws<ArgumentException>(() => hc.RegisterHotkey(Keys.G | Keys.Control, e => { counter++; counter2++; }, actionDescription: actionDescription));
        }

        [Test]
        public void HotkeyCollectionInternal_ShouldTriggerChordWithDifferentPositionsPressingAndReleasingModifiers() {
            //--- Assemble
            var counter = 0;

            var chord = new Keys[] { Keys.A | Keys.Control, Keys.B | Keys.Control, Keys.C | Keys.Control };
            var hc = new HotkeyCollectionInternal();

            hc.RegisterHotkey(chord, e => counter++);

            //----- First: Modifiers stay on throughout the chord
            //--- Act
            hc.OnKeyDown(null, new KeyEventArgs(Keys.Control));

            hc.OnKeyDown(null, new KeyEventArgs(Keys.A));
            hc.OnKeyUp(null, new KeyEventArgs(Keys.A));
            hc.OnKeyDown(null, new KeyEventArgs(Keys.B));
            hc.OnKeyUp(null, new KeyEventArgs(Keys.B));
            hc.OnKeyDown(null, new KeyEventArgs(Keys.C));
            hc.OnKeyUp(null, new KeyEventArgs(Keys.C));

            hc.OnKeyUp(null, new KeyEventArgs(Keys.Control));

            //---Assert
            Assert.That(counter, Is.EqualTo(1));

            //----------------------------------------------------
            //----- Second: Modifiers around every key press
            //--- Act
            hc.OnKeyDown(null, new KeyEventArgs(Keys.Control));
            hc.OnKeyDown(null, new KeyEventArgs(Keys.A));
            hc.OnKeyUp(null, new KeyEventArgs(Keys.A));
            hc.OnKeyUp(null, new KeyEventArgs(Keys.Control));

            hc.OnKeyDown(null, new KeyEventArgs(Keys.Control));
            hc.OnKeyDown(null, new KeyEventArgs(Keys.B));
            hc.OnKeyUp(null, new KeyEventArgs(Keys.B));
            hc.OnKeyUp(null, new KeyEventArgs(Keys.Control));

            hc.OnKeyDown(null, new KeyEventArgs(Keys.Control));
            hc.OnKeyDown(null, new KeyEventArgs(Keys.C));
            hc.OnKeyUp(null, new KeyEventArgs(Keys.C));
            hc.OnKeyUp(null, new KeyEventArgs(Keys.Control));

            //---Assert
            Assert.That(counter, Is.EqualTo(2));

            //----------------------------------------------------
            //----- Third: Modifiers around the first two and around the last key
            //--- Act
            hc.OnKeyDown(null, new KeyEventArgs(Keys.Control));
            hc.OnKeyDown(null, new KeyEventArgs(Keys.A));
            hc.OnKeyUp(null, new KeyEventArgs(Keys.A));
            hc.OnKeyDown(null, new KeyEventArgs(Keys.B));
            hc.OnKeyUp(null, new KeyEventArgs(Keys.B));
            hc.OnKeyUp(null, new KeyEventArgs(Keys.Control));

            hc.OnKeyDown(null, new KeyEventArgs(Keys.Control));
            hc.OnKeyDown(null, new KeyEventArgs(Keys.C));
            hc.OnKeyUp(null, new KeyEventArgs(Keys.C));
            hc.OnKeyUp(null, new KeyEventArgs(Keys.Control));

            //---Assert
            Assert.That(counter, Is.EqualTo(3));

            //----------------------------------------------------
            //----- Fourth: Modifiers around the last two and around the first key
            //--- Act
            hc.OnKeyDown(null, new KeyEventArgs(Keys.Control));
            hc.OnKeyDown(null, new KeyEventArgs(Keys.A));
            hc.OnKeyUp(null, new KeyEventArgs(Keys.A));
            hc.OnKeyUp(null, new KeyEventArgs(Keys.Control));

            hc.OnKeyDown(null, new KeyEventArgs(Keys.Control));
            hc.OnKeyDown(null, new KeyEventArgs(Keys.B));
            hc.OnKeyUp(null, new KeyEventArgs(Keys.B));
            hc.OnKeyDown(null, new KeyEventArgs(Keys.C));
            hc.OnKeyUp(null, new KeyEventArgs(Keys.C));
            hc.OnKeyUp(null, new KeyEventArgs(Keys.Control));

            //---Assert
            Assert.That(counter, Is.EqualTo(4));
        }

        [Test]
        public void HotkeyCollectionInternal_ShouldTriggerTwoSimpleHotkeyConsecutively() {
            //--- Assemble
            var counter = 0;
            var hc = new HotkeyCollectionInternal();

            hc.RegisterHotkey(Keys.C | Keys.Control, eventArgs => counter++);
            hc.RegisterHotkey(Keys.V | Keys.Control, eventArgs => counter++);

            //--- Act

            //press Control
            hc.OnKeyDown(null, new KeyEventArgs(Keys.Control));
            Assert.That(counter, Is.EqualTo(0));

            //Press C => ctrl+C should trigger
            hc.OnKeyDown(null, new KeyEventArgs(Keys.C));
            Assert.That(counter, Is.EqualTo(1));

            //Press V => ctrl+V should trigger
            hc.OnKeyDown(null, new KeyEventArgs(Keys.V));
            Assert.That(counter, Is.EqualTo(2));
        }

        [Test]
        public void HotkeyCollectionInternal_ShouldUseHotkeysInTypicalSituation([Values(true, false)] bool pad) {
            //--- Assemble

            var ctrlC = 0;
            var ctrlA = 0;
            var ctrlV = 0;
            var chord = 0;

            var padding = pad ? "a" : "";
            var lorem = padding + loremIpsum;

            var hotkeys = new List<List<Keys>> { new List<Keys> { Keys.A },
                                                   new List<Keys> { Keys.C },
                                                   new List<Keys> { Keys.V },
                                                   new List<Keys> { Keys.T, Keys.G } }.ToArray();

            var hotkeyCombinations = SetOperations.GetPowerset(hotkeys);

            var hc = new HotkeyCollectionInternal();

            hc.RegisterHotkey(Keys.A | Keys.Control, (e) => ctrlA++);
            hc.RegisterHotkey(Keys.C | Keys.Control, (e) => ctrlC++);
            hc.RegisterHotkey(Keys.V | Keys.Control, (e) => ctrlV++);
            hc.RegisterHotkey(new Keys[] { Keys.T | Keys.Control, Keys.G | Keys.Control }, (e) => chord++);

            //--- Act
            //Type lorem ipsum
            foreach (var c in lorem) {
                hc.OnKeyDown(null, new KeyEventArgs(Char2Keys(c)));
                hc.OnKeyUp(null, new KeyEventArgs(Char2Keys(c)));
            }

            foreach (var mods in hotkeyCombinations) {
                foreach (var mod in mods) {
                    hc.OnKeyDown(null, new KeyEventArgs(Keys.Control));
                    foreach (var key in mod) {
                        hc.OnKeyDown(null, new KeyEventArgs(key));
                        hc.OnKeyUp(null, new KeyEventArgs(key));
                    }
                    hc.OnKeyUp(null, new KeyEventArgs(Keys.Control));
                }
            }

            //---Assert
            Assert.That(ctrlA, Is.EqualTo(8), nameof(ctrlA));
            Assert.That(ctrlV, Is.EqualTo(8), nameof(ctrlV));
            Assert.That(ctrlC, Is.EqualTo(8), nameof(ctrlC));
            Assert.That(chord, Is.EqualTo(8), nameof(chord));

            foreach (var hotkey in hotkeys) {
                //Type lorem ipsum
                foreach (var c in lorem) {
                    hc.OnKeyDown(null, new KeyEventArgs(Char2Keys(c)));
                    hc.OnKeyUp(null, new KeyEventArgs(Char2Keys(c)));
                }

                //use hotkey once more
                hc.OnKeyDown(null, new KeyEventArgs(Keys.Control));
                foreach (var key in hotkey) {
                    hc.OnKeyDown(null, new KeyEventArgs(key));
                    hc.OnKeyUp(null, new KeyEventArgs(key));
                }
                hc.OnKeyUp(null, new KeyEventArgs(Keys.Control));
            }

            //---Assert
            Assert.That(ctrlA, Is.EqualTo(9), nameof(ctrlA));
            Assert.That(ctrlV, Is.EqualTo(9), nameof(ctrlV));
            Assert.That(ctrlC, Is.EqualTo(9), nameof(ctrlC));
            Assert.That(chord, Is.EqualTo(9), nameof(chord));
        }

        private static Keys Char2Keys(char c) {
            return (Keys)char.ToUpper(c);
        }

        private static IEnumerable<Keys> GetAllActiveModifiers(Keys key, IEnumerable<Keys> modifier) {
            var active = new List<Keys>();
            foreach (var mod in modifier) {
                if ((key & mod) != 0) {
                    active.Add(mod);
                }
            }
            return active;
        }

        private static Keys GetRawKey(Keys key, IEnumerable<Keys> activeModifiers) {
            return key ^ activeModifiers.Aggregate(Keys.None, (a, b) => a | b);
        }
    }
}