using JetBrains.Annotations;
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

        protected override void OnPlaybackStart(float duration)
        {
            Texture videoTexture = this.system.GetVideoTexture();

            this.UpdateMaterial();
            this.SetTexture(videoTexture);
        }

        private void ShowIdleTexture()
        {
            this.UpdateMaterial();
            this.SetTexture(idleTexture);
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

        private void UpdateMaterial()
        {
            bool isAVPro = this.system.currentPlayerHandler.type == VideoPlayerHandlerType.AVPro;

            foreach (Renderer screen in this.system.screens)
            {
                screen.material = isAVPro ? this.avproPlayerMaterial : this.unityPlayerMaterial;
            }
        }

        [PublicAPI]
        public void SetTexture(Texture texture)
        {
            foreach (Renderer screen in this.system.screens)
            {
                // screen.material.SetTexture("_MainTex", texture);
                screen.material.SetTexture("_EmissionMap", texture);
            }

            this.events.OnScreenTextureChange();
        }
    }
}
