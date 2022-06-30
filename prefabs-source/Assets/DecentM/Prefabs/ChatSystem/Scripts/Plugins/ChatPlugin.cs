using UdonSharp;
using UnityEngine;
using VRC.SDKBase;

using DecentM.Pubsub;

namespace DecentM.Chat.Plugins
{
    [UdonBehaviourSyncMode(BehaviourSyncMode.None)]
    public class ChatPlugin : PubsubSubscriber
    {
        public ChatSystem system;
        public ChatEvents events;

        protected virtual void OnMessageAdded(string id, object[] message) { }

        protected virtual void OnMessageChanged(string id, object[] message) { }

        protected virtual void OnMessageDeleted(string id, object[] message) { }

        protected virtual void OnPlayerTypingStart(int playerId) { }

        protected virtual void OnPlayerTypingStop(int playerId) { }

        protected virtual void OnProfilePictureChange(int playerId) { }

        protected virtual void OnPlayerPresent(int playerId) { }

        protected virtual void OnPlayerAway(int playerId) { }

        public sealed override void OnPubsubEvent(object name, object[] data)
        {
            switch (name)
            {
                #region Messages

                case ChatEvent.OnMessageAdded:
                {
                    string id = (string)data[0];
                    object[] message = (object[])data[1];
                    this.OnMessageAdded(id, message);
                    return;
                }

                case ChatEvent.OnMessageChanged:
                {
                    string id = (string)data[0];
                    object[] message = (object[])data[1];
                    this.OnMessageChanged(id, message);
                    return;
                }

                case ChatEvent.OnMessageDeleted:
                {
                    string id = (string)data[0];
                    object[] message = (object[])data[1];
                    this.OnMessageDeleted(id, message);
                    return;
                }

                #endregion

                #region Plugins

                case ChatEvent.OnPlayerTypingStart:
                {
                    int playerId = (int)data[0];
                    this.OnPlayerTypingStart(playerId);
                    return;
                }

                case ChatEvent.OnPlayerTypingStop:
                {
                    int playerId = (int)data[0];
                    this.OnPlayerTypingStop(playerId);
                    return;
                }

                case ChatEvent.OnProfilePictureChange:
                {
                    int playerId = (int)data[0];
                    this.OnProfilePictureChange(playerId);
                    return;
                }

                case ChatEvent.OnPlayerPresent:
                {
                    int playerId = (int)data[0];
                    this.OnPlayerPresent(playerId);
                    return;
                }

                case ChatEvent.OnPlayerAway:
                {
                    int playerId = (int)data[0];
                    this.OnPlayerAway(playerId);
                    return;
                }

                #endregion
            }
        }
    }
}
