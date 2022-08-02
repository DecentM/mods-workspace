using System;
using UnityEngine;
using TMPro;

namespace DecentM.Prefabs.VideoPlayer.Plugins
{
    public class AutoPlayPlugin : VideoPlayerPlugin
    {
        public bool autoplayOnLoad = true;

        /* TODO: Port this code once we have ownership working

            private bool isOwner
            {
                get { return Networking.GetOwner(this.gameObject) == Networking.LocalPlayer; }
            }
        */

        // TODO: Port this code once we have networked events working
        // private int receivedLoadedFrom = 0;

        protected override void OnLoadReady(float duration)
        {
            /* TODO: Port this code once we have a way to get player count

                if (VRCPlayerApi.GetPlayerCount() == 1)
                {
                    this.system.StartPlayback();
                    return;
                }
            */

            /* TODO: Port this code once we have ownership working

                if (this.isOwner)
                    return;
            */

            /* TODO: Port this code once we have network events working

            this.SendCustomNetworkEvent(
                VRC.Udon.Common.Interfaces.NetworkEventTarget.Owner,
                nameof(this.OnClientLoaded)
            );
            */

            // TODO: Remove this once the above functions are ported
            this.system.StartPlayback();
        }

        /* TODO: Restore these functions once we have ownership and networking working

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
        */
    }
}
