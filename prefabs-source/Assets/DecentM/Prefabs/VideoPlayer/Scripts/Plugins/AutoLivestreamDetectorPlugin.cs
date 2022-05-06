using System;
using UdonSharp;
using UnityEngine;
using VRC.SDK3.Components.Video;
using VRC.SDKBase;
using VRC.Udon;
using TMPro;

namespace DecentM.VideoPlayer.Plugins
{
    public class AutoLivestreamDetectorPlugin : VideoPlayerPlugin
    {
        public string[] knownLivestreamHosts;

        protected override void OnLoadRequested(VRCUrl vrcUrl)
        {
            // If we're already using the AVPro player, don't do anything
            if (this.system.currentPlayerHandler.type == VideoPlayerHandlerType.AVPro) return;

            string url = vrcUrl.ToString();

            // Switch to the next player if we know that the url trying to be played is a livestream URL
            // This will cause another OnLoadRequested, creating a loop that stops when we're using an AVPro player
            foreach (string host in knownLivestreamHosts)
            {
                if (url.Contains(host))
                {
                    this.events.OnLoadDenied(vrcUrl, "Switching to livestream mode");
                    this.system.UnloadVideo();
                    this.system.NextPlayerHandler();
                    this.events.OnLoadApproved(vrcUrl);
                    this.system.LoadVideo(vrcUrl);
                    break;
                }
            }
        }
    }
}
