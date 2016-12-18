# Dfust.Hotkeys
.Net Hotkey application for hotkeys (single key) and chords (multi key) shortcuts

```c#
//create a new hotkey collection for the application.
var hotkeyCollection = new HotkeyCollection(Scope.Application); //Scope.Global would tell the HotkeyCollection to listen for keystrokes globally

//Now let's create a hotkey with a single key.

//Define a key, in this case ctrl+G.
var hotkey = Keys.G | Keys.Control; //More modifiers could be added, for example "Keys.G | Keys.Control | Keys.Shift | Keys.Alt"

//Register the hotkey. In this case triggering the hotkey will show a message box.
hotkeyCollection.RegisterHotkey(hotkey, (e) => MessageBox.Show($"hello {e.Keys.First()}"));

//Now let's crate a chord, a hotkey consisting of a sequence of keys

//Define the chord. In this case we want to define "ctrl+k, ctrl+c", a chord that is used in Visual Studio to comment the selected text
var chord = new Keys[] { Keys.K | Keys.Control, Keys.C | Keys.Control };

//Register the chord. In this case triggering will call the OnChord function.
hotkeyCollection.RegisterHotkey(hotkey, OnChordTriggered);

//Finally: Dispose hotkey collection
hotkeyCollection.Dispose();        

/// <summary>
/// Handles the "ctrl+k, ctrl+c" hotkey.
/// </summary>
/// <param name="e">The HotKeyEventArgs instance containing the event data.</param>
private static void OnChordTriggered(HotKeyEventArgs e) {            
   var keys = e.Keys; //Which key (sequence) has triggered the hotkey?
   var count = e.Count; //How many times has this hotkey been triggered without any other triggered hotkeys in between?
   var sender = e.Sender; //Where did the KeyEventArg that was used in the HotkeyCollection originate from?      
            
   //Do something here!
}
```
