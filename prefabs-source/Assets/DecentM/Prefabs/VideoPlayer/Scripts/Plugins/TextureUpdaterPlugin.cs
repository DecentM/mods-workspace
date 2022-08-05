using UnityEngine;

using DecentM.Prefabs.VideoPlayer.Handlers;

namespace DecentM.Prefabs.VideoPlayer.Plugins
{
    public sealed class TextureUpdaterPlugin : VideoPlayerPlugin
    {
        public Texture idleTexture;

        protected override void OnPlaybackStart(float duration)
        {
            Texture videoTexture = this.system.GetVideoTexture();

            if (videoTexture == null)
                return;

            this.SetTexture(videoTexture);
        }

        private void ShowIdleTexture()
        {
            this.SetTexture(idleTexture);
        }

        private void SetAVPro(bool isAVPro)
        {
            foreach (ScreenHandler screen in this.system.screens)
            {
                screen.SetIsAVPro(isAVPro);
            }
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

        protected override void OnPlayerSwitch(VideoPlayerHandlerType type)
        {
            this.SetAVPro(type == VideoPlayerHandlerType.AVPro);
        }

        public void SetTexture(Texture texture)
        {
            if (texture == null)
                return;

            foreach (ScreenHandler screen in this.system.screens)
            {
                screen.SetTexture(texture);
            }

            this.events.OnScreenTextureChange();
        }
    }
}
