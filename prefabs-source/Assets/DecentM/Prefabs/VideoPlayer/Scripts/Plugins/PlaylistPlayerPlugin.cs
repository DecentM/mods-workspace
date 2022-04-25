
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

        private VRCUrl nextUrl;

        protected override void OnVideoPlayerInit()
        {
            this.nextUrl = this.playlist.GetCurrentUrl();
            
            string a = this.nextUrl == null ? string.Empty : this.nextUrl.ToString();

            this.system.LoadVideo(this.nextUrl);
        }

        protected override void OnPlaybackEnd()
        {
            this.nextUrl = this.playlist.Next();
            this.system.LoadVideo(this.nextUrl);
        }

        protected override void OnLoadReady(float duration)
        {
            if (this.system.GetCurrentUrl() != this.nextUrl) return;

            this.system.StartPlayback();
        }
    }
}
