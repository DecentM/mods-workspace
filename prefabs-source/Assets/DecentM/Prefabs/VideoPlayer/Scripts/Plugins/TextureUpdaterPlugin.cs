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
        public Texture idleTexture;

        public Material unityPlayerMaterial;
        public Material avproPlayerMaterial;

        private void UpdateMaterial(bool isAVPro)
        {
            foreach (Renderer screen in this.system.screens)
            {
                screen.material = isAVPro ? this.avproPlayerMaterial : this.unityPlayerMaterial;
            }
        }

        protected override void OnPlaybackStart(float duration)
        {
            this.UpdateMaterial(this.system.currentPlayerHandler.type == VideoPlayerHandlerType.AVPro);

            Texture videoTexture = this.system.GetVideoTexture();

            foreach (Renderer screen in this.system.screens)
            {
                // screen.material.SetTexture("_MainTex", videoTexture);
                screen.material.SetTexture("_EmissionMap", videoTexture);
            }

            this.events.OnScreenTextureChange();
        }

        private void ShowIdleTexture()
        {
            this.UpdateMaterial(false);

            foreach (Renderer screen in this.system.screens)
            {
                // screen.material.SetTexture("_MainTex", idleTexture);
                screen.material.SetTexture("_EmissionMap", idleTexture);
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
