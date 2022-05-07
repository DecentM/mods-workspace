using System;
using System.Linq;
using System.Net;
using System.IO;
using System.Text;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Threading;

using JetBrains.Annotations;

using UnityEngine;
using UnityEditor;

namespace DecentM.EditorTools
{
    public class ImageStore
    {
        private static bool ValidateUrl(string url)
        {
            return Uri.TryCreate(url, UriKind.Absolute, out Uri result);
        }

        private static void ApplyImportSettings()
        {
            List<string> files = Directory.GetFiles($"{EditorAssets.ImageCacheFolder}", "*.jpg", SearchOption.TopDirectoryOnly)
                .ToList();

            for (int i = 0; i < files.Count; i++)
            {
                string file = files[i];
                if (string.IsNullOrEmpty(file)) continue;

                EditorUtility.DisplayProgressBar("Reapplying import settings...", Path.GetFileName(file), (float)i / files.Count);

                AssetImporter importer = AssetImporter.GetAtPath(file);
                if (importer == null || !(importer is TextureImporter)) continue;

                TextureImporter textureImporter = (TextureImporter)importer;

                // If it's imported as a sprite, we assume it's done by us so that we don't keep re-crunching already imported images.
                // This also lets the user change the other settings without us resetting them every time.
                if (textureImporter.textureType == TextureImporterType.Sprite) continue;

                textureImporter.textureType = TextureImporterType.Sprite;
                textureImporter.mipmapEnabled = false;
                textureImporter.wrapMode = TextureWrapMode.Clamp;
                textureImporter.maxTextureSize = 1024;
                textureImporter.textureCompression = TextureImporterCompression.Compressed;
                textureImporter.compressionQuality = 50;
                textureImporter.crunchedCompression = true;
                textureImporter.SaveAndReimport();
            }

            EditorUtility.ClearProgressBar();
        }

        private static void PostprocessAssets(string[] urls)
        {
            ApplyImportSettings();
        }

        private static string GetPathFromUrl(string url)
        {
            if (!ValidateUrl(url)) return null;

            string hash = Hash.String(url);
            return $"{EditorAssets.ImageCacheFolder}/{hash}";
        }

        private static IEnumerator Fetch(string url)
        {
            if (string.IsNullOrEmpty(url)) return null;
            if (!ValidateUrl(url)) return null;

            string path = GetPathFromUrl(url);

            return YTDLCommands.DownloadThumbnail(url, path);
        }

        private static IEnumerator FetchInParallel(List<string> urls, Action OnFinish)
        {
            List<DCoroutine> coroutines = new List<DCoroutine>();

            for (int i = 0; i < urls.Count; i++)
            {
                string url = urls[i];
                if (string.IsNullOrEmpty(url)) continue;
                if (!ValidateUrl(url)) continue;

                coroutines.Add(DCoroutine.Start(Fetch(url)));
            }

            return Parallelism.WaitForCoroutines(coroutines, OnFinish);
        }

        private static DCoroutine Fetch(Queue<string> urls, int batchSize, Action OnFinish, Action<int> OnQueueSizeChange)
        {
            if (urls.Count == 0)
            {
                OnFinish();
                return null;
            }

            List<string> batch = new List<string>();

            while (urls.Count > 0 && batch.Count < batchSize)
            {
                batch.Add(urls.Dequeue());
                OnQueueSizeChange(urls.Count);
            }

            return DCoroutine.Start(FetchInParallel(batch, () => Fetch(urls, batchSize, OnFinish, OnQueueSizeChange)));
        }

        [PublicAPI]
        public static Texture2D GetCached(string url)
        {
            // TODO: Implement getting a cached image from the folder
            return null;

            if (!ValidateUrl(url)) return null;

            string path = GetPathFromUrl(url);
            return AssetDatabase.LoadAssetAtPath<Texture2D>(path);
        }

        [PublicAPI]
        public static bool IsCached(string url)
        {
            if (!ValidateUrl(url)) return false;

            Texture2D texture = GetCached(url);
            return texture != null;
        }

        private static void CreateFolders(string[] urls)
        {
            List<Tuple<string, string>> folders = new List<Tuple<string, string>>();

            foreach (string url in urls)
            {
                string hash = Hash.String(url);
                folders.Add(new Tuple<string, string>($"{EditorAssets.ImageCacheFolder}", hash));
            }

            Assets.CreateFolders(folders);
        }

        private static Queue<string> PreprocessAssets(string[] urls)
        {
            Queue<string> queue = new Queue<string>();

            for (int i = 0; i < urls.Length; i++)
            {
                string url = urls[i];
                if (string.IsNullOrEmpty(url)) continue;
                if (!ValidateUrl(url)) continue;
                if (IsCached(url)) continue;

                queue.Enqueue(url);
            }

            CreateFolders(urls);

            return queue;
        }

        private static bool Fetch(string[] urls, Action<float> OnProgress, Action OnFinish)
        {
            Queue<string> queue = PreprocessAssets(urls);

            void Callback()
            {
                AsyncProgress.Clear();
                PostprocessAssets(urls);
                OnFinish();
            }

            void OnQueueSizeChange(int newSize)
            {
                OnProgress((urls.Length - newSize) / (float)urls.Length);
            }

            Fetch(queue, 4, Callback, OnQueueSizeChange);

            return true;
        }

        [PublicAPI]
        public static bool Refresh(string url, Action<float> OnProgress, Action OnFinish)
        {
            return Fetch(new string[] { url }, OnProgress, OnFinish);
        }

        [PublicAPI]
        public static bool Refresh(string[] urls, Action<float> OnProgress, Action OnFinish)
        {
            if (urls.Length == 0) return true;
            return Fetch(urls, OnProgress, OnFinish);
        }

        [PublicAPI]
        public static bool Refresh(string[] urls, Action<float> OnProgress)
        {
            return Refresh(urls, OnProgress, () => { });
        }
    }
}
