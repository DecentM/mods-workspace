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
    }

    public class VideoMetadataStore
    {
        private static bool ValidateUrl(string url)
        {
            return Uri.TryCreate(url, UriKind.Absolute, out Uri result);
        }

        private static void ApplyThumbnailImportSettings(string path)
        {
            AssetImporter importer = AssetImporter.GetAtPath(path);

            if (importer is TextureImporter)
            {
                TextureImporter textureImporter = (TextureImporter)importer;

                // If it's imported as a sprite, we assume it's done by us so that we don't keep re-crunching already imported images.
                // This also lets the user change the other settings without us resetting them every time.
                if (textureImporter.textureType == TextureImporterType.Sprite) return;

                textureImporter.textureType = TextureImporterType.Sprite;
                textureImporter.mipmapEnabled = false;
                textureImporter.wrapMode = TextureWrapMode.Clamp;
                textureImporter.maxTextureSize = 1024;
                textureImporter.textureCompression = TextureImporterCompression.Compressed;
                textureImporter.compressionQuality = 50;
                textureImporter.crunchedCompression = true;
                textureImporter.SaveAndReimport();
            }
        }

        public static void ReapplyImportSettings()
        {
            string[] files = Directory.GetFiles(string.Join(Application.dataPath, "Assets/Editor/DecentM/VideoMetadata"), "*.jpg", SearchOption.AllDirectories);

            for (int i = 0; i < files.Length; i++)
            {
                if (EditorUtility.DisplayCancelableProgressBar($"Reapplying import settings... ({i} of {files.Length})", files[i], 1f * i / files.Length)) break;
                ApplyThumbnailImportSettings(files[i]);
            }

            EditorUtility.ClearProgressBar();
        }

        private static Texture2D GetCachedThumbnail(string url)
        {
            if (!ValidateUrl(url)) return null;

            string hash = Hash.String(url);
            string filename = "thumbnail.jpg";
            string path = $"{EditorAssets.VideoMetadataFolder}/{hash}/{filename}";
            Texture2D result = AssetDatabase.LoadAssetAtPath<Texture2D>(path);

            return result;
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
                metadata.thumbnail = GetCachedThumbnail(url);
                metadata.siteName = json.extractor_key;
                metadata.description = json.description;
                metadata.duration = json.duration_string;

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
            if (jsonOrNull == null) return false;

            YTDLVideoJson json = (YTDLVideoJson)jsonOrNull;
            Texture2D thumbnail = GetCachedThumbnail(json.thumbnail);

            return thumbnail != null;
        }

        /*
         * * TODO: uncomment when I implement video comments!

            private async static Task FetchComments(string url, string path)
            {
                await YTDLCommands.DownloadMetadataWithComments(url, path);
            }
        */

        private static void CompileSubtitles(string inputPath, string outputPath)
        {
            if (!File.Exists(inputPath) || File.Exists(outputPath)) return;

            try
            {
                string sourceContents = File.ReadAllText(inputPath);
                string extension = Path.GetExtension(inputPath);
                SubtitleCompiler.CompilationResult result = SubtitleCompiler.Compile(sourceContents, extension);

                File.WriteAllBytes(outputPath, Encoding.UTF8.GetBytes(result.output));
            }
            catch (Exception ex)
            {
                Debug.LogWarning(ex);
                Debug.LogWarning($"Failed to compile subtitles, skipping file: {inputPath}");
            }
        }

        private static void CompileAllSubtitles()
        {
            List<string> files = Directory.GetFiles($"{EditorAssets.VideoMetadataFolder}", "*.*", SearchOption.AllDirectories)
                .Where(name => name.EndsWith(".srt") || name.EndsWith(".vtt"))
                .ToList();

            for (int i = 0; i < files.Count; i++)
            {
                string file = files[i];
                if (string.IsNullOrEmpty(file) || !File.Exists(file)) continue;

                EditorUtility.DisplayProgressBar("Compiling subtitles...", Path.GetFileName(file), 1f * i / files.Count);

                string lang = Path.GetExtension(Path.GetFileNameWithoutExtension(file)).Remove(0, 1);
                if (string.IsNullOrEmpty(lang)) lang = Path.GetFileNameWithoutExtension(file);
                string outputPath = $"{Path.GetDirectoryName(file)}/{lang}.txt";
                CompileSubtitles(file, outputPath);
            }

            EditorUtility.ClearProgressBar();
        }

        private static void FetchSubtitlesSync(string url, string hash)
        {
            string path = $"{EditorAssets.VideoMetadataFolder}/{hash}";

            YTDLCommands.DownloadSubtitlesSync(url, $"{path}/Subtitles", false);
        }

        private static Task FetchSubtitlesAsync(string url, string hash)
        {
            string path = $"{EditorAssets.VideoMetadataFolder}/{hash}";

            return YTDLCommands.DownloadSubtitles(url, $"{path}/Subtitles", false);
        }

        private static void FetchThumbnailSync(YTDLVideoJson json, string path)
        {
            WebRequest request = HttpWebRequest.Create(json.thumbnail);
            HttpWebResponse response = (HttpWebResponse)request.GetResponse();

            if ((int)response.StatusCode < 200 || (int)response.StatusCode >= 400)
            {
                throw new Exception($"The web server hosting {request.RequestUri.AbsolutePath} has responded with {(int)response.StatusCode}");
            }

            Stream data = response.GetResponseStream();
            MemoryStream ms = new MemoryStream();
            data.CopyTo(ms);
            byte[] bytes = ms.ToArray();
            File.WriteAllBytes(path, bytes);
        }

        private async static Task FetchThumbnail(YTDLVideoJson json, string path)
        {
            WebRequest request = HttpWebRequest.Create(json.thumbnail);
            HttpWebResponse response = (HttpWebResponse)await request.GetResponseAsync();

            if ((int)response.StatusCode < 200 || (int)response.StatusCode >= 400)
            {
                throw new Exception($"The web server hosting {request.RequestUri.AbsolutePath} has responded with {(int)response.StatusCode}");
            }

            Stream data = response.GetResponseStream();
            MemoryStream ms = new MemoryStream();
            data.CopyTo(ms);
            byte[] bytes = ms.ToArray();
            File.WriteAllBytes(path, bytes);
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

            if (cancellation.IsCancellationRequested) return;

            try
            {
                YTDLVideoJson json = YTDLCommands.GetMetadataSync(url);
                SaveMetadata(hash, json);
                FetchThumbnailSync(json, $"{EditorAssets.VideoMetadataFolder}/{hash}/thumbnail.jpg");
            }
            catch (Exception ex)
            {
                Debug.LogException(ex);
                Debug.LogWarning($"Error while downloading metadata for {url}, skipping this video...");
                return;
            }

            if (cancellation.IsCancellationRequested) return;

            try
            {
                FetchSubtitlesSync(url, hash);
            }
            catch (Exception ex)
            {
                Debug.LogException(ex);
                Debug.LogWarning($"This video won't have subtitles because of the above error.");
            }

            /*
             * TODO: uncomment when I implement video comments!

                try
                {
                    await FetchComments(url, $"{EditorAssets.VideoMetadataFolder}/{hash}/Comments");
                } catch (Exception ex)
                {
                    Debug.LogException(ex);
                    Debug.LogWarning($"This video won't have comments because of the above error.");
                }
            */
        }

        private async static Task FetchMetadataAsync(string url)
        {
            if (string.IsNullOrEmpty(url)) return;
            if (!ValidateUrl(url)) return;

            string hash = Hash.String(url);

            if (cancellation.IsCancellationRequested) return;

            try
            {
                YTDLVideoJson json = await YTDLCommands.GetMetadata(url);
                SaveMetadata(hash, json);
                await FetchThumbnail(json, $"{EditorAssets.VideoMetadataFolder}/{hash}/thumbnail.jpg");
            } catch (Exception ex)
            {
                Debug.LogException(ex);
                Debug.LogWarning($"Error while downloading metadata for {url}, skipping this video...");
                return;
            }

            if (cancellation.IsCancellationRequested) return;

            try
            {
                await FetchSubtitlesAsync(url, hash);
            } catch (Exception ex)
            {
                Debug.LogException(ex);
                Debug.LogWarning($"This video won't have subtitles because of the above error.");
            }

            /*
             * TODO: uncomment when I implement video comments!

                try
                {
                    await FetchComments(url, $"{EditorAssets.VideoMetadataFolder}/{hash}/Comments");
                } catch (Exception ex)
                {
                    Debug.LogException(ex);
                    Debug.LogWarning($"This video won't have comments because of the above error.");
                }
            */
        }

        private async static Task FetchMetadataInParallel(List<string> urls)
        {
            List<Task> tasks = new List<Task>();

            for (int i = 0; i < urls.Count; i++)
            {
                if (cancellation.IsCancellationRequested) break;

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
                if (cancellation.IsCancellationRequested) break;

                batch.Add(urls.Dequeue());

                if (batch.Count >= batchSize)
                {
                    await FetchMetadataInParallel(batch);
                    batch.Clear();
                }
            }

            if (cancellation.IsCancellationRequested) return;

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

        private static CancellationTokenSource cancellation;

        private static void CreateFolders(string[] urls)
        {
            List<Tuple<string, string>> folders = new List<Tuple<string, string>>();

            foreach (string url in urls)
            {
                string hash = Hash.String(url);
                folders.Add(new Tuple<string, string>($"{EditorAssets.VideoMetadataFolder}/{hash}", "Subtitles"));

                /*
                 * TODO: uncomment when I implement video comments!

                    folders.Add(new Tuple<string, string>($"{EditorAssets.VideoMetadataFolder}/{hash}", "Comments"));
                */
            }

            Assets.CreateFolders(folders);
        }

        private static Queue<string> PreprocessAssets(string[] urls)
        {
            cancellation = new CancellationTokenSource();
            Queue<string> queue = new Queue<string>();

            for (int i = 0; i < urls.Length; i++)
            {
                string url = urls[i];
                if (string.IsNullOrEmpty(url)) continue;
                if (!ValidateUrl(url)) continue;
                if (IsUrlCached(url)) continue;

                queue.Enqueue(url);
            }

            AssetDatabase.DisallowAutoRefresh();
            CreateFolders(urls);
            isLocked = false;

            return queue;
        }

        private static void PostprocessAssets()
        {
            CompileAllSubtitles();
            AssetDatabase.Refresh();
            ReapplyImportSettings();
            AssetDatabase.AllowAutoRefresh();
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
        public static void CancelRefresh()
        {
            if (!isLocked) return;

            cancellation.Cancel();
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
