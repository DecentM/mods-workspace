using System;
using UdonSharp;
using UnityEngine;
using VRC.SDK3.Components.Video;
using VRC.SDKBase;
using VRC.Udon;
using TMPro;

namespace DecentM.VideoPlayer.Plugins
{
    public class AutoPlayPlugin : VideoPlayerPlugin
    {
        public bool autoplayOnLoad = true;

        private bool isOwner
        {
            get { return Networking.GetOwner(this.gameObject) == Networking.LocalPlayer; }
        }

        private int receivedLoadedFrom = 0;

        protected override void OnLoadReady(float duration)
        {
            if (VRCPlayerApi.GetPlayerCount() == 1)
            {
                this.system.StartPlayback();
                return;
            }

            if (this.isOwner)
                return;

            this.SendCustomNetworkEvent(
                VRC.Udon.Common.Interfaces.NetworkEventTarget.Owner,
                nameof(this.OnClientLoaded)
            );
        }

        // Everyone except the owner does this, because then owner already knows when it finishes loading
        public void OnClientLoaded()
        {
            if (!this.isOwner || !this.autoplayOnLoad)
                return;

            this.receivedLoadedFrom++;

            this.events.OnRemotePlayerLoaded(this.receivedLoadedFrom);

            // Everyone is loaded when this counter is above the player count
            // (skipping one for the local player)
            if (this.receivedLoadedFrom < VRCPlayerApi.GetPlayerCount() - 1)
                return;

            this.system.StartPlayback();
        }

        private int ownerId = 0;

        protected override void OnOwnershipChanged(int previousOwnerId, VRCPlayerApi nextOwner)
        {
            if (nextOwner == null || !nextOwner.IsValid())
                return;

            this.ownerId = nextOwner.playerId;
        }

        protected override void OnLoadApproved(VRCUrl url)
        {
            if (!this.isOwner || !this.autoplayOnLoad)
                return;

            this.receivedLoadedFrom = 0;
        }
    }
}
