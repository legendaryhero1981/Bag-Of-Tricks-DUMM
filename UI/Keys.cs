using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using GL = UnityEngine.GUILayout;


namespace BagOfTricks.ModUI {
    static class Keys {
        static private KeyCode[] mouseButtonsValid = { KeyCode.Mouse3, KeyCode.Mouse4, KeyCode.Mouse5, KeyCode.Mouse6 };
        public static void SetKeyBinding(ref KeyCode keyCode) {
            string label = (keyCode == KeyCode.None) ? Strings.GetText("button_PressKey") : keyCode.ToString();
            if (GL.Button(label, GL.ExpandWidth(false))) {
                keyCode = KeyCode.None;
            }
            if (keyCode == KeyCode.None && Event.current != null) {
                if (Event.current.isKey) {
                    keyCode = Event.current.keyCode;
                    Input.ResetInputAxes();
                }
                else {
                    foreach (KeyCode mouseButton in mouseButtonsValid) {
                        if (Input.GetKey(mouseButton)) {
                            keyCode = mouseButton;
                            Input.ResetInputAxes();
                        }
                    }
                }
            }
        }
    }
}
