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

        protected override void OnScreenTextureChange()
        {
            if (this.outputs == null || this.outputs.Length == 0) return;

            Texture videoPlayerTex = this.system.GetVideoTexture();

            foreach (Material material in this.outputs)
            {
                material.SetTexture(this.textureProperty, videoPlayerTex);
                material.SetInt(this.avProProperty, this.system.currentPlayerHandler.type == VideoPlayerHandlerType.AVPro ? 1 : 0);
            }
        }
    }
}
