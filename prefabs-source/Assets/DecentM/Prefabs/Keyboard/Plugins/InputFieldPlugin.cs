using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;
using TMPro;
using UnityEngine.UI;

namespace DecentM.Keyboard.Plugins
{
    [UdonBehaviourSyncMode(BehaviourSyncMode.None)]
    public class InputFieldPlugin : UdonSharpBehaviour
    {
        public KeyboardEvents events;
        public InputField field;
        public int maxLength = 64;
        public TextMeshProUGUI lengthHintSlot;
        public Text placeholder;
        public GameObject vrOnlyHint;

        void Start()
        {
            this.events.Subscribe(this);
            this.field.characterLimit = this.maxLength;
            this.field.text = "";
            this.field.ForceLabelUpdate();
            this.RenderLengthHint();

            if (Networking.LocalPlayer == null || !Networking.LocalPlayer.IsValid())
                return;

            if (Networking.LocalPlayer.IsUserInVR())
            {
                this.vrOnlyHint.SetActive(true);
                this.placeholder.text = "Start typing to enter text...";
            }
            else
            {
                this.vrOnlyHint.SetActive(false);
                this.placeholder.text = "Click here and start typing...";
            }
        }

        private void RenderLengthHint()
        {
            this.lengthHintSlot.text = $"{this.field.text.Length} / {this.maxLength}";
        }

        private void HandleEnter()
        {
            this.events.OnInputSubmit(this.field.text);
            this.field.text = "";
            this.field.MoveTextEnd(false);
            this.RenderLengthHint();
            this.field.DeactivateInputField();
        }

        private void HandleSymbolEntry(string symbol)
        {
            if (symbol == "\n")
            {
                this.HandleEnter();
                return;
            }

            if (this.field.text.Length >= maxLength)
                return;

            this.field.ActivateInputField();
            this.field.text += symbol;
            this.field.MoveTextEnd(false);
            this.RenderLengthHint();
        }

        private void HandleBackspace()
        {
            if (this.field.text.Length == 0)
                return;
            this.field.text = this.field.text.Remove(this.field.text.Length - 1, 1);
            this.field.MoveTextEnd(false);
            this.RenderLengthHint();
        }

        private void HandleClear()
        {
            this.field.text = "";
            this.field.MoveTextEnd(false);
            this.RenderLengthHint();
            this.field.DeactivateInputField();
        }

        private string OnKeyboardEvent_name;
        private object[] OnKeyboardEvent_data;

        public void OnKeyboardEvent()
        {
            switch (OnKeyboardEvent_name)
            {
                case nameof(this.events.OnSymbolEntry):
                    this.HandleSymbolEntry((string)OnKeyboardEvent_data[0]);
                    break;

                case nameof(this.events.OnBackspace):
                    this.HandleBackspace();
                    break;

                case nameof(this.events.OnCommandClear):
                    this.HandleClear();
                    break;

                default:
                    break;
            }
        }

        public void OnInputEndEdit()
        {
            this.HandleEnter();
        }

        public void OnInputValueChanged()
        {
            this.RenderLengthHint();

            if (this.field.text == "")
            {
                return;
            }

            this.events.OnSymbolEntry();
        }
    }
}
