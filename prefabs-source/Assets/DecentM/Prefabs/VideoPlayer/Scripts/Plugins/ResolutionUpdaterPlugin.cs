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
        public bool dynamicResolution = false;

        public Vector2Int defaultResolution = new Vector2Int(1920, 1080);

        private void ChangeScreenResolution(ScreenHandler screen, float width, float height)
        {
            if (this.dynamicResolution)
            {
                float aspectRatio = width / height;

                screen.SetSize(new Vector2(screen.transform.localScale.x, screen.transform.localScale.x / aspectRatio));
                screen.SetAspectRatio(aspectRatio);
            }

            this.events.OnScreenResolutionChange(screen, width, height);
        }

        private void ChangeScreenResolution()
        {
            Texture videoTexture = this.system.GetVideoTexture();

            if (videoTexture == null) return;

            foreach (ScreenHandler screen in this.system.screens)
            {
                float w = videoTexture.width;
                float h = videoTexture.height;

                this.ChangeScreenResolution(screen, w, h);
            }
        }

        protected override void OnPlaybackStart(float duration)
        {
            this.ChangeScreenResolution();
        }

        protected override void OnScreenTextureChange()
        {
            this.ChangeScreenResolution();
        }
    }
}
