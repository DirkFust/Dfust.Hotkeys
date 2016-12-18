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
        private const string loremIpsum = "Lorem ipsum dolor sit amet, consetetur sadipscing elitr, sed diam nonumy eirmod tempor invidunt ut labore et dolore magna aliquyam erat, sed diam voluptua. At vero eos et accusam et justo duo dolores et ea rebum. Stet clita kasd gubergren, no sea takimata sanctus est Lorem ipsum dolor sit amet. Lorem ipsum dolor sit amet, consetetur sadipscing elitr, sed diam nonumy eirmod tempor invidunt ut labore et dolore magna aliquyam erat, sed diam voluptua. At vero eos et accusam et justo duo dolores et ea rebum. Stet clita kasd gubergren, no sea takimata sanctus est Lorem ipsum dolor sit amet. Lorem ipsum dolor sit amet, consetetur sadipscing elitr, sed diam nonumy eirmod tempor invidunt ut labore et dolore magna aliquyam erat, sed diam voluptua. At vero eos et accusam et justo duo dolores et ea rebum. Stet clita kasd gubergren, no sea takimata sanctus est Lorem ipsum dolor sit amet. Duis autem vel eum iriure dolor in endrerit in vulputate velit esse molestie consequat, vel illum dolore eu";
        private static int m_counter;

        [Test]
        public void HotkeyCollectionShouldBeInstantiable() {
            Assert.DoesNotThrow(() => new HotkeyCollectionInternal());
        }

        [Test]
        public void HotkeyCollectionShouldNotFireOnWrongModifier() {
            //--- Assemble
            var hc = new HotkeyCollectionInternal();
            const Keys key = Keys.A;

            hc.RegisterHotkey(key, TestMethod);

            var modifiers = new Keys[]
            {
                 Keys.Control,
                 Keys.Shift ,
                 Keys.Alt     ,
                 Keys.LWin
            };

            //--- Act / Assert
            foreach (var item in modifiers) {
                Assert.That(m_counter, Is.EqualTo(0));

                //Modifier key down
                hc.OnKeyDown(sender: null, e: new KeyEventArgs(item));
                Assert.That(m_counter, Is.EqualTo(0));

                //A+modifier key down
                hc.OnKeyDown(sender: null, e: new KeyEventArgs(key));
                Assert.That(m_counter, Is.EqualTo(0), $"key={item}");

                //A+modifier key up
                hc.OnKeyUp(sender: null, e: new KeyEventArgs(key));
                Assert.That(m_counter, Is.EqualTo(0));

                //Modifier key up
                hc.OnKeyUp(sender: null, e: new KeyEventArgs(item));
                Assert.That(m_counter, Is.EqualTo(0));
            }
        }

        [Test]
        public void HotkeyCollectionShouldRegisterSimpleHotkeyAndFireWhenDetected() {
            //--- Assemble
            var hc = new HotkeyCollectionInternal();
            const Keys key = Keys.A;

            //--- Act
            hc.RegisterHotkey(key, TestMethod);

            Assert.That(m_counter, Is.EqualTo(0));

            //---Assert

            //Try twice, so we see we can chain hotkeys
            for (int i = 0; i < 2; i++) {
                m_counter = 0;

                hc.OnKeyDown(sender: null, e: new KeyEventArgs(Keys.A));
                Assert.That(m_counter, Is.EqualTo(1));

                hc.OnKeyUp(sender: null, e: new KeyEventArgs(Keys.A));
                Assert.That(m_counter, Is.EqualTo(1));
            }
        }

        [SetUp]
        public void SetUp() {
            m_counter = 0;
        }

        [Test]
        public void ShouldBeAbleToChangeTheHandledPropertyPerHotkey() {
            //--- Assemble
            var hc = new HotkeyCollectionInternal();

            hc.RegisterHotkey(Keys.C | Keys.Control, eventArgs => m_counter++, handled: false);
            hc.RegisterHotkey(Keys.V | Keys.Control, eventArgs => m_counter++, handled: true);

            //--- Act

            //handled on non-hotkey key (V without control) stays false
            var e = new KeyEventArgs(Keys.V);
            Assert.IsFalse(e.Handled);
            hc.OnKeyDown(null, e);
            Assert.IsFalse(e.Handled);

            Assert.That(m_counter, Is.EqualTo(0));

            //handled on modifier stays false
            e = new KeyEventArgs(Keys.Control);
            Assert.IsFalse(e.Handled);
            hc.OnKeyDown(null, e);
            Assert.IsFalse(e.Handled);
            Assert.That(m_counter, Is.EqualTo(0));

            //handled on ctrl+C stays false (we said so during registration)
            e = new KeyEventArgs(Keys.C);
            Assert.IsFalse(e.Handled);
            hc.OnKeyDown(null, e);
            Assert.IsFalse(e.Handled);
            Assert.That(m_counter, Is.EqualTo(1));

            //handled on ctrl+V toggles (we said so during registration)
            e = new KeyEventArgs(Keys.V);
            Assert.IsFalse(e.Handled);
            hc.OnKeyDown(null, e);
            Assert.That(m_counter, Is.EqualTo(2));
            Assert.IsTrue(e.Handled);
        }

        [Test]
        public void ShouldCountNumberOfConsecutivelyTriggersOfSameHotkey([Range(1, 10)] int pressCount, [Values(1, 5, 10)]int chordLength) {
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
        public void ShouldDetectChordOfDifferentKeysInAnyPossibleCombinationOfKeystrokes([Values(1, 2, 3, 4)] int chordLength) {
            //--- Assemble

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
                m_counter = 0;
                var hc = new HotkeyCollectionInternal();
                hc.RegisterHotkey(chord, TestMethod);

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
                    Assert.That(m_counter, Is.EqualTo(round), $"chord: {Keys2String.ChordToString(chord)}, keySequence: {keySequence}, round {round}");
                    keySequence.Append($"  ->  ");
                }
            }
        }

        [Test]
        public void ShouldFireSameChordMultipleTimes([Values(1, 2, 3, 4, 5)] int chordLength, [Values(true, false)]bool useOnlyOneKeyForChords) {
            //--- Assemble

            //These are the possible modifiers
            var modifiers = new Keys[] { Keys.Control, Keys.Shift, Keys.Alt }; // TODO:, Keys.LWin };
            //Create the power set of all possible combinations of modifiers
            var allPossibleModifiers = SetOperations.GetPowerset(modifiers);

            //how many times do we want to consecutively trigger the chord?
            for (int i = 0; i < 10; i++) {
                foreach (var modifier in allPossibleModifiers) {
                    m_counter = 0;
                    var mods = modifier.Aggregate(Keys.None, (a, b) => a | b);

                    //The keys Z,X,W,H,F do not appear in lorem ipsum, so writing it does not trigger the chords...
                    var possibleHotkeys = new Keys[] { Keys.Z, Keys.X, Keys.W, Keys.H, Keys.F };

                    var chord = Enumerable.Repeat(possibleHotkeys[0] | mods, chordLength);
                    if (!useOnlyOneKeyForChords) {
                        chord = possibleHotkeys.Take(chordLength).Select(k => k | mods);
                    }

                    var hc = new HotkeyCollectionInternal();
                    hc.RegisterHotkey(chord, TestMethod);

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
                    Assert.That(m_counter, Is.EqualTo(i + 1), $"chord: {Keys2String.ChordToString(chord)}, i: {i}");
                }
            }
        }

        [Test]
        public void ShouldHaveCountOf1WhenTriggeringOtherHotkeyBetweenTwoTimesSameHotkey() {
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
        public void ShouldListAllRegisteredHotkeys() {
            //--- Assemble
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

            hc.RegisterHotkey(shift_A, TestMethod);
            hc.RegisterHotkey(a, TestMethod);
            hc.RegisterHotkey(chord1, TestMethod);
            hc.RegisterHotkey(chord2, TestMethod);
            hc.RegisterHotkey(chord3, TestMethod);

            //--- Act

            var hotkeys = hc.GetHotkeys();

            //---Assert
            Assert.That(hotkeys.Count(), Is.EqualTo(5));
        }

        [Test]
        public void ShouldNotFireWhenReleasingAllModifiersBetweenChord() {
            //--- Assemble
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
                hc.RegisterHotkey(chord, TestMethod);

                //---Assert

                //Press modifier and A and release both. No hotkey should trigger.
                Assert.That(m_counter, Is.EqualTo(0));
                hc.OnKeyDown(sender: null, e: new KeyEventArgs(modifier));
                Assert.That(m_counter, Is.EqualTo(0));
                hc.OnKeyDown(sender: null, e: new KeyEventArgs(Keys.A));
                Assert.That(m_counter, Is.EqualTo(0));
                hc.OnKeyUp(sender: null, e: new KeyEventArgs(modifier));
                Assert.That(m_counter, Is.EqualTo(0));

                //Press modifier and B and release both.  No hotkey should trigger.
                hc.OnKeyDown(sender: null, e: new KeyEventArgs(modifier));
                Assert.That(m_counter, Is.EqualTo(0));
                hc.OnKeyUp(sender: null, e: new KeyEventArgs(Keys.B));
                Assert.That(m_counter, Is.EqualTo(0));
                hc.OnKeyUp(sender: null, e: new KeyEventArgs(modifier));
                Assert.That(m_counter, Is.EqualTo(0));
            }
        }

        [Test]
        public void ShouldRegisterComplexHotkeyWithMultipleKeyStrokesAndFire1() {
            //--- Assemble
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
                hc.RegisterHotkey(chord, TestMethod);

                //---Assert

                //Try twice, so we see we can chain hotkeys
                for (int i = 0; i < 2; i++) {
                    m_counter = 0;

                    //Keys.A with Keys.Control alone is not a hotkey...
                    Assert.That(m_counter, Is.EqualTo(0));
                    hc.OnKeyDown(sender: null, e: new KeyEventArgs(modifier));
                    Assert.That(m_counter, Is.EqualTo(0));
                    hc.OnKeyDown(sender: null, e: new KeyEventArgs(Keys.A));
                    Assert.That(m_counter, Is.EqualTo(0));
                    hc.OnKeyUp(sender: null, e: new KeyEventArgs(Keys.A));
                    Assert.That(m_counter, Is.EqualTo(0));

                    //But with Keys.B and Control it is
                    hc.OnKeyDown(sender: null, e: new KeyEventArgs(Keys.B));
                    Assert.That(m_counter, Is.EqualTo(1), $"modifier: {modifier}, round:{i}");
                    hc.OnKeyUp(sender: null, e: new KeyEventArgs(Keys.B));
                    Assert.That(m_counter, Is.EqualTo(1));
                    hc.OnKeyUp(sender: null, e: new KeyEventArgs(modifier));
                    Assert.That(m_counter, Is.EqualTo(1));
                }
            }
        }

        [Test]
        public void ShouldRegisterComplexHotkeyWithMultipleKeyStrokesAndFire2() {
            //--- Assemble
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
                hc.RegisterHotkey(chord, TestMethod);

                //---Assert

                //Try twice, so we see we can chain hotkeys
                for (int i = 0; i < 2; i++) {
                    m_counter = 0;
                    //Keys.A with modifier alone is not a hotkey...
                    Assert.That(m_counter, Is.EqualTo(0));
                    hc.OnKeyDown(sender: null, e: new KeyEventArgs(modifier));
                    Assert.That(m_counter, Is.EqualTo(0));
                    hc.OnKeyDown(sender: null, e: new KeyEventArgs(Keys.A));
                    Assert.That(m_counter, Is.EqualTo(0));
                    hc.OnKeyUp(sender: null, e: new KeyEventArgs(Keys.A));
                    Assert.That(m_counter, Is.EqualTo(0));
                    hc.OnKeyUp(sender: null, e: new KeyEventArgs(modifier));
                    Assert.That(m_counter, Is.EqualTo(0));

                    //But with Keys.B it is
                    hc.OnKeyDown(sender: null, e: new KeyEventArgs(Keys.B));
                    Assert.That(m_counter, Is.EqualTo(1));
                    hc.OnKeyUp(sender: null, e: new KeyEventArgs(Keys.B));
                    Assert.That(m_counter, Is.EqualTo(1));
                }
            }
        }

        [Test]
        public void ShouldRegisterComplexHotkeyWithMultipleKeyStrokesAndFire3() {
            //--- Assemble
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
                hc.RegisterHotkey(chord, TestMethod);

                //---Assert
                //Try twice, so we see we can chain hotkeys
                for (int i = 0; i < 2; i++) {
                    m_counter = 0;
                    //Keys.A alone is not a hotkey...
                    Assert.That(m_counter, Is.EqualTo(0));
                    hc.OnKeyDown(sender: null, e: new KeyEventArgs(Keys.A));
                    Assert.That(m_counter, Is.EqualTo(0));
                    hc.OnKeyUp(sender: null, e: new KeyEventArgs(Keys.A));
                    Assert.That(m_counter, Is.EqualTo(0));

                    //But with Keys.B and modifier it is
                    hc.OnKeyDown(sender: null, e: new KeyEventArgs(modifier));
                    Assert.That(m_counter, Is.EqualTo(0));
                    hc.OnKeyDown(sender: null, e: new KeyEventArgs(Keys.B));
                    Assert.That(m_counter, Is.EqualTo(1), $"{modifier}");
                    hc.OnKeyUp(sender: null, e: new KeyEventArgs(Keys.B));
                    Assert.That(m_counter, Is.EqualTo(1));
                    hc.OnKeyUp(sender: null, e: new KeyEventArgs(modifier));
                    Assert.That(m_counter, Is.EqualTo(1));
                }
            }
        }

        [Test]
        public void ShouldRegisterHotkeyWithModifiers() {
            //--- Assemble
            var hc = new HotkeyCollectionInternal();
            const Keys key = Keys.A | Keys.Control | Keys.Alt;

            //--- Act
            hc.RegisterHotkey(key, TestMethod);

            //---Assert

            //Just Keys.A is not a hotkey without modifiers
            Assert.That(m_counter, Is.EqualTo(0));
            hc.OnKeyDown(sender: null, e: new KeyEventArgs(Keys.A));
            Assert.That(m_counter, Is.EqualTo(0));
            hc.OnKeyUp(sender: null, e: new KeyEventArgs(Keys.A));
            Assert.That(m_counter, Is.EqualTo(0));

            //Keys.A and just Control is not a hotkey
            hc.OnKeyDown(sender: null, e: new KeyEventArgs(Keys.Control));
            hc.OnKeyDown(sender: null, e: new KeyEventArgs(Keys.A));
            Assert.That(m_counter, Is.EqualTo(0));
            hc.OnKeyUp(sender: null, e: new KeyEventArgs(Keys.A));
            hc.OnKeyUp(sender: null, e: new KeyEventArgs(Keys.Control));
            Assert.That(m_counter, Is.EqualTo(0));

            //Keys.A and just Alt is not a hotkey
            hc.OnKeyDown(sender: null, e: new KeyEventArgs(Keys.Alt));
            hc.OnKeyDown(sender: null, e: new KeyEventArgs(Keys.A));
            Assert.That(m_counter, Is.EqualTo(0));
            hc.OnKeyUp(sender: null, e: new KeyEventArgs(Keys.A));
            hc.OnKeyUp(sender: null, e: new KeyEventArgs(Keys.Alt));
            Assert.That(m_counter, Is.EqualTo(0));

            //With Control and Alt Keys.A is a hotkey. Sequence1 of pressing and releasing ctrl and alt
            hc.OnKeyDown(sender: null, e: new KeyEventArgs(Keys.Control));
            hc.OnKeyDown(sender: null, e: new KeyEventArgs(Keys.Alt));
            hc.OnKeyDown(sender: null, e: new KeyEventArgs(Keys.A));
            Assert.That(m_counter, Is.EqualTo(1), "Sequence1 failed");
            hc.OnKeyUp(sender: null, e: new KeyEventArgs(Keys.A));
            hc.OnKeyUp(sender: null, e: new KeyEventArgs(Keys.Alt));
            hc.OnKeyUp(sender: null, e: new KeyEventArgs(Keys.Control));
            Assert.That(m_counter, Is.EqualTo(1), "Sequence1 failed");

            //With Control and Alt Keys.A is a hotkey. Sequence2 of pressing and releasing ctrl and alt
            hc.OnKeyDown(sender: null, e: new KeyEventArgs(Keys.Alt));
            hc.OnKeyDown(sender: null, e: new KeyEventArgs(Keys.Control));
            hc.OnKeyDown(sender: null, e: new KeyEventArgs(Keys.A));
            Assert.That(m_counter, Is.EqualTo(2), "Sequence2 failed");
            hc.OnKeyUp(sender: null, e: new KeyEventArgs(Keys.A));
            hc.OnKeyUp(sender: null, e: new KeyEventArgs(Keys.Alt));
            hc.OnKeyUp(sender: null, e: new KeyEventArgs(Keys.Control));
            Assert.That(m_counter, Is.EqualTo(2), "Sequence2 failed");

            //With Control and Alt Keys.A is a hotkey. Sequence3 of pressing and releasing ctrl and alt
            hc.OnKeyDown(sender: null, e: new KeyEventArgs(Keys.Control));
            hc.OnKeyDown(sender: null, e: new KeyEventArgs(Keys.Alt));
            hc.OnKeyDown(sender: null, e: new KeyEventArgs(Keys.A));
            Assert.That(m_counter, Is.EqualTo(3), "Sequence3 failed");
            hc.OnKeyUp(sender: null, e: new KeyEventArgs(Keys.A));
            hc.OnKeyUp(sender: null, e: new KeyEventArgs(Keys.Control));
            hc.OnKeyUp(sender: null, e: new KeyEventArgs(Keys.Alt));
            Assert.That(m_counter, Is.EqualTo(3), "Sequence3 failed");

            //With Control and Alt Keys.A is a hotkey. Sequence4 of pressing and releasing ctrl and alt
            hc.OnKeyDown(sender: null, e: new KeyEventArgs(Keys.Alt));
            hc.OnKeyDown(sender: null, e: new KeyEventArgs(Keys.Control));
            hc.OnKeyDown(sender: null, e: new KeyEventArgs(Keys.A));
            Assert.That(m_counter, Is.EqualTo(4), "Sequence4 failed");
            hc.OnKeyUp(sender: null, e: new KeyEventArgs(Keys.A));
            hc.OnKeyUp(sender: null, e: new KeyEventArgs(Keys.Control));
            hc.OnKeyUp(sender: null, e: new KeyEventArgs(Keys.Alt));
            Assert.That(m_counter, Is.EqualTo(4), "Sequence4 failed");
        }

        /// <summary>
        /// Test that it is possible to add and remove modifiers from any key
        /// </summary>
        [Test]
        public void ShouldRemoveModifiersFromKey() {
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
        public void ShouldReturnAllHotkeysInHumanReadableForm() {
            //--- Assemble
            var hc = new HotkeyCollectionInternal();
            const Keys keyA = Keys.A;
            const Keys keyB = Keys.B;
            const Keys ctrlA = Keys.A | Keys.Control;

            //--- Act

            hc.RegisterHotkey(new Keys[] { keyA, keyB }, TestMethod);
            hc.RegisterHotkey(new Keys[] { keyB }, TestMethod, "desc B");
            hc.RegisterHotkey(new Keys[] { ctrlA, ctrlA, ctrlA }, TestMethod, "desc 3xCtrl+A");
            hc.RegisterHotkey(new Keys[] { keyA }, TestMethod, "desc A");

            //---Assert
            var desc = hc.ToString();

            var expected = "Registered Hotkeys:" + Environment.NewLine +
                           "- a (desc A)" + Environment.NewLine +
                           "- b (desc B)" + Environment.NewLine +
                           "- a, b" + Environment.NewLine +
                           "- Control+a, Control+a, Control+a (desc 3xCtrl+A)";

            Assert.That(desc, Is.EqualTo(expected));
            Console.WriteLine(desc);
        }

        [Test]
        public void ShouldTriggerChordWithDifferentPositionsPressingAndReleasingModifiers() {
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
        public void ShouldTriggerTwoSimpleHotkeyConsecutively() {
            //--- Assemble
            var hc = new HotkeyCollectionInternal();

            hc.RegisterHotkey(Keys.C | Keys.Control, eventArgs => m_counter++);
            hc.RegisterHotkey(Keys.V | Keys.Control, eventArgs => m_counter++);

            //--- Act

            //press Control
            hc.OnKeyDown(null, new KeyEventArgs(Keys.Control));
            Assert.That(m_counter, Is.EqualTo(0));

            //Press C => ctrl+C should trigger
            hc.OnKeyDown(null, new KeyEventArgs(Keys.C));
            Assert.That(m_counter, Is.EqualTo(1));

            //Press V => ctrl+V should trigger
            hc.OnKeyDown(null, new KeyEventArgs(Keys.V));
            Assert.That(m_counter, Is.EqualTo(2));
        }

        [Test]
        public void ShouldUseHotkeysInTypicalSituation([Values(true, false)] bool pad) {
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

        private void TestMethod(HotKeyEventArgs args) {
            m_counter++;
            Console.WriteLine("Test");
        }
    }
}