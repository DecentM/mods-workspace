using System;
using UnityEngine;
using TMPro;

using DecentM.Prefabs.VideoPlayer.Handlers;

namespace DecentM.Prefabs.VideoPlayer.Plugins
{
    public class AutoLivestreamDetectorPlugin : VideoPlayerPlugin
    {
        public string[] knownLivestreamHosts;

        protected override void OnLoadRequested(string url)
        {
            // TODO: Take the guard out once we're capable of playing livestreams
            return;

            // If we're already using the AVPro player, don't do anything
            if (this.system.currentPlayerHandler.type != VideoPlayerHandlerType.Unity)
                return;

            // Switch to the next player if we know that the url trying to be played is a livestream URL
            // This will cause another OnLoadRequested, creating a loop that stops when we're using an AVPro player
            foreach (string host in knownLivestreamHosts)
            {
                if (url.Contains(host))
                {
                    this.events.OnLoadDenied(url, "Switching to livestream mode");
                    this.system.Unload();
                    this.system.NextPlayerHandler();
                    this.system.RequestVideo(url);
                    break;
                }
            }
        }
    }
}
