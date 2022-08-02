using UnityEngine;

using ABI.CCK.Components;

namespace DecentM.Prefabs.VideoPlayer.Handlers
{
    public class CVRVideo : PlayerHandler
    {
        public override VideoPlayerHandlerType type => VideoPlayerHandlerType.CVR;

        public CVRVideoPlayer player;
        public VideoPlayerEvents events;

        private void Start()
        {
            this.player.startedPlayback.AddListener(this.OnPlaybackStart);
            this.player.finishedPlayback.AddListener(this.OnPlaybackFinished);
            this.player.pausedPlayback.AddListener(this.OnPlaybackPaused);
        }

        private bool _isPlaying = false;

        private void OnPlaybackStart()
        {
            this._isPlaying = true;
            this.events.OnPlaybackStart(this.GetTime());
        }

        private void OnPlaybackFinished()
        {
            this._isPlaying = false;
            this.events.OnPlaybackEnd();
        }

        private void OnPlaybackPaused()
        {
            this._isPlaying = false;
            this.events.OnPlaybackStop(this.GetTime());
        }

        public override bool IsPlaying
        {
            get
            {
                return this._isPlaying;
            }
        }

        public override float GetDuration()
        {
            return -1;
            // TODO: Restore once we have inline modded video players
            // return (float)this.player.VideoPlayer.Info.VideoMetaData.GetDuration();
        }

        public override float GetTime()
        {
            return -1;
            // TODO: Restore once we have inline modded video players
            // return (float)this.player.VideoPlayer.Time;
        }

        public override void SetTime(float time)
        {
            // this.player.SetVideoTimestamp(time);
        }

        public override void LoadURL(string url)
        {
            this.player.SetUrl(url);
        }

        public override void Pause()
        {
            this.player.Pause();
        }

        public override void Pause(float time)
        {
            this.Pause();
            this.SetTime(time);
        }

        public override void Play()
        {
            this.player.Play();
        }

        public override void Play(float time)
        {
            this.SetTime(time);
            this.Play();
        }

        public override void Unload()
        {
            this.Pause();
            this.SetTime(0);
            this.player.SetUrl(null);
        }

        public RenderTexture texture;

        public override Texture GetScreenTexture()
        {
            return this.texture;
        }
    }
}
