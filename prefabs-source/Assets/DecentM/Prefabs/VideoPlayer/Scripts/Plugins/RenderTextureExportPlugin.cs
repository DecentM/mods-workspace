using UnityEngine;

namespace DecentM.Prefabs.VideoPlayer.Plugins
{
    public class RenderTextureExportPlugin : VideoPlayerPlugin
    {
        public float targetFps = 24;
        public RenderTexture renderTexture;

        protected override void _Start()
        {
            this.fps = this.targetFps;
        }

        private float fps = 24;
        private bool running = false;
        private float elapsed = 0;

        private void LateUpdate()
        {
            if (!this.running)
                return;

            this.elapsed += Time.unscaledDeltaTime;
            if (elapsed < 1 / this.fps)
                return;
            this.elapsed = 0;

            this.system.RenderCurrentFrame(this.renderTexture);
        }

        protected override void OnMetadataChange(
            string title,
            string uploader,
            string siteName,
            int viewCount,
            int likeCount,
            string resolution,
            int fps,
            string description,
            string duration,
            TextAsset[] subtitles
        )
        {
            if (fps > 0)
                this.fps = Mathf.Min(fps, this.targetFps);
            else
                this.fps = this.targetFps;
        }

        protected override void OnPlaybackEnd()
        {
            this.running = false;
        }

        protected override void OnPlaybackStart(float timestamp)
        {
            this.running = true;
        }

        protected override void OnUnload()
        {
            this.running = false;
        }
    }
}
