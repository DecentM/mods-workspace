using UnityEngine;

namespace DecentM.Prefabs.VideoPlayer.Plugins
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

            string url = (string)item[0];

            this.system.RequestVideo(url);
        }
    }
}
