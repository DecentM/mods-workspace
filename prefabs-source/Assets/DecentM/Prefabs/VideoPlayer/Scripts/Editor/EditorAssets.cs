using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace DecentM.EditorTools
{
    public static partial class EditorAssets
    {
        private const string VideoTexturesPath = "Prefabs/VideoPlayer/Textures/Editor";
        private const string VideoScriptsPath = "Prefabs/VideoPlayer/Scripts/Editor";

        public static Texture2D VideoPlayerBanner
        {
            get { return GetAsset<Texture2D>(VideoTexturesPath, "splash.psd"); }
        }

        public static Texture2D FallbackVideoThumbnail
        {
            get { return GetAsset<Texture2D>(VideoTexturesPath, "thumbnail-missing.psd"); }
        }

        public static string YtDlpPath
        {
            get { return string.Join("/", Application.dataPath, "DecentM", VideoScriptsPath, "Bin/yt-dlp.exe"); }
        }

        public static string VideoMetadataFolder
        {
            get { return "Assets/Editor/DecentM/VideoMetadata"; }
        }
    }
}
