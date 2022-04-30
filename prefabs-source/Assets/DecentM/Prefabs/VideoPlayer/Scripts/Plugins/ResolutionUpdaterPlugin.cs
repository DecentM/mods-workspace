using System;
using UdonSharp;
using UnityEngine;
using VRC.SDK3.Components.Video;
using VRC.SDKBase;
using VRC.Udon;
using TMPro;

namespace DecentM.VideoPlayer.Plugins
{
    public sealed class ResolutionUpdaterPlugin : VideoPlayerPlugin
    {
        public Vector2Int defaultResolution = new Vector2Int(1920, 1080);

        private void ChangeScreenResolution(Renderer screen, float width, float height)
        {
            float aspectRatio = width / height;

            // screen.transform.localScale = new Vector3(screen.transform.localScale.x, screen.transform.localScale.x / aspectRatio, screen.transform.localScale.z);
            screen.material.SetFloat("_TargetAspectRatio", (float)width / (float)height);

            this.events.OnScreenResolutionChange(screen, width, height);
        }

        protected override void OnPlaybackStart(float duration)
        {
            Texture videoTexture = this.system.GetVideoTexture();

            if (videoTexture == null) return;

            foreach (Renderer screen in this.system.screens)
            {
                float w = videoTexture.width;
                float h = videoTexture.height;

                this.ChangeScreenResolution(screen, w, h);
            }
        }

        private void ResetScreenResolution()
        {
            foreach (Renderer screen in this.system.screens)
            {
                this.ChangeScreenResolution(screen, this.defaultResolution.x, this.defaultResolution.y);
            }
        }

        protected override void OnLoadBegin()
        {
            this.ResetScreenResolution();
        }

        protected override void OnPlaybackEnd()
        {
            this.ResetScreenResolution();
        }

        protected override void OnUnload()
        {
            this.ResetScreenResolution();
        }

        protected override void OnVideoPlayerInit()
        {
            this.ResetScreenResolution();
        }
    }
}
