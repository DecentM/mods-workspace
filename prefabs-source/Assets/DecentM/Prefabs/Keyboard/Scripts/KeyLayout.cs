
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;
using TMPro;

namespace DecentM.Keyboard
{
    [UdonBehaviourSyncMode(BehaviourSyncMode.None)]
    public class KeyLayout : UdonSharpBehaviour
    {
        public TextMeshProUGUI primarySlot;
        public TextMeshProUGUI secondarySlot;
        public TextMeshProUGUI tertiarySlot;

        private bool SetSlotText(TextMeshProUGUI slot, string text)
        {
            if (slot == null) return false;

            slot.text = text;
            return true;
        }

        public bool SetSymbols(string primary, string secondary, string tertiary)
        {
            if (primary != null && primary != "")
            {
                if (!this.SetSlotText(this.primarySlot, primary)) return false;
            }

            if (secondary != null && secondary != "")
            {
                if (!this.SetSlotText(this.secondarySlot, secondary)) return false;
            }

            if (tertiary != null && tertiary != "")
            {
                if (!this.SetSlotText(this.tertiarySlot, tertiary)) return false;
            }

            return true;
        }

        private bool SetSlotColour(TextMeshProUGUI slot, Color colour)
        {
            if (slot == null) return false;

            slot.color = colour;
            return true;
        }

        public bool SetColours(Color primary, Color secondary, Color tertiary)
        {
            if (primary != null)
            {
                if (!this.SetSlotColour(this.primarySlot, primary)) return false;
            }

            if (secondary != null)
            {
                if (!this.SetSlotColour(this.secondarySlot, secondary)) return false;
            }

            if (tertiary != null)
            {
                if (!this.SetSlotColour(this.tertiarySlot, tertiary)) return false;
            }

            return true;
        }
    }
}

