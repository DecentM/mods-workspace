using System;
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

using DecentM.Pubsub;

namespace DecentM.Keyboard
{
    public enum KeyboardEvent
    {
        OnDebugLog,

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
    public class KeyboardEvents : PubsubHost
    {
        public void OnDebugLog(string message)
        {
            this.BroadcastEvent(KeyboardEvent.OnDebugLog, new object[] { message });
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
