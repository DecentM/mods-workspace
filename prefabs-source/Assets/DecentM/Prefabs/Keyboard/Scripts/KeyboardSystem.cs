using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

namespace DecentM.Keyboard
{
    [UdonBehaviourSyncMode(BehaviourSyncMode.None)]
    public class KeyboardSystem : UdonSharpBehaviour
    {
        private KeyboardLayout[] layouts;

        public string defaultLayout = "en_US";

        void Start()
        {
            this.layouts = this.GetComponentsInChildren<KeyboardLayout>();

            foreach (KeyboardLayout layout in this.layouts)
            {
                layout.SetProgramVariable("system", this);

                if (layout.layoutName == this.defaultLayout)
                    layout.gameObject.SetActive(true);
                else
                    layout.gameObject.SetActive(false);
            }
        }

        public bool ActivateLayout(string layoutName)
        {
            if (this.layouts == null || this.layouts.Length == 0)
                return false;

            bool success = false;

            foreach (KeyboardLayout layout in this.layouts)
            {
                if (layout.layoutName == layoutName)
                {
                    layout.gameObject.SetActive(true);
                    success = true;
                }
                else
                {
                    layout.gameObject.SetActive(false);
                }
            }

            return success;
        }

        public bool DeactivateLayout(string layoutName)
        {
            if (this.layouts == null || this.layouts.Length == 0)
                return false;

            foreach (KeyboardLayout layout in this.layouts)
            {
                if (layout.layoutName == layoutName)
                {
                    layout.gameObject.SetActive(false);
                    return true;
                }
            }

            return false;
        }
    }
}
