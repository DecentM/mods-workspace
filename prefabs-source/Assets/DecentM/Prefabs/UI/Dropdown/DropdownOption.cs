
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;
using TMPro;

namespace DecentM.UI
{
    [UdonBehaviourSyncMode(BehaviourSyncMode.None)]
    public class DropdownOption : UdonSharpBehaviour
    {
        public TextMeshProUGUI labelSlot;

        private Dropdown dropdown;
        private object value;

        public void SetData(Dropdown dropdown, object value, string label)
        {
            this.dropdown = dropdown;
            this.labelSlot.text = label;
            this.value = value;
        }

        public void OnClick()
        {
            if (this.dropdown == null) return;

            this.dropdown.OnValueClick(this.value);
        }
    }
}
