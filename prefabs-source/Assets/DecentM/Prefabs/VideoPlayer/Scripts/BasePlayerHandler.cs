using UnityEngine;
using UdonSharp;
using VRC.SDK3.Video.Components;
using VRC.SDK3.Video.Components.Base;
using VRC.SDK3.Video.Components.AVPro;
using VRC.SDK3.Components.Video;
using VRC.SDKBase;
using VRC.Udon;

namespace DecentM.VideoPlayer
{
    public enum VideoPlayerHandlerType
    {
        Unity,
        AVPro,
    }

    public abstract class BasePlayerHandler : UdonSharpBehaviour
    {
        public abstract VideoPlayerHandlerType type { get; }

        public BaseVRCVideoPlayer player;
        public VideoPlayerEvents events;

        public Renderer screen;

        private MaterialPropertyBlock fetchBlock;

        void Start()
        {
            if (this.player == null)
            {
                Debug.LogError($"missing BaseVRCVideoPlayer on {this.name}, this video player will be broken");
                this.enabled = false;
                return;
            }

            this.fetchBlock = new MaterialPropertyBlock();
        }

        public float progressReportIntervalSeconds = 1;

        private float clock = 0;

        private void FixedUpdate()
        {
            if (!this.player.IsPlaying || float.IsInfinity(this.player.GetDuration())) return;

            this.clock += Time.fixedDeltaTime;

            if (this.clock > this.progressReportIntervalSeconds)
            {
                this.HandleProgress();
                this.clock = 0;
            }
        }

        private void HandleProgress()
        {
            this.events.OnProgress(this.player.GetTime(), this.player.GetDuration());
        }

        public override void OnVideoEnd()
        {
            this.events.OnPlaybackEnd();
        }

        public override void OnVideoPause()
        {
            this.events.OnPlaybackStop(this.player.GetTime());
        }

        public override void OnVideoPlay()
        {
            this.events.OnPlaybackStart(this.player.GetTime());
        }

        public override void OnVideoReady()
        {
            this.events.OnLoadReady(this.player.GetDuration());
        }

        public override void OnVideoStart()
        {
            this.events.OnPlaybackStart(this.player.GetTime());
        }

        public override void OnVideoError(VideoError videoError)
        {
            this.events.OnLoadError(videoError);
        }

        public void StartPlayback()
        {
            this.player.Play();
        }

        public void StartPlayback(float timestamp)
        {
            this.player.SetTime(timestamp);
            this.player.Play();
        }

        public void Seek(float timestamp)
        {
            this.player.SetTime(timestamp);
        }

        public void PausePlayback(float timestamp)
        {
            this.player.Pause();
            this.player.SetTime(timestamp);
        }

        public void PausePlayback()
        {
            this.player.Pause();
        }

        public void LoadVideo(VRCUrl url)
        {
            this.player.LoadURL(url);
            this.player.Pause();
            this.player.SetTime(0);
        }

        public void UnloadVideo()
        {
            this.player.Stop();
        }

        public bool IsPlaying()
        {
            return this.player.IsPlaying;
        }

        public Texture GetScreenTexture()
        {
            if (this.type == VideoPlayerHandlerType.Unity)
            {
                this.screen.GetPropertyBlock(this.fetchBlock);

                return this.fetchBlock.GetTexture("_MainTex");
            }

            return this.screen.material.GetTexture("_MainTex");
        }
    }
}
