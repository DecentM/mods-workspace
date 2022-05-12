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
            if (this.playlist == null)
                return;

            object[] next = this.playlist.GetCurrent();
            this.PlayItem(next);
        }

        protected override void OnPlaybackEnd()
        {
            if (this.playlist == null)
                return;

            object[] next = this.playlist.Next();
            this.PlayItem(next);
        }

        protected override void OnAutoRetryAbort()
        {
            if (this.playlist == null)
                return;

            object[] next = this.playlist.Next();
            this.PlayItem(next);
        }

        public void SetCurrentPlaylist(VideoPlaylist playlist)
        {
            this.playlist = playlist;
        }

        public void PlayItem(object[] item)
        {
            if (item == null)
                return;

            VRCUrl url = (VRCUrl)item[0];

            this.system.RequestVideo(url);
        }
    }
}
