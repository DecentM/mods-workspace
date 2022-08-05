using System;
using System.IO;
using System.Text;
using System.Collections;
using System.Collections.Generic;

using JetBrains.Annotations;

using UnityEngine;
using UnityEditor;

using DecentM.EditorTools;
using DecentM.Shared;
using DecentM.Prefabs.VideoPlayer.EditorTools.Importers;

namespace DecentM.Prefabs.VideoPlayer.EditorTools
{
    public struct VideoMetadata
    {
        public string duration;
        public string title;
        public string uploader;
        public int viewCount;
        public int likeCount;
        public Sprite thumbnail;
        public string resolution;
        public int fps;
        public string siteName;
        public string description;
        public TextAsset[] subtitles;
    }

    public class VideoMetadataStore
    {
        private static bool ValidateUrl(string url)
        {
            return Uri.TryCreate(url, UriKind.Absolute, out Uri result);
        }

        private static YTDLVideoJson? GetCachedYTDLJson(string url)
        {
            if (!ValidateUrl(url))
                return null;

            string filename = "metadata.ytdl-json";
            string hash = Hash.String(url);
            string path = $"{EditorAssets.VideoMetadataFolder}/{hash}/{filename}";

            VideoMetadataAsset asset = AssetDatabase.LoadAssetAtPath<VideoMetadataAsset>(path);

            if (asset == null)
                return null;

            return asset.metadata;
        }

        [PublicAPI]
        public static VideoMetadata GetCached(string url)
        {
            VideoMetadata metadata = new VideoMetadata();

            if (!ValidateUrl(url))
                return metadata;

            try
            {
                YTDLVideoJson? jsonOrNull = GetCachedYTDLJson(url);
                if (jsonOrNull == null)
                    return metadata;
                YTDLVideoJson json = (YTDLVideoJson)jsonOrNull;

                metadata.fps = json.fps;
                metadata.duration = json.duration_string;
                metadata.title = json.title;
                metadata.resolution = json.resolution;
                metadata.uploader = json.uploader;
                metadata.viewCount = 0;
                metadata.likeCount = 0;
                metadata.thumbnail = ImageStore.GetCached(url);
                metadata.siteName = json.extractor_key;
                metadata.description = json.description;
                metadata.duration = json.duration_string;
                metadata.subtitles = SubtitleStore.GetFromCache(url);

                int.TryParse(json.like_count, out metadata.likeCount);
                int.TryParse(json.view_count, out metadata.viewCount);
            }
            catch
            {
                return metadata;
            }

            return metadata;
        }

        [PublicAPI]
        public static bool IsCached(string url)
        {
            if (!ValidateUrl(url))
                return false;

            YTDLVideoJson? jsonOrNull = GetCachedYTDLJson(url);
            return jsonOrNull != null;
        }

        private static void Save(string hash, YTDLVideoJson? jsonOrNull)
        {
            if (jsonOrNull == null)
                return;

            YTDLVideoJson json = (YTDLVideoJson)jsonOrNull;

            string path = $"{EditorAssets.VideoMetadataFolder}/{hash}/metadata.ytdl-json";
            byte[] bytes = Encoding.UTF8.GetBytes(JsonUtility.ToJson(json));
            File.WriteAllBytes(path, bytes);
        }

        private static IEnumerator Fetch(string url)
        {
            if (string.IsNullOrEmpty(url))
                return null;
            if (!ValidateUrl(url))
                return null;

            string hash = Hash.String(url);

            return YTDLCommands.GetMetadata(url, (json) => Save(hash, json));
        }

        private static IEnumerator FetchInParallel(List<string> urls, Action OnFinish)
        {
            List<DCoroutine> coroutines = new List<DCoroutine>();

            for (int i = 0; i < urls.Count; i++)
            {
                string url = urls[i];
                if (string.IsNullOrEmpty(url))
                    continue;
                if (!ValidateUrl(url))
                    continue;

                coroutines.Add(DCoroutine.Start(Fetch(url)));
            }

            return Parallelism.WaitForCoroutines(coroutines, OnFinish);
        }

        private static DCoroutine Fetch(
            Queue<string> urls,
            int batchSize,
            Action OnFinish,
            Action<int> OnQueueSizeChange
        )
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

            return DCoroutine.Start(
                FetchInParallel(batch, () => Fetch(urls, batchSize, OnFinish, OnQueueSizeChange))
            );
        }

        #region Methods that support the public API

        private static void CreateFolders(string[] urls)
        {
            List<Tuple<string, string>> folders = new List<Tuple<string, string>>();

            foreach (string url in urls)
            {
                if (!ValidateUrl(url))
                    continue;

                string hash = Hash.String(url);
                folders.Add(new Tuple<string, string>($"{EditorAssets.VideoMetadataFolder}", hash));
            }

            Assets.CreateFolders(folders);
        }

        private static Queue<string> PreprocessAssets(string[] urls)
        {
            Queue<string> queue = new Queue<string>();

            for (int i = 0; i < urls.Length; i++)
            {
                string url = urls[i];
                if (string.IsNullOrEmpty(url))
                    continue;
                if (!ValidateUrl(url))
                    continue;
                if (IsCached(url))
                    continue;

                queue.Enqueue(url);
            }

            CreateFolders(urls);

            return queue;
        }

        private static void PostprocessAssets()
        {
            AssetDatabase.Refresh();
        }

        private static bool Fetch(string[] urls, Action<float> OnProgress, Action OnFinish)
        {
            Queue<string> queue = PreprocessAssets(urls);

            void Callback()
            {
                AsyncProgress.Clear();
                PostprocessAssets();
                OnFinish();
            }

            void OnQueueSizeChange(int newSize)
            {
                OnProgress((urls.Length - newSize) / (float)urls.Length);
            }

            Fetch(queue, 4, Callback, OnQueueSizeChange);

            return true;
        }

        #endregion

        [PublicAPI]
        public static bool Refresh(string url, Action<float> OnProgress, Action OnFinish)
        {
            return Fetch(new string[] { url }, OnProgress, OnFinish);
        }

        [PublicAPI]
        public static bool Refresh(string[] urls, Action<float> OnProgress, Action OnFinish)
        {
            if (urls.Length == 0)
                return true;
            return Fetch(urls, OnProgress, OnFinish);
        }

        [PublicAPI]
        public static bool Refresh(string[] urls, Action<float> OnProgress)
        {
            return Refresh(urls, OnProgress, () => { });
        }
    }
}
