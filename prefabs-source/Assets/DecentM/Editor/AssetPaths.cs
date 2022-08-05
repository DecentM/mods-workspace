using UnityEngine;
using UnityEditor;

using System.IO;

using DecentM.EditorTools.SelfLocator;

namespace DecentM.EditorTools
{
    public static class AssetPaths
    {
        public static string SelfLocation = SelfLocatorAsset.LocateSelf();

        private const string VideoTexturesPath = "Prefabs/VideoPlayer/Textures/Editor";
        private const string VideoScriptsPath = "Prefabs/VideoPlayer/Scripts/Editor";

        private static T GetAsset<T>(params string[] location) where T : UnityEngine.Object
        {
            string path = $"{SelfLocation}/{string.Join("/", location)}";
            return AssetDatabase.LoadAssetAtPath<T>(path);
        }

        public static Texture2D VideoPlayerBanner
        {
            get { return GetAsset<Texture2D>(VideoTexturesPath, "splash.psd"); }
        }
        public static Sprite FallbackVideoThumbnail
        {
            get { return GetAsset<Sprite>(VideoTexturesPath, "thumbnail-missing.psd"); }
        }

        public static string VideoMetadataFolder
        {
            get { return "Assets/Editor/DecentM/VideoMetadata"; }
        }
        public static string ImageCacheFolder
        {
            get { return "Assets/Editor/DecentM/ImageCache"; }
        }
        public static string SubtitleCacheFolder
        {
            get { return "Assets/Editor/DecentM/SubtitleCache"; }
        }
    }
}
