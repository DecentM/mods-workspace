
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
            string title = (string)item[2];
            string uploader = (string)item[3];
            string platform = (string)item[4];
            int views = (int)item[5];
            int likes = (int)item[6];
            string resolution = (string)item[7];
            int fps = (int)item[8];
            string description = (string)item[9];
            string duration = (string)item[10];
            string[][] subtitles = (string[][])item[11];

            this.playlist.textureUpdater.SetTexture(thumbnail.texture);
            this.system.LoadVideo(url);
            this.events.OnMetadataChange(title, uploader, platform, views, likes, resolution, fps, description, duration, subtitles);
        }
    }
}
