using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace DecentM.EditorTools
{
    public static partial class EditorAssets
    {
        private const string VideoTexturesPath = "Prefabs/VideoPlayer/Textures/Editor";

        public static Texture2D VideoPlayerBanner
        {
            get { return GetAsset<Texture2D>(VideoTexturesPath, "splash.psd"); }
        }

        public static Texture2D FallbackVideoThumbnail
        {
            get { return GetAsset<Texture2D>(VideoTexturesPath, "thumbnail-missing.psd"); }
        }
    }
}
