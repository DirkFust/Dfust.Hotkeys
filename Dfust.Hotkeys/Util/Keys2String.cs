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
using System.Runtime.InteropServices;
using System.Text;
using System.Windows.Forms;

namespace Dfust.Hotkeys.Util {

    public static class Keys2String {

        /// <summary>
        /// Returns a human readable textual representation of a hotkey (single key+modifiers) or
        /// chord (multiple keys+modifiers).
        /// </summary>
        /// <param name="chord">The hotkey/chord.</param>
        /// <returns></returns>
        public static string ChordToString(IEnumerable<Keys> chord) {
            var sb = new StringBuilder();
            var first = true;
            foreach (var key in chord) {
                if (!first) {
                    sb.Append(", ");
                }

                sb.Append(KeyToString(key));
                first = false;
            }
            return sb.ToString();
        }

        //Declare Function ToAscii Lib "user32" (ByVal uVirtKey As Integer, ByVal uScanCode As Integer, ByRef lpbKeyState As Byte, ByRef lpwTransKey As  Integer, ByVal fuState As Integer) As Integer
        //Private Declare Function GetKeyboardState Lib "user32.dll" (ByRef pbKeyState As Byte) As Long
        /// <summary>
        /// Returns a human readable textual representation of a hotkey (key+modifiers).
        /// </summary>
        /// <param name="key">The hotkey.</param>
        /// <returns></returns>
        public static string KeyToString(Keys key) {
            var sb = new StringBuilder();
            var modifiers = new Keys[] { Keys.Control, Keys.Alt, Keys.Shift };

            foreach (var modifier in modifiers) {
                if ((key & modifier) != 0) {
                    sb.Append(modifier);
                    sb.Append("+");
                }
            }

            //Make sure all modifiers are set
            var allModifiersSet = key | Keys.Control | Keys.Alt | Keys.Shift;
            //dump all modifiers via XOR so we just get the non-modifier key
            var nonModifierKey = allModifiersSet ^ Keys.Control ^ Keys.Alt ^ Keys.Shift;

            //on a German keyboard the key labeled "ü" produces "Keys.OEM1" as key event.
            //we don't want the name to be OEM1, so we translate it back
            var keyboardRepresentation = KeyToUTF8(nonModifierKey);
            sb.Append(keyboardRepresentation);

            return sb.ToString();
        }

        /// <summary>
        /// Converts a key to it's representation on the keyboard. For example, on a German keyboard
        /// the key labeled "ü" produces "Keys.OEM1" as key event.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <returns></returns>
        private static string KeyToUTF8(Keys key) {
            var buf = new StringBuilder(256);
            var keyboardState = new byte[256];
            //if ((key & Keys.Alt) != 0) {
            //    keyboardState[(int)Keys.ControlKey] = 0xff;
            //    keyboardState[(int)Keys.Menu] = 0xff;
            //}
            ToUnicode((uint)key, 0, keyboardState, buf, 256, 0);
            return buf.ToString();
        }

        /// <summary>
        /// Converts .
        /// </summary>
        /// <param name="virtualKeyCode">The virtual key code.</param>
        /// <param name="scanCode">The scan code.</param>
        /// <param name="keyboardState">State of the keyboard.</param>
        /// <param name="receivingBuffer">The receiving buffer.</param>
        /// <param name="bufferSize">Size of the buffer.</param>
        /// <param name="flags">The flags.</param>
        /// <returns></returns>
        [DllImport("user32.dll")]
        private static extern int ToUnicode(uint virtualKeyCode,
                                           uint scanCode,
                                           byte[] keyboardState,
                                           [Out, MarshalAs(UnmanagedType.LPWStr, SizeConst = 64)] StringBuilder receivingBuffer,
                                           int bufferSize,
                                           uint flags);
    }
}