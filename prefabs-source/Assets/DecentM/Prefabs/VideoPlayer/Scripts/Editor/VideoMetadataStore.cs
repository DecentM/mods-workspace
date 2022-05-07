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

using DecentM.EditorTools;
using DecentM.Subtitles;

namespace DecentM.VideoPlayer
{
    public struct VideoMetadata
    {
        public string duration;
        public string title;
        public string uploader;
        public int viewCount;
        public int likeCount;
        public Texture2D thumbnail;
        public string resolution;
        public int fps;
        public string siteName;
        public string description;
        public CachedSubtitle[] subtitles;
    }

    public class VideoMetadataStore
    {
        private static bool ValidateUrl(string url)
        {
            return Uri.TryCreate(url, UriKind.Absolute, out Uri result);
        }

        private static YTDLVideoJson? GetCachedYTDLJson(string url)
        {
            if (!ValidateUrl(url)) return null;

            string filename = "metadata.json";
            string hash = Hash.String(url);
            string path = $"{EditorAssets.VideoMetadataFolder}/{hash}/{filename}";

            TextAsset textAsset = AssetDatabase.LoadAssetAtPath<TextAsset>(path);

            if (textAsset == null) return null;

            try
            {
                return JsonUtility.FromJson<YTDLVideoJson>(textAsset.text);
            }
            catch
            {
                return null;
            }
        }

        [PublicAPI]
        public static VideoMetadata GetCached(string url)
        {
            VideoMetadata metadata = new VideoMetadata();

            if (!ValidateUrl(url)) return metadata;

            try
            {
                YTDLVideoJson? jsonOrNull = GetCachedYTDLJson(url);
                if (jsonOrNull == null) return metadata;
                YTDLVideoJson json = (YTDLVideoJson)jsonOrNull;

                metadata.fps = json.fps;
                metadata.duration = json.duration_string;
                metadata.title = json.title;
                metadata.resolution = json.resolution;
                metadata.uploader = json.uploader;
                metadata.viewCount = 0;
                metadata.likeCount = 0;
                metadata.thumbnail = ImageStore.GetFromCache(json.thumbnail);
                metadata.siteName = json.extractor_key;
                metadata.description = json.description;
                metadata.duration = json.duration_string;
                metadata.subtitles = SubtitleStore.GetFromCache(url);

                int.TryParse(json.like_count, out metadata.likeCount);
                int.TryParse(json.view_count, out metadata.viewCount);
            } catch
            {
                return metadata;
            }

            return metadata;
        }

        [PublicAPI]
        public static bool IsCached(string url)
        {
            if (!ValidateUrl(url)) return false;

            YTDLVideoJson? jsonOrNull = GetCachedYTDLJson(url);
            return jsonOrNull != null;
        }

        private static void SaveMetadata(string hash, YTDLVideoJson? jsonOrNull)
        {
            if (jsonOrNull == null) return;

            YTDLVideoJson json = (YTDLVideoJson)jsonOrNull;

            string path = $"{EditorAssets.VideoMetadataFolder}/{hash}/metadata.json";
            byte[] bytes = Encoding.UTF8.GetBytes(JsonUtility.ToJson(json));
            File.WriteAllBytes(path, bytes);
        }

        private static IEnumerator Fetch(string url)
        {
            if (string.IsNullOrEmpty(url)) return null;
            if (!ValidateUrl(url)) return null;

            string hash = Hash.String(url);

            return YTDLCommands.GetMetadata(url, (json) => SaveMetadata(hash, json));
        }

        private static IEnumerator FetchInParallel(List<string> urls)
        {
            List<DCoroutine> coroutines = new List<DCoroutine>();

            for (int i = 0; i < urls.Count; i++)
            {
                string url = urls[i];
                if (string.IsNullOrEmpty(url)) continue;
                if (!ValidateUrl(url)) continue;

                coroutines.Add(DCoroutine.Start(Fetch(url)));
            }

            return Parallelism.WaitForCoroutines(coroutines);
        }

        private static IEnumerator Fetch(Queue<string> urls, int batchSize, Action OnFinish)
        {
            List<string> batch = new List<string>();

            while (urls.Count > 0)
            {
                batch.Add(urls.Dequeue());

                if (batch.Count >= batchSize)
                {
                    yield return FetchInParallel(batch);
                    batch.Clear();
                }
            }

            // Process the last batch
            if (batch.Count > 0)
            {
                yield return FetchInParallel(batch);
                batch.Clear();
            }

            OnFinish();
        }

        #region Methods that support the public API

        private static void CreateFolders(string[] urls)
        {
            List<Tuple<string, string>> folders = new List<Tuple<string, string>>();

            foreach (string url in urls)
            {
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
                if (string.IsNullOrEmpty(url)) continue;
                if (!ValidateUrl(url)) continue;
                if (IsCached(url)) continue;

                queue.Enqueue(url);
            }

            CreateFolders(urls);

            return queue;
        }

        private static void PostprocessAssets()
        {
            AssetDatabase.Refresh();
        }

        private static bool Fetch(string[] urls, Action OnFinish)
        {
            Queue<string> queue = PreprocessAssets(urls);

            void Callback()
            {
                PostprocessAssets();
                OnFinish();
            }

            DCoroutine.Start(Fetch(queue, 4, Callback));

            return true;
        }

        #endregion

        [PublicAPI]
        public static bool Refresh(string url, Action OnFinish)
        {
            return Fetch(new string[] { url }, OnFinish);
        }

        [PublicAPI]
        public static bool Refresh(string[] urls, Action OnFinish)
        {
            if (urls.Length == 0) return true;
            return Fetch(urls, OnFinish);
        }

        [PublicAPI]
        public static bool Refresh(string[] urls)
        {
            return Refresh(urls, () => { });
        }
    }
}
