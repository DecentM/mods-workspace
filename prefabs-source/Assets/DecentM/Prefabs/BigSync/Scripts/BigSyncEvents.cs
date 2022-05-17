using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;
using VRC.SDK3.Components.Video;
using DecentM.Pubsub;

namespace DecentM.BigSync
{
    public enum BigSyncDirection
    {
        Send,
        Receive,
    }

    public enum BigSyncStatus
    {
        Successful,
        Failed,
    }

    public enum BigSyncEvent
    {
        OnSyncBegin,
        OnSyncProgress,
        OnSyncEnd,
        OnSyncCancel,
        OnDebugLog,
    }

    [UdonBehaviourSyncMode(BehaviourSyncMode.None)]
    public sealed class BigSyncEvents : PubsubHost
    {
        #region Core

        public void OnDebugLog(string message)
        {
            this.BroadcastEvent(BigSyncEvent.OnDebugLog, message);
        }

        public void OnSyncBegin(BigSyncDirection direction, int playerId, int arrayLength)
        {
            this.BroadcastEvent(BigSyncEvent.OnSyncBegin, direction, playerId, arrayLength);
        }

        public void OnSyncProgress(BigSyncDirection direction, int playerId, float progress)
        {
            this.BroadcastEvent(BigSyncEvent.OnSyncProgress, direction, playerId, progress);
        }

        public void OnSyncEnd(BigSyncDirection direction, int playerId, BigSyncStatus status)
        {
            this.BroadcastEvent(BigSyncEvent.OnSyncEnd, direction, playerId, status);
        }

        public void OnSyncCancel(BigSyncDirection direction, int playerId)
        {
            this.BroadcastEvent(BigSyncEvent.OnSyncCancel, direction, playerId);
        }

        #endregion

    }
}
