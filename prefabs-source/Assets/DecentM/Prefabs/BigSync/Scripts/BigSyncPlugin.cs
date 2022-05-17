using UdonSharp;
using DecentM.Pubsub;
using UnityEngine;
using VRC.SDKBase;
using VRC.SDK3.Components.Video;

namespace DecentM.BigSync.Plugins
{
    [UdonBehaviourSyncMode(BehaviourSyncMode.None)]
    public abstract class BigSyncPlugin : PubsubSubscriber
    {
        public BigSyncSystem system;
        public BigSyncEvents events;

        protected virtual void OnDebugLog(string message) { }

        protected virtual void OnSyncBegin(
            BigSyncDirection direction,
            int playerId,
            int arrayLength
        ) { }

        protected virtual void OnSyncProgress(
            BigSyncDirection direction,
            int playerId,
            float progress
        ) { }

        protected virtual void OnSyncEnd(
            BigSyncDirection direction,
            int playerId,
            BigSyncStatus status
        ) { }

        public sealed override void OnPubsubEvent(object name, object[] data)
        {
            switch (name)
            {
                #region Core

                case BigSyncEvent.OnDebugLog:
                {
                    string message = (string)data[0];
                    this.OnDebugLog(message);
                    break;
                }

                case BigSyncEvent.OnSyncBegin:
                {
                    BigSyncDirection direction = (BigSyncDirection)data[0];
                    int playerId = (int)data[1];
                    int arrayLength = (int)data[2];
                    this.OnSyncBegin(direction, playerId, arrayLength);
                    break;
                }

                case BigSyncEvent.OnSyncProgress:
                {
                    BigSyncDirection direction = (BigSyncDirection)data[0];
                    int playerId = (int)data[1];
                    float progress = (float)data[2];
                    this.OnSyncProgress(direction, playerId, progress);
                    break;
                }

                case BigSyncEvent.OnSyncEnd:
                {
                    BigSyncDirection direction = (BigSyncDirection)data[0];
                    int playerId = (int)data[1];
                    BigSyncStatus status = (BigSyncStatus)data[2];
                    this.OnSyncEnd(direction, playerId, status);
                    break;
                }

                default:
                    break;

                #endregion
            }
        }
    }
}
