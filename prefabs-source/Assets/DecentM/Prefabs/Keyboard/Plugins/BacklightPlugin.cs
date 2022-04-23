
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

namespace DecentM.Keyboard.Plugins
{
    [UdonBehaviourSyncMode(BehaviourSyncMode.None)]
    public class BacklightPlugin : UdonSharpBehaviour
    {
        public KeyboardEvents events;
        public float fadeTime = 2f;
        public Color toggleActiveColour;

        void Start()
        {
            this.events.Subscribe(this);
            if (this.toggleActiveColour == null) this.toggleActiveColour = Color.white;
        }

        private Color RandomColour()
        {
            return new Color(
                UnityEngine.Random.Range(0, 255) / 255f,
                UnityEngine.Random.Range(0, 255) / 255f,
                UnityEngine.Random.Range(0, 255) / 255f,
                1
            );
        }

        private void HandleKeyPress(KeyboardKey key)
        {
            if (key.isToggle) return;

            key.ChangeBacklight(this.RandomColour());
            key.ChangeBacklightAfterSeconds(Color.black, this.fadeTime);
        }

        private void HandleKeyStateChange(string symbol, bool state, KeyboardLayout layout)
        {
            KeyboardKey[] keysWithSameSymbol = layout.GetKeysByPrimarySymbol(symbol);

            if (keysWithSameSymbol == null) return;

            foreach (KeyboardKey key in keysWithSameSymbol)
            {
                key.ChangeBacklight(state ? this.toggleActiveColour : Color.black);
            }
        }

        private string OnKeyboardEvent_name;
        private object[] OnKeyboardEvent_data;
        public void OnKeyboardEvent()
        {
            if (OnKeyboardEvent_name == null || OnKeyboardEvent_data == null) return;

            switch (OnKeyboardEvent_name)
            {
                case nameof(this.events.OnKeyPressDown):
                    this.HandleKeyPress((KeyboardKey) OnKeyboardEvent_data[0]);
                    break;

                case nameof(this.events.OnShiftStateChange):
                    this.HandleKeyStateChange("SHIFT", (bool) OnKeyboardEvent_data[0], (KeyboardLayout) OnKeyboardEvent_data[1]);
                    break;
                case nameof(this.events.OnAltGrStateChange):
                    this.HandleKeyStateChange("ALTGR", (bool) OnKeyboardEvent_data[0], (KeyboardLayout) OnKeyboardEvent_data[1]);
                    break;
                case nameof(this.events.OnCtrlStateChange):
                    this.HandleKeyStateChange("CTRL", (bool) OnKeyboardEvent_data[0], (KeyboardLayout) OnKeyboardEvent_data[1]);
                    break;

                default:
                    break;
            }

            this.OnKeyboardEvent_name = null;
            this.OnKeyboardEvent_data = null;
        }
    }
}

