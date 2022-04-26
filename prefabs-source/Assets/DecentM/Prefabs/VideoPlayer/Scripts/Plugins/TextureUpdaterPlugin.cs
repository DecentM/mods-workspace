using System;
using UdonSharp;
using UnityEngine;
using VRC.SDK3.Components.Video;
using VRC.SDKBase;
using VRC.Udon;
using TMPro;

namespace DecentM.VideoPlayer.Plugins
{
    public sealed class TextureUpdaterPlugin : VideoPlayerPlugin
    {
        public Texture2D idleTexture;

        protected override void OnPlaybackStart(float duration)
        {
            Texture videoTexture = this.system.GetVideoTexture();

            foreach (Renderer screen in this.system.screens)
            {
                screen.material.SetTexture("_MainTex", videoTexture);
                screen.material.SetInt("_IsAVPro", System.Convert.ToInt32(this.system.currentPlayerHandler.type == VideoPlayerHandlerType.AVPro));
            }

            this.events.OnScreenTextureChange();
        }

        private void ShowIdleTexture()
        {
            foreach (Renderer screen in this.system.screens)
            {
                screen.material.SetTexture("_MainTex", idleTexture);
                screen.material.SetInt("_IsAVPro", 0);
            }

            this.events.OnScreenTextureChange();
        }

        protected override void OnVideoPlayerInit()
        {
            this.ShowIdleTexture();
        }

        protected override void OnUnload()
        {
            this.ShowIdleTexture();
        }

        protected override void OnPlaybackEnd()
        {
            this.ShowIdleTexture();
        }
    }
}
