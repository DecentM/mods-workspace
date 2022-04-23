using System;
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

namespace DecentM.Keyboard
{
    [UdonBehaviourSyncMode(BehaviourSyncMode.None)]
    public class KeyboardEvents : UdonSharpBehaviour
    {
        [HideInInspector]
        public UdonSharpBehaviour[] subscribers;

        private void Start()
        {
            if (this.subscribers == null) this.subscribers = new UdonSharpBehaviour[0];
        }

        public int Subscribe(UdonSharpBehaviour behaviour)
        {
            bool initialised = this.subscribers != null;

            if (initialised)
            {
                UdonSharpBehaviour[] tmp = new UdonSharpBehaviour[this.subscribers.Length + 1];
                Array.Copy(this.subscribers, 0, tmp, 0, this.subscribers.Length);
                tmp[tmp.Length - 1] = behaviour;
                this.subscribers = tmp;
            }
            else
            {
                UdonSharpBehaviour[] tmp = new UdonSharpBehaviour[1];
                tmp[0] = behaviour;
                this.subscribers = tmp;
            }

            return this.subscribers.Length - 1;
        }

        public bool Unsubscribe(int index)
        {
            if (this.subscribers == null || this.subscribers.Length == 0 || index < 0 || index >= this.subscribers.Length) return false;

            UdonSharpBehaviour[] tmp = new UdonSharpBehaviour[subscribers.Length + 1];
            Array.Copy(this.subscribers, 0, tmp, 0, index);
            Array.Copy(this.subscribers, index + 1, tmp, index, this.subscribers.Length - 1 - index);
            this.subscribers = tmp;

            return true;
        }

        private void BroadcastEvent(string eventName, object[] data)
        {
            foreach (UdonSharpBehaviour subscriber in this.subscribers)
            {
                subscriber.SetProgramVariable($"OnKeyboardEvent_name", eventName);
                subscriber.SetProgramVariable($"OnKeyboardEvent_data", data);
                subscriber.SendCustomEvent("OnKeyboardEvent");
            }
        }

        #region Keypresses

        public void OnKeyPressDown(KeyboardKey key)
        {
            this.BroadcastEvent(nameof(OnKeyPressDown), new object[] { key });
        }

        public void OnKeyPressUp(KeyboardKey key)
        {
            this.BroadcastEvent(nameof(OnKeyPressUp), new object[] { key });
        }

        public void OnSymbolEntry()
        {
            this.BroadcastEvent(nameof(OnSymbolEntry), new object[0]);
        }

        public void OnSymbolEntry(string symbol)
        {
            this.BroadcastEvent(nameof(OnSymbolEntry), new object[] { symbol });
        }

        public void OnBackspace()
        {
            this.BroadcastEvent(nameof(OnBackspace), new object[0]);
        }

        #endregion

        #region State changes

        public void OnShiftStateChange(bool state, KeyboardLayout layout)
        {
            this.BroadcastEvent(nameof(OnShiftStateChange), new object[] { state, layout });
        }

        public void OnCtrlStateChange(bool state, KeyboardLayout layout)
        {
            this.BroadcastEvent(nameof(OnCtrlStateChange), new object[] { state, layout });
        }

        public void OnAltGrStateChange(bool state, KeyboardLayout layout)
        {
            this.BroadcastEvent(nameof(OnAltGrStateChange), new object[] { state, layout });
        }

        #endregion

        #region CTRL Commands

        public void OnCommandClear()
        {
            this.BroadcastEvent(nameof(OnCommandClear), new object[0]);
        }

        public void OnCommandClipboardCut()
        {
            this.BroadcastEvent(nameof(OnCommandClipboardCut), new object[0]);
        }

        public void OnCommandClipboardPaste()
        {
            this.BroadcastEvent(nameof(OnCommandClipboardPaste), new object[0]);
        }

        public void OnCommandClipboardCopy()
        {
            this.BroadcastEvent(nameof(OnCommandClipboardCopy), new object[0]);
        }

        public void OnCommandUndo()
        {
            this.BroadcastEvent(nameof(OnCommandUndo), new object[0]);
        }

        public void OnCommandRedo()
        {
            this.BroadcastEvent(nameof(OnCommandRedo), new object[0]);
        }

        public void OnCommandQuit()
        {
            this.BroadcastEvent(nameof(OnCommandQuit), new object[0]);
        }

        public void OnCommandCloseTab()
        {
            this.BroadcastEvent(nameof(OnCommandCloseTab), new object[0]);
        }

        public void OnCommandRefresh()
        {
            this.BroadcastEvent(nameof(OnCommandRefresh), new object[0]);
        }

        public void OnCommandOpen()
        {
            this.BroadcastEvent(nameof(OnCommandOpen), new object[0]);
        }

        public void OnCommandPrint()
        {
            this.BroadcastEvent(nameof(OnCommandPrint), new object[0]);
        }

        public void OnCommandSelectAll()
        {
            this.BroadcastEvent(nameof(OnCommandSelectAll), new object[0]);
        }

        public void OnCommandSave()
        {
            this.BroadcastEvent(nameof(OnCommandSave), new object[0]);
        }

        public void OnCommandDuplicate()
        {
            this.BroadcastEvent(nameof(OnCommandDuplicate), new object[0]);
        }

        public void OnCommandSearch()
        {
            this.BroadcastEvent(nameof(OnCommandSearch), new object[0]);
        }

        public void OnCommandHistory()
        {
            this.BroadcastEvent(nameof(OnCommandHistory), new object[0]);
        }

        public void OnCommandNew()
        {
            this.BroadcastEvent(nameof(OnCommandNew), new object[0]);
        }

        public void OnCommandMap()
        {
            this.BroadcastEvent(nameof(OnCommandMap), new object[0]);
        }

        public void OnCommandQuickAction()
        {
            this.BroadcastEvent(nameof(OnCommandQuickAction), new object[0]);
        }

        public void OnCommandNext()
        {
            this.BroadcastEvent(nameof(OnCommandNext), new object[0]);
        }

        #endregion

        #region Plugin Events

        public void OnInputSubmit(string input)
        {
            this.BroadcastEvent(nameof(OnInputSubmit), new object[] { input });
        }

        #endregion
    }
}
