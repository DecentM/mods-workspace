using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

namespace DecentM.VideoPlayer.Plugins
{
    public class TextureReferencePlugin : VideoPlayerPlugin
    {
        public Material[] outputs;

        public string textureProperty = "_EmissionMap";
        public string avProProperty = "_IsAVProInput";

        protected override void OnScreenTextureChange()
        {
            if (this.outputs == null || this.outputs.Length == 0) return;

            Texture videoPlayerTex = this.system.GetVideoTexture();

            foreach (Material material in this.outputs)
            {
                material.SetTexture(this.textureProperty, videoPlayerTex);
            }
        }

        protected override void OnPlayerSwitch(VideoPlayerHandlerType type)
        {
            foreach (Material material in this.outputs)
            {
                material.SetInt(this.avProProperty, type == VideoPlayerHandlerType.AVPro ? 1 : 0);
            }
        }
    }
}
