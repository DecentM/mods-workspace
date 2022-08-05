using UnityEngine;

namespace DecentM.Prefabs.VideoPlayer.Handlers
{
    public enum VideoPlayerHandlerType
    {
        AVPro,
        Unity,
        CVR,
        VLC,
    }

    public abstract class PlayerHandler : MonoBehaviour
    {
        public abstract VideoPlayerHandlerType type { get; }
        public VideoPlayerEvents events;

        protected PlayerHandler playerHandler;

        public void RegisterPlayerHandler(PlayerHandler playerHandler)
        {
            this.playerHandler = playerHandler;
        }

        public abstract bool IsPlaying { get; }

        public abstract float GetDuration();

        public abstract float GetTime();

        public abstract void SetTime(float time);

        public abstract void LoadURL(string url);

        public abstract void Play();

        public abstract void Play(float time);

        public abstract void Unload();

        public abstract void Pause();

        public abstract void Pause(float time);

        public abstract Texture GetScreenTexture();

        protected virtual void OnVideoUnload()
        {
            this.events.OnUnload();
        }

        protected virtual void OnProgress()
        {
            this.events.OnProgress(this.GetTime(), this.GetDuration());
        }

        protected virtual void OnVideoEnd()
        {
            this.events.OnPlaybackEnd();
        }

        protected virtual void OnVideoPause()
        {
            this.events.OnPlaybackStop(this.GetTime());
        }

        protected virtual void OnVideoPlay()
        {
            this.events.OnPlaybackStart(this.GetTime());
        }

        protected virtual void OnVideoReady()
        {
            this.events.OnLoadReady(this.GetDuration());
        }

        protected virtual void OnVideoError()
        {
            this.events.OnLoadError(new VideoError(VideoErrorType.Unknown));
        }
    }
}

