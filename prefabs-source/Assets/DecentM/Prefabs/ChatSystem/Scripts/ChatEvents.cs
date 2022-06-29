using System;
using UdonSharp;
using DecentM.Pubsub;

namespace DecentM.Chat
{
    public enum ChatEvent
    {
        OnMessageAdded,
        OnMessageChanged,
        OnMessageDeleted,

        OnChannelAdded,
        OnChannelDeleted,

        OnPlayerTypingStart,
        OnPlayerTypingStop,

        OnProfilePictureChange,

        OnPlayerPresent,
        OnPlayerAway,
    }

    [UdonBehaviourSyncMode(BehaviourSyncMode.None)]
    public sealed class ChatEvents : PubsubHost
    {
        #region Messages

        public void OnMessageAdded(ChatMessage message)
        {
            this.BroadcastEvent(ChatEvent.OnMessageAdded, message);
        }

        public void OnMessageChanged(ChatMessage message)
        {
            this.BroadcastEvent(ChatEvent.OnMessageChanged, message);
        }

        public void OnMessageDeleted(ChatMessage message)
        {
            this.BroadcastEvent(ChatEvent.OnMessageDeleted, message.id);
        }

        #endregion

        #region Channels

        public void OnChannelAdded(MessageStore store)
        {
            this.BroadcastEvent(ChatEvent.OnChannelAdded, store.channel);
        }

        public void OnChannelDeleted(MessageStore store)
        {
            this.BroadcastEvent(ChatEvent.OnChannelDeleted, store.channel);
        }

        #endregion

        #region Typing Indicators

        public void OnPlayerTypingStart(int playerId)
        {
            this.BroadcastEvent(ChatEvent.OnPlayerTypingStart, playerId);
        }

        public void OnPlayerTypingStop(int playerId)
        {
            this.BroadcastEvent(ChatEvent.OnPlayerTypingStop, playerId);
        }

        #endregion

        #region Profile Pictures

        public void OnProfilePictureChange(int playerId)
        {
            this.BroadcastEvent(ChatEvent.OnProfilePictureChange, playerId);
        }

        #endregion

        #region Presence

        public void OnPlayerPresent(int playerId)
        {
            this.BroadcastEvent(ChatEvent.OnPlayerPresent, playerId);
        }

        public void OnPlayerAway(int playerId)
        {
            this.BroadcastEvent(ChatEvent.OnPlayerAway, playerId);
        }

        #endregion
    }
}
