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

using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Windows.Forms;
using Dfust.Hotkeys.Util;
using NUnit.Framework;

namespace Dfust.Hotkeys.Tests {

    [TestFixture, ExcludeFromCodeCoverage]
    public class KeyUpDownCleanerTests {
        private string m_output;

        [Test]
        public void ShouldIgnoreMultipleKeyDownOfSameKey([Values(1, 2, 50)] int times) {
            //--- Assemble

            var cleaner = Setup();

            //--- Act

            for (int t = 0; t < times; t++) {
                for (int i = 0; i < 10; i++) {
                    cleaner.OnKeyDown(null, new KeyEventArgs(Keys.A));
                }
                cleaner.OnKeyUp(null, new KeyEventArgs(Keys.A));
            }

            //---Assert
            var expected = string.Join("", Enumerable.Repeat("↓A↑A", times));
            Assert.That(m_output, Is.EqualTo(expected));
        }

        [Test]
        public void ShouldIgnoreMultipleKeyDownOfSameKeyWithModifiers([Values(1, 2, 50)] int times) {
            //--- Assemble

            var cleaner = Setup();

            var modifiers = new Keys[] { Keys.Control, Keys.Shift, Keys.Alt };

            var modPermutations = SetOperations.GetPowerset(modifiers);
            var useModifiers = modPermutations.Select(item => item.Aggregate(Keys.None, (a, b) => a | b));

            //--- Act

            for (int t = 0; t < times; t++) {
                foreach (var item in useModifiers) {
                    cleaner.OnKeyDown(null, new KeyEventArgs(Keys.A | item));
                }
                cleaner.OnKeyUp(null, new KeyEventArgs(Keys.A));
            }

            //---Assert
            var expected = string.Join("", Enumerable.Repeat("↓A↑A", times));
            Assert.That(m_output, Is.EqualTo(expected));
        }

        [Test]
        public void ShouldRegisterDownUpCorrectly() {
            //--- Assemble

            var cleaner = Setup();

            //--- Act

            cleaner.OnKeyDown(null, new KeyEventArgs(Keys.A));
            cleaner.OnKeyUp(null, new KeyEventArgs(Keys.A));

            //---Assert

            Assert.That(m_output, Is.EqualTo("↓A↑A"));
        }

        [Test]
        public void ShouldReplaceModifierKeysWithModifierFlagAndDeleteRepetitions() {
            //--- Assemble
            var cleaner = Setup();
            var modifiers = new[]        { new {ToReplace= Keys.LControlKey,Replacement= Keys.Control , ModifyWithReplacement=true}, //Shift
                                           new {ToReplace= Keys.RControlKey,Replacement= Keys.Control, ModifyWithReplacement=true },
                                           new {ToReplace= Keys.LShiftKey,Replacement= Keys.Shift , ModifyWithReplacement=true},     //Control
                                           new {ToReplace= Keys.RShiftKey,Replacement= Keys.Shift, ModifyWithReplacement=true },
                                           new {ToReplace= Keys.LMenu,Replacement= Keys.Alt, ModifyWithReplacement=true },           //Alt
                                           new {ToReplace= Keys.RMenu,Replacement= Keys.Alt , ModifyWithReplacement=true}  ,
                                           new {ToReplace= Keys.LWin,Replacement= Keys.LWin, ModifyWithReplacement=false },          //Win
                                           new {ToReplace= Keys.RWin,Replacement= Keys.LWin , ModifyWithReplacement=false}};

            foreach (var item in modifiers) {
                m_output = "";
                //--- Act
                //we simulate a key press by hand. The first occurrence is just the key itself...
                cleaner.OnKeyDown(null, new KeyEventArgs(item.ToReplace));

                for (int i = 0; i < 10; i++) {
                    //... any further key press is the key AND it's modifier
                    cleaner.OnKeyDown(null, new KeyEventArgs(item.ToReplace | (item.ModifyWithReplacement ? item.Replacement : Keys.None)));
                }

                cleaner.OnKeyUp(null, new KeyEventArgs(item.ToReplace | (item.ModifyWithReplacement ? item.Replacement : Keys.None)));

                //---Assert
                Assert.That(m_output, Is.EqualTo($"↓{item.Replacement}↑{item.Replacement}"));
            }
        }

