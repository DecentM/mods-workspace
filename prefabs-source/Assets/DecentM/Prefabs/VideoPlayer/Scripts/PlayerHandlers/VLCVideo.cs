using System;
using System.Collections;
using System.Threading.Tasks;

using UnityEngine;
using LibVLCSharp;

using DecentM.Shared;
using DecentM.Shared.YTdlp;
using DecentM.Mods.CustomComponents.VideoPlayer;

namespace DecentM.Prefabs.VideoPlayer.Handlers
{
    public class VLCVideo : PlayerHandler
    {
        public override VideoPlayerHandlerType type => VideoPlayerHandlerType.VLC;

        public VLCPlayer player;
        public MeshRenderer screen;

        private MaterialPropertyBlock _fetchBlock;

        private void Start()
        {
            this._fetchBlock = new MaterialPropertyBlock();

            this.player.mediaPlayer.Playing += this.HandlePlay;
            this.player.mediaPlayer.Paused += this.HandlePause;
            this.player.mediaPlayer.Stopped += this.HandleStopped;
            this.player.TextureChanged += this.HandleTextureChanged;

            this.player.mediaPlayer.MediaChanged += this.HandleMediaChanged;
            this.player.mediaPlayer.EncounteredError += this.HandleError;
        }

        private float elapsed = 0;

        private void FixedUpdate()
        {
            if (!this.IsPlaying)
                return;

            this.elapsed += Time.fixedDeltaTime;
            if (this.elapsed < 1) return;
            this.elapsed = 0;

            this.OnProgress();
        }

        private void HandlePause(object sender, object args)
        {
            this.OnVideoPause();
        }

        private void HandlePlay(object sender, object args)
        {
            if (textureChangeSent == false)
                return;

            this.OnVideoPause();
        }

        private bool textureChangeSent = false;

        private void HandleTextureChanged(object sender, object args)
        {
            if (this.IsPlaying && !this.textureChangeSent)
            {
                this.OnVideoPlay();
                this.textureChangeSent = true;
            }
        }

        private void HandleStopped(object sender, object args)
        {
            this.textureChangeSent = false;
            this.OnVideoUnload();
        }

        private void HandleMediaChanged(object sender, object args)
        {
            this.textureChangeSent = false;
            this.OnVideoReady();
        }

        private void HandleError(object sender, object args)
        {
            this.textureChangeSent = false;
            this.OnVideoError();
        }

        public override bool IsPlaying
        {
            get
            {
                return this.player.IsPlaying;
            }
        }

        public override float GetDuration()
        {
            return this.player.Duration / 1000;
        }

        public override float GetTime()
        {
            return this.player.Time / 1000;
        }

        public override void SetTime(float time)
        {
            // this.player.mediaPlayer.SeekTo(new TimeSpan(0, 0, 0, Mathf.FloorToInt(time)));
            this.player.SetTime((long)time * 1000);
        }

        public override void LoadURL(string url)
        {
            this.StartCoroutine(this.LoadURLCoroutine(url));
        }

        private IEnumerator LoadURLCoroutine(string url)
        {
            string newUrl = url;

            Debug.Log($"[DecentM.VideoPlayer] Resolving url for playback: {url}");

            yield return Parallelism.WaitForCallback((callback) =>
            {
                YTdlpCommands.GetVideoUrl(url, new Vector2Int(1920, 1080), (string resolved) =>
                {
                    newUrl = resolved;
                    callback();
                });
            });

            Debug.Log($"[DecentM.VideoPlayer] Resolved to: {newUrl}");

            Media media = new Media(LibVLCSingleton.GetInstance(), new Uri(newUrl));

            Task parseStatusTask = Task.Run(async () => await media.ParseAsync(MediaParseOptions.ParseNetwork));

            yield return Parallelism.WaitForTask(parseStatusTask, (success) =>
            {
                if (!success)
                {
                    this.events.OnLoadError(new VideoError(VideoErrorType.Unknown));
                    return;
                }
            });

            this.player.Open(media);
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
            this.player.Close();
            this.SetTime(0);
        }

        public override Texture GetScreenTexture()
        {
            Texture result = this.screen.material.GetTexture("_MainTex");

            if (result == null)
            {
                result = this.screen.material.mainTexture;
            }

            if (result == null)
            {
                this.screen.GetPropertyBlock(_fetchBlock);
                result = _fetchBlock.GetTexture("_MainTex");
            }

            if (result == null)
            {
                result = this.player.GetTexture();
            }

            return result;
        }
    }
}
