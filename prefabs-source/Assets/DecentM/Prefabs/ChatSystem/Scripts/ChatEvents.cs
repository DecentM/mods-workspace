using System;
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

namespace DecentM.Chat
{
    [UdonBehaviourSyncMode(BehaviourSyncMode.None)]
    public class ChatEvents : UdonSharpBehaviour
    {
        private UdonSharpBehaviour[] subscribers;

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

        private void BroadcastEvent(string eventName, object data)
        {
            foreach (UdonSharpBehaviour subscriber in this.subscribers)
            {
                subscriber.SetProgramVariable($"OnChatEvent_name", eventName);
                subscriber.SetProgramVariable($"OnChatEvent_data", data);
                subscriber.SendCustomEvent("OnChatEvent");
            }
        }

        #region Messages

        // public string OnMessageAddedEvent = nameof(ChatEvents.OnMessageAdded);

        public void OnMessageAdded(ChatMessage message)
        {
            this.BroadcastEvent(nameof(OnMessageAdded), message);
        }

        // public string OnMessageChangedEvent = nameof(ChatEvents.OnMessageChanged);

        public void OnMessageChanged(ChatMessage message)
        {
            this.BroadcastEvent(nameof(OnMessageChanged), message);
        }

        // public string OnMessageDeletedEvent = nameof(ChatEvents.OnMessageDeleted);

        public void OnMessageDeleted(ChatMessage message)
        {
            this.BroadcastEvent(nameof(OnMessageDeleted), message.id);
        }

        #endregion

        #region Channels

        // public string OnChannelAddedEvent = nameof(ChatEvents.OnChannelAdded);

        public void OnChannelAdded(MessageStore store)
        {
            this.BroadcastEvent(nameof(OnChannelAdded), store.channel);
        }

        // public string OnChannelDeletedEvent = nameof(ChatEvents.OnChannelDeleted);

        public void OnChannelDeleted(MessageStore store)
        {
            this.BroadcastEvent(nameof(OnChannelDeleted), store.channel);
        }

        #endregion

        #region Typing Indicators

        // public string OnPlayerTypingStartEvent = nameof(ChatEvents.OnPlayerTypingStart);

        public void OnPlayerTypingStart(int playerId)
        {
            this.BroadcastEvent(nameof(OnPlayerTypingStart), playerId);
        }

        // public string OnPlayerTypingStopEvent = nameof(ChatEvents.OnPlayerTypingStop);

        public void OnPlayerTypingStop(int playerId)
        {
            this.BroadcastEvent(nameof(OnPlayerTypingStop), playerId);
        }

        #endregion

        #region Profile Pictures

        // public string OnProfilePictureChangeEvent = nameof(ChatEvents.OnProfilePictureChange);

        public void OnProfilePictureChange(int playerId)
        {
            this.BroadcastEvent(nameof(OnProfilePictureChange), playerId);
        }

        #endregion

        #region Presence

        // public string OnPlayerPresentEvent = nameof(ChatEvents.OnPlayerPresent);

        public void OnPlayerPresent(int playerId)
        {
            this.BroadcastEvent(nameof(OnPlayerPresent), playerId);
        }

        // public string OnPlayerAwayEvent = nameof(ChatEvents.OnPlayerAway);

        public void OnPlayerAway(int playerId)
        {
            this.BroadcastEvent(nameof(OnPlayerAway), playerId);
        }

        #endregion
    }
}
