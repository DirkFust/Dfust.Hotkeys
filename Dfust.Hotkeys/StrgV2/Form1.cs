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
using System.Windows.Forms;
using Dfust.Hotkeys;
using Gma.System.MouseKeyHook;
using static Dfust.Hotkeys.Enums;

namespace StrgV2 {

    public partial class Form1 : Form {
        private readonly HotkeyCollection m_hotkey;
        private KeyUpDownCleaner m_cleaner;
        private IKeyboardMouseEvents m_globalHook;

        public Form1() {
            InitializeComponent();
            m_hotkey = new HotkeyCollection(Scope.Application);
            m_hotkey.RegisterHotkey(Keys.G | Keys.Control, (e) => MessageBox.Show($"hello {e.Keys}"), "hello");
            Subscribe();
        }

        private void buttonClear_Click(object sender, EventArgs e) {
            txtLogFiltered.Clear();
            txtLogUnfiltered.Clear();
        }

        private void buttonStart_Click(object sender, EventArgs e) {
            m_hotkey.StartListening();
        }

        private void buttonStop_Click(object sender, EventArgs e) {
            m_hotkey.StopListening();
        }

        private void GlobalHookFiltered_KeyDown(object sender, KeyEventArgs e) {
            var keyData = e.KeyData;
            //if (keyData == (Keys.V | keyData)) {
            //    if (e.Control) {
            //        e.Handled = true;
            //    }
            txtLogFiltered.AppendText(Environment.NewLine + "" + $@"↓ {keyData}");
            //}
        }

        private void GlobalHookFiltered_KeyUp(object sender, KeyEventArgs e) {
            var keyData = e.KeyData;
            //if (keyData == (Keys.V | keyData)) {
            //    if (e.Control) {
            //        e.Handled = true;
            //    }
            txtLogFiltered.AppendText(Environment.NewLine + "" + $@"↑ {keyData}");
            //}
        }

        private void GlobalHookUnfiltered_KeyDown(object sender, KeyEventArgs e) {
            var keyData = e.KeyData;
            //if (keyData == (Keys.V | keyData)) {
            //    if (e.Control) {
            //        e.Handled = true;
            //    }
            txtLogUnfiltered.AppendText(Environment.NewLine + "" + $@"↓ {keyData}");
            //}
        }

        private void GlobalHookUnfiltered_KeyUp(object sender, KeyEventArgs e) {
            var keyData = e.KeyData;
            //if (keyData == (Keys.V | keyData)) {
            //    if (e.Control) {
            //        e.Handled = true;
            //    }
            txtLogUnfiltered.AppendText(Environment.NewLine + "" + $@"↑ {keyData}");
            //}
        }

        private void Subscribe() {
            m_globalHook = Hook.GlobalEvents();
            m_cleaner = new KeyUpDownCleaner();
            m_globalHook.KeyDown += m_cleaner.OnKeyDown;
            m_globalHook.KeyUp += m_cleaner.OnKeyUp;
            m_globalHook.KeyDown += GlobalHookUnfiltered_KeyDown;
            m_globalHook.KeyUp += GlobalHookUnfiltered_KeyUp;

            m_cleaner.KeyDown += GlobalHookFiltered_KeyDown;
            m_cleaner.KeyUp += GlobalHookFiltered_KeyUp;
        }
    }
}