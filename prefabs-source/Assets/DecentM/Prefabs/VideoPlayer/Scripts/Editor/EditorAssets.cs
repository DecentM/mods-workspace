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

        public static Texture2D DeleteIcon
        {
            get { return GetAsset<Texture2D>(VideoTexturesPath, "delete.png");  }
        }

        public static Texture2D RefreshIcon
        {
            get { return GetAsset<Texture2D>(VideoTexturesPath, "refresh.png"); }
        }

        public static Texture2D ChevronUp
        {
            get { return GetAsset<Texture2D>(VideoTexturesPath, "chevron-up.png"); }
        }

        public static Texture2D ChevronDown
        {
            get { return GetAsset<Texture2D>(VideoTexturesPath, "chevron-down.png"); }
        }

        public static Texture2D ChevronDoubleUp
        {
            get { return GetAsset<Texture2D>(VideoTexturesPath, "chevron-double-up.png"); }
        }

        public static Texture2D ChevronDoubleDown
        {
            get { return GetAsset<Texture2D>(VideoTexturesPath, "chevron-double-down.png"); }
        }

        public static Texture2D CloseIcon
        {
            get { return GetAsset<Texture2D>(VideoTexturesPath, "close.png"); }
        }

        public static Texture2D PlusIcon
        {
            get { return GetAsset<Texture2D>(VideoTexturesPath, "plus.png"); }
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
