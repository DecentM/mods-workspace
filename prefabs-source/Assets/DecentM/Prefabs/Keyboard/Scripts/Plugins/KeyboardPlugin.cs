using System;
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

using DecentM.Pubsub;

namespace DecentM.Keyboard.Plugins
{
    [UdonBehaviourSyncMode(BehaviourSyncMode.None)]
    public abstract class KeyboardPlugin : PubsubSubscriber
    {
        public KeyboardEvents events;
        public KeyboardSystem system;

        protected virtual void OnDebugLog(string message) { }

        protected virtual void OnKeyPressDown(KeyboardKey key) { }

        protected virtual void OnKeyPressUp(KeyboardKey key) { }

        protected virtual void OnSymbolEntry(string symbol) { }

        protected virtual void OnBackspace() { }

        protected virtual void OnShiftStateChange(bool state, KeyboardLayout layout) { }

        protected virtual void OnCtrlStateChange(bool state, KeyboardLayout layout) { }

        protected virtual void OnAltGrStateChange(bool state, KeyboardLayout layout) { }

        protected virtual void OnCommandClear() { }

        protected virtual void OnCommandClipboardCut() { }

        protected virtual void OnCommandClipboardPaste() { }

        protected virtual void OnCommandClipboardCopy() { }

        protected virtual void OnCommandUndo() { }

        protected virtual void OnCommandRedo() { }

        protected virtual void OnCommandQuit() { }

        protected virtual void OnCommandCloseTab() { }

        protected virtual void OnCommandRefresh() { }

        protected virtual void OnCommandOpen() { }

        protected virtual void OnCommandPrint() { }

        protected virtual void OnCommandSelectAll() { }

        protected virtual void OnCommandSave() { }

        protected virtual void OnCommandDuplicate() { }

        protected virtual void OnCommandSearch() { }

        protected virtual void OnCommandHistory() { }

        protected virtual void OnCommandNew() { }

        protected virtual void OnCommandMap() { }

        protected virtual void OnCommandQuickAction() { }

        protected virtual void OnCommandNext() { }

        protected virtual void OnInputSubmit(string input) { }

        public sealed override void OnPubsubEvent(object name, object[] data)
        {
            switch (name)
            {
                case KeyboardEvent.OnDebugLog:
                {
                    string message = (string)data[0];
                    this.OnDebugLog(message);
                    return;
                }

                #region Keypresses

                case KeyboardEvent.OnKeyPressDown:
                {
                    KeyboardKey key = (KeyboardKey)data[0];
                    this.OnKeyPressDown(key);
                    return;
                }

                case KeyboardEvent.OnKeyPressUp:
                {
                    KeyboardKey key = (KeyboardKey)data[0];
                    this.OnKeyPressUp(key);
                    return;
                }

                case KeyboardEvent.OnSymbolEntry:
                {
                    string symbol = (string)data[0];
                    this.OnSymbolEntry(symbol);
                    return;
                }

                case KeyboardEvent.OnBackspace:
                {
                    this.OnBackspace();
                    return;
                }

                #endregion

                #region State changes

                case KeyboardEvent.OnShiftStateChange:
                {
                    bool state = (bool)data[0];
                    KeyboardLayout layout = (KeyboardLayout)data[1];
                    this.OnShiftStateChange(state, layout);
                    return;
                }

                case KeyboardEvent.OnCtrlStateChange:
                {
                    bool state = (bool)data[0];
                    KeyboardLayout layout = (KeyboardLayout)data[1];
                    this.OnCtrlStateChange(state, layout);
                    return;
                }

                case KeyboardEvent.OnAltGrStateChange:
                {
                    bool state = (bool)data[0];
                    KeyboardLayout layout = (KeyboardLayout)data[1];
                    this.OnAltGrStateChange(state, layout);
                    return;
                }

                #endregion

                #region CTRL Commands

                case KeyboardEvent.OnCommandClear:
                {
                    this.OnCommandClear();
                    return;
                }

                case KeyboardEvent.OnCommandClipboardCut:
                {
                    this.OnCommandClipboardCut();
                    return;
                }

                case KeyboardEvent.OnCommandClipboardPaste:
                {
                    this.OnCommandClipboardPaste();
                    return;
                }

                case KeyboardEvent.OnCommandClipboardCopy:
                {
                    this.OnCommandClipboardCopy();
                    return;
                }

                case KeyboardEvent.OnCommandUndo:
                {
                    this.OnCommandUndo();
                    return;
                }

                case KeyboardEvent.OnCommandRedo:
                {
                    this.OnCommandRedo();
                    return;
                }

                case KeyboardEvent.OnCommandQuit:
                {
                    this.OnCommandQuit();
                    return;
                }

                case KeyboardEvent.OnCommandCloseTab:
                {
                    this.OnCommandCloseTab();
                    return;
                }

                case KeyboardEvent.OnCommandRefresh:
                {
                    this.OnCommandRefresh();
                    return;
                }

                case KeyboardEvent.OnCommandOpen:
                {
                    this.OnCommandOpen();
                    return;
                }

                case KeyboardEvent.OnCommandPrint:
                {
                    this.OnCommandPrint();
                    return;
                }

                case KeyboardEvent.OnCommandSelectAll:
                {
                    this.OnCommandSelectAll();
                    return;
                }

                case KeyboardEvent.OnCommandSave:
                {
                    this.OnCommandSave();
                    return;
                }

                case KeyboardEvent.OnCommandDuplicate:
                {
                    this.OnCommandDuplicate();
                    return;
                }

                case KeyboardEvent.OnCommandSearch:
                {
                    this.OnCommandSearch();
                    return;
                }

                case KeyboardEvent.OnCommandHistory:
                {
                    this.OnCommandHistory();
                    return;
                }

                case KeyboardEvent.OnCommandNew:
                {
                    this.OnCommandNew();
                    return;
                }

                case KeyboardEvent.OnCommandMap:
                {
                    this.OnCommandMap();
                    return;
                }

                case KeyboardEvent.OnCommandQuickAction:
                {
                    this.OnCommandQuickAction();
                    return;
                }

                case KeyboardEvent.OnCommandNext:
                {
                    this.OnCommandNext();
                    return;
                }

                #endregion

                #region Plugins

                case KeyboardEvent.OnInputSubmit:
                {
                    string symbol = (string)data[0];
                    this.OnInputSubmit(symbol);
                    return;
                }

                #endregion
            }
        }
    }
}
