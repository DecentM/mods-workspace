using System;
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

namespace DecentM.Keyboard
{
    public enum KeyboardEvent
    {
        // Basics
        OnKeyPressDown,
        OnKeyPressUp,
        OnSymbolEntry,
        OnBackspace,

        // Modifier key changes
        OnShiftStateChange,
        OnCtrlStateChange,
        OnAltGrStateChange,

        // Commands
        OnCommandClear,
        OnCommandClipboardCut,
        OnCommandClipboardPaste,
        OnCommandClipboardCopy,
        OnCommandUndo,
        OnCommandRedo,
        OnCommandQuit,
        OnCommandCloseTab,
        OnCommandRefresh,
        OnCommandOpen,
        OnCommandPrint,
        OnCommandSelectAll,
        OnCommandSave,
        OnCommandDuplicate,
        OnCommandSearch,
        OnCommandHistory,
        OnCommandNew,
        OnCommandMap,
        OnCommandQuickAction,
        OnCommandNext,

        // Plugins
        OnInputSubmit,
    }

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

        private void BroadcastEvent(KeyboardEvent eventName, object[] data)
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
            this.BroadcastEvent(KeyboardEvent.OnKeyPressDown, new object[] { key });
        }

        public void OnKeyPressUp(KeyboardKey key)
        {
            this.BroadcastEvent(KeyboardEvent.OnKeyPressUp, new object[] { key });
        }

        public void OnSymbolEntry()
        {
            this.BroadcastEvent(KeyboardEvent.OnSymbolEntry, new object[0]);
        }

        public void OnSymbolEntry(string symbol)
        {
            this.BroadcastEvent(KeyboardEvent.OnSymbolEntry, new object[] { symbol });
        }

        public void OnBackspace()
        {
            this.BroadcastEvent(KeyboardEvent.OnBackspace, new object[0]);
        }

        #endregion

        #region State changes

        public void OnShiftStateChange(bool state, KeyboardLayout layout)
        {
            this.BroadcastEvent(KeyboardEvent.OnShiftStateChange, new object[] { state, layout });
        }

        public void OnCtrlStateChange(bool state, KeyboardLayout layout)
        {
            this.BroadcastEvent(KeyboardEvent.OnCtrlStateChange, new object[] { state, layout });
        }

        public void OnAltGrStateChange(bool state, KeyboardLayout layout)
        {
            this.BroadcastEvent(KeyboardEvent.OnAltGrStateChange, new object[] { state, layout });
        }

        #endregion

        #region CTRL Commands

        public void OnCommandClear()
        {
            this.BroadcastEvent(KeyboardEvent.OnCommandClear, new object[0]);
        }

        public void OnCommandClipboardCut()
        {
            this.BroadcastEvent(KeyboardEvent.OnCommandClipboardCut, new object[0]);
        }

        public void OnCommandClipboardPaste()
        {
            this.BroadcastEvent(KeyboardEvent.OnCommandClipboardPaste, new object[0]);
        }

        public void OnCommandClipboardCopy()
        {
            this.BroadcastEvent(KeyboardEvent.OnCommandClipboardCopy, new object[0]);
        }

        public void OnCommandUndo()
        {
            this.BroadcastEvent(KeyboardEvent.OnCommandUndo, new object[0]);
        }

        public void OnCommandRedo()
        {
            this.BroadcastEvent(KeyboardEvent.OnCommandRedo, new object[0]);
        }

        public void OnCommandQuit()
        {
            this.BroadcastEvent(KeyboardEvent.OnCommandQuit, new object[0]);
        }

        public void OnCommandCloseTab()
        {
            this.BroadcastEvent(KeyboardEvent.OnCommandCloseTab, new object[0]);
        }

        public void OnCommandRefresh()
        {
            this.BroadcastEvent(KeyboardEvent.OnCommandRefresh, new object[0]);
        }

        public void OnCommandOpen()
        {
            this.BroadcastEvent(KeyboardEvent.OnCommandOpen, new object[0]);
        }

        public void OnCommandPrint()
        {
            this.BroadcastEvent(KeyboardEvent.OnCommandPrint, new object[0]);
        }

        public void OnCommandSelectAll()
        {
            this.BroadcastEvent(KeyboardEvent.OnCommandSelectAll, new object[0]);
        }

        public void OnCommandSave()
        {
            this.BroadcastEvent(KeyboardEvent.OnCommandSave, new object[0]);
        }

        public void OnCommandDuplicate()
        {
            this.BroadcastEvent(KeyboardEvent.OnCommandDuplicate, new object[0]);
        }

        public void OnCommandSearch()
        {
            this.BroadcastEvent(KeyboardEvent.OnCommandSearch, new object[0]);
        }

        public void OnCommandHistory()
        {
            this.BroadcastEvent(KeyboardEvent.OnCommandHistory, new object[0]);
        }

        public void OnCommandNew()
        {
            this.BroadcastEvent(KeyboardEvent.OnCommandNew, new object[0]);
        }

        public void OnCommandMap()
        {
            this.BroadcastEvent(KeyboardEvent.OnCommandMap, new object[0]);
        }

        public void OnCommandQuickAction()
        {
            this.BroadcastEvent(KeyboardEvent.OnCommandQuickAction, new object[0]);
        }

        public void OnCommandNext()
        {
            this.BroadcastEvent(KeyboardEvent.OnCommandNext, new object[0]);
        }

        #endregion

        #region Plugin Events

        public void OnInputSubmit(string input)
        {
            this.BroadcastEvent(KeyboardEvent.OnInputSubmit, new object[] { input });
        }

        #endregion
    }
}
