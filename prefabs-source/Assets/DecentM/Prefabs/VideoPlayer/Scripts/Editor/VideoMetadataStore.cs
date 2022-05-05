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
        public static VideoMetadata GetCachedMetadata(string url)
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
        public static bool IsUrlCached(string url)
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

        private static void FetchMetadataSync(string url)
        {
            if (string.IsNullOrEmpty(url)) return;
            if (!ValidateUrl(url)) return;

            string hash = Hash.String(url);

            try
            {
                YTDLVideoJson json = YTDLCommands.GetMetadataSync(url);
                SaveMetadata(hash, json);
            }
            catch (Exception ex)
            {
                Debug.LogException(ex);
                Debug.LogWarning($"Error while downloading metadata for {url}, skipping this video...");
                return;
            }
        }

        private async static Task FetchMetadataAsync(string url)
        {
            if (string.IsNullOrEmpty(url)) return;
            if (!ValidateUrl(url)) return;

            string hash = Hash.String(url);

            try
            {
                YTDLVideoJson json = await YTDLCommands.GetMetadata(url);
                SaveMetadata(hash, json);
            } catch (Exception ex)
            {
                Debug.LogException(ex);
                Debug.LogWarning($"Error while downloading metadata for {url}, skipping this video...");
                return;
            }
        }

        private async static Task FetchMetadataInParallel(List<string> urls)
        {
            List<Task> tasks = new List<Task>();

            for (int i = 0; i < urls.Count; i++)
            {
                string url = urls[i];
                if (string.IsNullOrEmpty(url)) continue;
                if (!ValidateUrl(url)) continue;

                tasks.Add(FetchMetadataAsync(url));
            }

            await Task.WhenAll(tasks);
        }

        private async static Task FetchMetadataAsync(Queue<string> urls, int batchSize)
        {
            List<string> batch = new List<string>();

            while (urls.Count > 0)
            {
                batch.Add(urls.Dequeue());

                if (batch.Count >= batchSize)
                {
                    await FetchMetadataInParallel(batch);
                    batch.Clear();
                }
            }

            // Process the last batch
            if (batch.Count > 0)
            {
                await FetchMetadataInParallel(batch);
                batch.Clear();
            }
        }

        #region Methods that support the public API

        private static bool isLocked = false;

        public static bool IsLocked
        {
            get { return isLocked; }
        }

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
                if (IsUrlCached(url)) continue;

                queue.Enqueue(url);
            }

            CreateFolders(urls);
            isLocked = true;

            return queue;
        }

        private static void PostprocessAssets()
        {
            AssetDatabase.Refresh();
            isLocked = false;
        }

        private static bool FetchMetadataAsync(string[] urls, Action OnFinish)
        {
            if (isLocked) return false;

            Queue<string> queue = PreprocessAssets(urls);

            Task allFetches = Task.Run(() => FetchMetadataAsync(queue, 4));

            EditorCoroutine.Start(
                Parallelism.WaitForTask(allFetches, (bool success) => {
                    PostprocessAssets();
                    if (!success) Debug.LogError("An error occurred during batched metadata fetching, there is likely more information about it above.");
                    OnFinish();
                })
            );

            return true;
        }

        private static void FetchMetadataSync(Queue<string> urls)
        {
            for (int i = 0; i < urls.Count; i++)
            {
                string url = urls.ElementAt(i);
                if (url == null) continue;

                if (EditorUtility.DisplayCancelableProgressBar("Fetching metadata...", url, 1f * i / urls.Count))
                {
                    break;
                }

                FetchMetadataSync(url);
            }

            EditorUtility.ClearProgressBar();
        }

        private static bool FetchMetadataSync(string[] urls)
        {
            if (isLocked) return false;

            Queue<string> queue = PreprocessAssets(urls);

            try
            {
                FetchMetadataSync(queue);
            } catch (Exception ex)
            {
                PostprocessAssets();
                throw ex;
            }

            return true;
        }

        #endregion

        [PublicAPI]
        public static void Unlock()
        {
            if (!isLocked) return;

            isLocked = false;
        }

        [PublicAPI]
        public static bool RefreshMetadataAsync(string url, Action OnFinish)
        {
            return FetchMetadataAsync(new string[] { url }, OnFinish);
        }

        [PublicAPI]
        public static bool RefreshMetadataAsync(string[] urls, Action OnFinish)
        {
            if (urls.Length == 0) return true;
            return FetchMetadataAsync(urls, OnFinish);
        }

        [PublicAPI]
        public static bool RefreshMetadataAsync(string[] urls)
        {
            return RefreshMetadataAsync(urls, () => { });
        }

        [PublicAPI]
        public static bool RefreshMetadataSync(string url)
        {
            return FetchMetadataSync(new string[] { url });
        }

        [PublicAPI]
        public static bool RefreshMetadataSync(string[] urls)
        {
            if (urls.Length == 0) return true;
            return FetchMetadataSync(urls);
        }
    }
}
