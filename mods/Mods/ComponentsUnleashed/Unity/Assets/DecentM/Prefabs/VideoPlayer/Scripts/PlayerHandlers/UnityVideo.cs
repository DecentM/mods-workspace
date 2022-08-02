using UnityEngine;

namespace DecentM.Prefabs.VideoPlayer.Handlers
{
    public class UnityVideo : PlayerHandler
    {
        public override VideoPlayerHandlerType type => VideoPlayerHandlerType.Unity;

        public UnityEngine.Video.VideoPlayer player;
        public AudioSource audioSource;
        public MeshRenderer screen;
        public VideoPlayerEvents events;

        private MaterialPropertyBlock _fetchBlock;

        private void Start()
        {
            this.player.SetTargetAudioSource(0, this.audioSource);

            this.player.loopPointReached += OnPlaybackEnd;
            this.player.prepareCompleted += OnPlayerLoaded;
            this.player.errorReceived += OnVideoError;

            this._fetchBlock = new MaterialPropertyBlock();
        }

        private void OnVideoError(UnityEngine.Video.VideoPlayer source, string message)
        {
            VideoError error = new VideoError(VideoErrorType.Unknown, message);

            // TODO: Do some logging here and get some sort of error-type detection going
            if (message.Contains("denied"))
                error.type = VideoErrorType.HttpError;

            this.events.OnLoadError(error);
        }

        private void OnPlayerLoaded(UnityEngine.Video.VideoPlayer source)
        {
            this.events.OnLoadReady((float)source.length);
        }

        private void OnPlaybackEnd(UnityEngine.Video.VideoPlayer source)
        {
            this.events.OnPlaybackEnd();
        }

        public override bool IsPlaying
        {
            get
            {
                return this.player.isPlaying;
            }
        }

        public override float GetDuration()
        {
            return (float)this.player.length;
        }

        public override float GetTime()
        {
            return (float)this.player.time;
        }

        public override void SetTime(float time)
        {
            this.player.time = time;
        }

        public override void LoadURL(string url)
        {
            this.player.url = url;
        }

        public override void Pause()
        {
            this.player.Pause();
            this.events.OnPlaybackStop(this.GetTime());
        }

        public override void Pause(float time)
        {
            this.Pause();
            this.SetTime(time);
        }

        public override void Play()
        {
            this.player.Play();
            this.events.OnPlaybackStart(this.GetTime());
        }

        public override void Play(float time)
        {
            this.SetTime(time);
            this.Play();
        }

        public override void Unload()
        {
            this.player.Stop();
            this.SetTime(0);
            this.player.url = "";
        }

        public override Texture GetScreenTexture()
        {
            Texture result = this.screen.material.GetTexture("_MainTex");

            if (result == null)
            {
                this.screen.GetPropertyBlock(_fetchBlock);
                result = _fetchBlock.GetTexture("_MainTex");
            }

            return result;
        }
    }
}
