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

        // public string OnMessageAddedEvent = ChatEvent.ChatEvents.OnMessageAdded);

        public void OnMessageAdded(ChatMessage message)
        {
            this.BroadcastEvent(ChatEvent.OnMessageAdded, message);
        }

        // public string OnMessageChangedEvent = ChatEvent.ChatEvents.OnMessageChanged);

        public void OnMessageChanged(ChatMessage message)
        {
            this.BroadcastEvent(ChatEvent.OnMessageChanged, message);
        }

        // public string OnMessageDeletedEvent = ChatEvent.ChatEvents.OnMessageDeleted);

        public void OnMessageDeleted(ChatMessage message)
        {
            this.BroadcastEvent(ChatEvent.OnMessageDeleted, message.id);
        }

        #endregion

        #region Channels

        // public string OnChannelAddedEvent = ChatEvent.ChatEvents.OnChannelAdded);

        public void OnChannelAdded(MessageStore store)
        {
            this.BroadcastEvent(ChatEvent.OnChannelAdded, store.channel);
        }

        // public string OnChannelDeletedEvent = ChatEvent.ChatEvents.OnChannelDeleted);

        public void OnChannelDeleted(MessageStore store)
        {
            this.BroadcastEvent(ChatEvent.OnChannelDeleted, store.channel);
        }

        #endregion

        #region Typing Indicators

        // public string OnPlayerTypingStartEvent = ChatEvent.ChatEvents.OnPlayerTypingStart);

        public void OnPlayerTypingStart(int playerId)
        {
            this.BroadcastEvent(ChatEvent.OnPlayerTypingStart, playerId);
        }

        // public string OnPlayerTypingStopEvent = ChatEvent.ChatEvents.OnPlayerTypingStop);

        public void OnPlayerTypingStop(int playerId)
        {
            this.BroadcastEvent(ChatEvent.OnPlayerTypingStop, playerId);
        }

        #endregion

        #region Profile Pictures

        // public string OnProfilePictureChangeEvent = ChatEvent.ChatEvents.OnProfilePictureChange);

        public void OnProfilePictureChange(int playerId)
        {
            this.BroadcastEvent(ChatEvent.OnProfilePictureChange, playerId);
        }

        #endregion

        #region Presence

        // public string OnPlayerPresentEvent = ChatEvent.ChatEvents.OnPlayerPresent);

        public void OnPlayerPresent(int playerId)
        {
            this.BroadcastEvent(ChatEvent.OnPlayerPresent, playerId);
        }

        // public string OnPlayerAwayEvent = ChatEvent.ChatEvents.OnPlayerAway);

        public void OnPlayerAway(int playerId)
        {
            this.BroadcastEvent(ChatEvent.OnPlayerAway, playerId);
        }

        #endregion
    }
}
