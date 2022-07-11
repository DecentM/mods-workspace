using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

namespace DecentM.Keyboard.Plugins
{
    [UdonBehaviourSyncMode(BehaviourSyncMode.None)]
    public class BacklightPlugin : KeyboardPlugin
    {
        public float fadeTime = 2f;
        public Color toggleActiveColour;

        protected override void _Start()
        {
            if (this.toggleActiveColour == null)
                this.toggleActiveColour = Color.white;
        }

        private Color RandomColour()
        {
            return new Color(
                Random.Range(0, 255) / 255f,
                Random.Range(0, 255) / 255f,
                Random.Range(0, 255) / 255f,
                1
            );
        }

        private void HandleKeyStateChange(string symbol, bool state, KeyboardLayout layout)
        {
            KeyboardKey[] keysWithSameSymbol = layout.GetKeysByPrimarySymbol(symbol);

            if (keysWithSameSymbol == null)
                return;

            foreach (KeyboardKey key in keysWithSameSymbol)
            {
                key.ChangeBacklight(state ? this.toggleActiveColour : Color.black);
            }
        }

        protected override void OnKeyPressDown(KeyboardKey key)
        {
            if (key.isToggle)
                return;

            key.ChangeBacklight(this.RandomColour());
            key.ChangeBacklightAfterSeconds(Color.black, this.fadeTime);
        }

        protected override void OnShiftStateChange(bool state, KeyboardLayout layout)
        {
            this.HandleKeyStateChange("SHIFT", state, layout);
        }

        protected override void OnCtrlStateChange(bool state, KeyboardLayout layout)
        {
            this.HandleKeyStateChange("CTRL", state, layout);
        }

        protected override void OnAltGrStateChange(bool state, KeyboardLayout layout)
        {
            this.HandleKeyStateChange("ALTGR", state, layout);
        }
    }
}
