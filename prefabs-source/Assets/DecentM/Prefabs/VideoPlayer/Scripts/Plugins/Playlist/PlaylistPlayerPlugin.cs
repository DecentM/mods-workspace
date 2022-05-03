
using UdonSharp;
using UnityEngine;
using VRC.SDK3.Components.Video;
using VRC.SDKBase;
using VRC.Udon;

namespace DecentM.VideoPlayer.Plugins
{
    public class PlaylistPlayerPlugin : VideoPlayerPlugin
    {
        public VideoPlaylist playlist;

        protected override void OnVideoPlayerInit()
        {
            VRCUrl nextUrl = this.playlist.GetCurrentUrl();
            if (nextUrl == null) return;

            this.system.LoadVideo(nextUrl);
        }

        protected override void OnPlaybackEnd()
        {
            VRCUrl nextUrl = this.playlist.Next();
            if (nextUrl == null) return;

            this.system.LoadVideo(nextUrl);
        }
    }
}
