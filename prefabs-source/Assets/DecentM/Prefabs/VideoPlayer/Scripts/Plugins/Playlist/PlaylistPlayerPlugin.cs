
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
            object[] next = this.playlist.GetCurrent();
            this.PlayItem(next);
        }

        protected override void OnPlaybackEnd()
        {
            object[] next = this.playlist.Next();
            this.PlayItem(next);
        }

        protected override void OnAutoRetryAbort()
        {
            object[] next = this.playlist.Next();
            this.PlayItem(next);
        }

        public void SetCurrentPlaylist(VideoPlaylist playlist)
        {
            this.playlist = playlist;
        }

        public void PlayItem(object[] item)
        {
            if (item == null) return;

            VRCUrl url = (VRCUrl)item[0];
            Sprite thumbnail = (Sprite)item[1];

            this.playlist.textureUpdater.SetTexture(thumbnail.texture);
            this.system.LoadVideo(url);
        }
    }
}