        [Test]
        public void ShouldReplaceModifierKeysWithModifierFlagAndDeleteRepetitions_AlternatingKeys([Values(true, false)] bool toggle) {
            //--- Assemble
            var cleaner = Setup();
            var modifiers = new[]  { new {Key1= Keys.LControlKey, Key2=Keys.RControlKey,Replacement= Keys.Control, ModifyWithReplacement=true }, //Control
                                     new {Key1= Keys.RControlKey, Key2=Keys.LControlKey,Replacement= Keys.Control, ModifyWithReplacement=true },
                                     new {Key1= Keys.LShiftKey, Key2=Keys.RShiftKey,Replacement= Keys.Shift , ModifyWithReplacement=true},       //Shift
                                     new {Key1= Keys.RShiftKey, Key2=Keys.LShiftKey,Replacement= Keys.Shift, ModifyWithReplacement=true },
                                     new {Key1= Keys.LMenu, Key2=Keys.RMenu,Replacement= Keys.Alt , ModifyWithReplacement=true},                 //Alt
                                     new {Key1= Keys.RMenu, Key2=Keys.LMenu,Replacement= Keys.Alt, ModifyWithReplacement=true}  ,
                                     new {Key1= Keys.LWin, Key2=Keys.RWin,Replacement= Keys.LWin , ModifyWithReplacement=false},                 //Win
                                     new {Key1= Keys.RWin, Key2=Keys.LWin,Replacement= Keys.LWin, ModifyWithReplacement=false}
            };

            foreach (var item in modifiers) {
                m_output = "";
                //--- Act
                //we simulate a key press by hand. The first occurrence is just the key itself...
                cleaner.OnKeyDown(null, new KeyEventArgs(item.Key1));

                for (int i = 0; i < 10; i++) {
                    //... any further key press is the key AND it's modifier
                    cleaner.OnKeyDown(null, new KeyEventArgs(item.Key1 | (item.ModifyWithReplacement ? item.Replacement : Keys.None)));
                }

                //then we simulate multiple key presses of the other key for the modifier
                for (int i = 0; i < 10; i++) {
                    cleaner.OnKeyDown(null, new KeyEventArgs(item.Key2 | (item.ModifyWithReplacement ? item.Replacement : Keys.None)));
                }

                //Then we release the two keys. Both options are tried:
                if (toggle) {
                    cleaner.OnKeyUp(null, new KeyEventArgs(item.Key1 | (item.ModifyWithReplacement ? item.Replacement : Keys.None)));
                    cleaner.OnKeyUp(null, new KeyEventArgs(item.Key2 | (item.ModifyWithReplacement ? item.Replacement : Keys.None)));
                } else {
                    cleaner.OnKeyUp(null, new KeyEventArgs(item.Key2 | (item.ModifyWithReplacement ? item.Replacement : Keys.None)));
                    cleaner.OnKeyUp(null, new KeyEventArgs(item.Key1 | (item.ModifyWithReplacement ? item.Replacement : Keys.None)));
                }

                //---Assert
                Assert.That(m_output, Is.EqualTo($"↓{item.Replacement}↑{item.Replacement}"));
            }
        }

        private void OnKeyDown(object sender, KeyEventArgs e) {
            m_output += ($@"↓{e.KeyData}");
        }

        private void OnKeyUp(object sender, KeyEventArgs e) {
            m_output += ($@"↑{e.KeyData}");
        }

        private KeyUpDownCleaner Setup() {
            m_output = "";
            var cleaner = new KeyUpDownCleaner();
            cleaner.KeyDown += OnKeyDown;
            cleaner.KeyUp += OnKeyUp;
            return cleaner;
        }
    }
}