using UnityEngine;

namespace DecentM.VideoPlayer.Plugins
{
    public sealed class TextureUpdaterPlugin : VideoPlayerPlugin
    {
        public Texture idleTexture;

        protected override void OnPlaybackStart(float duration)
        {
            Texture videoTexture = this.system.GetVideoTexture();

            this.SetTexture(videoTexture);
        }

        private void ShowIdleTexture()
        {
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

        protected override void OnPlayerSwitch(VideoPlayerHandlerType type)
        {
            foreach (ScreenHandler screen in this.system.screens)
            {
                screen.SetIsAVPro(type == VideoPlayerHandlerType.AVPro);
            }
        }

        public void SetTexture(Texture texture)
        {
            foreach (ScreenHandler screen in this.system.screens)
            {
                screen.SetTexture(texture);
            }

            this.events.OnScreenTextureChange();
        }
    }
}
