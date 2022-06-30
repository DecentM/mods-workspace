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

        public void OnMessageAdded(string id, object[] message)
        {
            this.BroadcastEvent(ChatEvent.OnMessageAdded, id, message);
        }

        public void OnMessageChanged(string id, object[] message)
        {
            this.BroadcastEvent(ChatEvent.OnMessageChanged, id, message);
        }

        public void OnMessageDeleted(string id, object[] message)
        {
            this.BroadcastEvent(ChatEvent.OnMessageDeleted, id, message);
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
