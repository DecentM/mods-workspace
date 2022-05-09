using System.IO;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace DecentM.EditorTools
{
    public static partial class EditorAssets
    {
        private const string VideoTexturesPath = "Prefabs/VideoPlayer/Textures/Editor";
        private const string VideoScriptsPath = "Prefabs/VideoPlayer/Scripts/Editor";

        public static Texture2D VideoPlayerBanner { get { return GetAsset<Texture2D>(VideoTexturesPath, "splash.psd"); } }
        public static Sprite FallbackVideoThumbnail { get { return GetAsset<Sprite>(VideoTexturesPath, "thumbnail-missing.psd"); } }

        public static string YtDlpPath { get { return string.Join("/", Directory.GetCurrentDirectory(), "Assets", "DecentM", VideoScriptsPath, "Bin/yt-dlp.exe"); }}

        public static string VideoMetadataFolder { get { return "Assets/Editor/DecentM/VideoMetadata"; }}
        public static string ImageCacheFolder { get { return "Assets/Editor/DecentM/ImageCache"; }}
        public static string SubtitleCacheFolder { get { return "Assets/Editor/DecentM/SubtitleCache"; }}
    }
}
