
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

namespace DecentM.VideoPlayer.Plugins
{
    public class RenderTextureOutput : VideoPlayerPlugin
    {
        public int targetFps = 30;
        public Material[] outputs;

        public string textureProperty = "_EmissionMap";
        public string avProProperty = "_IsAVPro";

        private bool isRunning = false;

        private Texture lastFrame;
        private float elapsed = 0;
        private float fps = 0;

        private void LateUpdate()
        {
            if (!this.isRunning || this.outputs == null || this.outputs.Length == 0) return;

            this.elapsed += Time.unscaledDeltaTime;
            if (elapsed < 1f / fps) return;
            elapsed = 0;

            Texture videoPlayerTex = this.system.GetVideoTexture();

            if (lastFrame != videoPlayerTex)
            {
                foreach (Material material in this.outputs)
                {
                    material.SetTexture(this.textureProperty, videoPlayerTex);
                }

                lastFrame = videoPlayerTex;
            }
        }

        protected override void OnMetadataChange(string title, string uploader, string siteName, int viewCount, int likeCount, string resolution, int fps, string description, string duration, string[][] subtitles)
        {
            this.fps = fps == 0 ? this.targetFps : Mathf.Min(fps, targetFps);
        }

        protected override void OnPlaybackEnd()
        {
            this.isRunning = false;
        }

        protected override void OnPlaybackStart(float timestamp)
        {
            this.isRunning = true;
            if (this.outputs == null || this.outputs.Length == 0) return;

            foreach (Material material in this.outputs)
            {
                material.SetInt(this.avProProperty, this.system.currentPlayerHandler.type == VideoPlayerHandlerType.AVPro ? 1 : 0);
            }
        }

        protected override void OnPlaybackStop(float timestamp)
        {
            this.isRunning = false;
        }

        protected override void OnUnload()
        {
            this.isRunning = false;
        }
    }
}

