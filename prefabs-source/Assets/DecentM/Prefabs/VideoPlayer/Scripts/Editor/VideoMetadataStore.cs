using System;
using System.Linq;
using System.Net;
using System.IO;
using System.Text;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;

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
        private static string GetHash(string text)
        {
            if (String.IsNullOrEmpty(text))
                return String.Empty;

            using (var sha = new System.Security.Cryptography.SHA256Managed())
            {
                byte[] textData = Encoding.UTF8.GetBytes(text);
                byte[] hash = sha.ComputeHash(textData);
                return BitConverter.ToString(hash).Replace("-", String.Empty);
            }
        }

        private static bool ValidateUrl(string url)
        {
            return Uri.TryCreate(url, UriKind.Absolute, out Uri result);
        }

        private static bool CreateFolder(string basePath, string name)
        {
            if (!basePath.StartsWith("Assets/")) return false;

            if (AssetDatabase.IsValidFolder($"{basePath}/{name}")) return true;
            if (!AssetDatabase.IsValidFolder(basePath))
            {
                string[] paths = basePath.Split('/');
                CreateFolder(string.Join("/", paths.Take(paths.Length - 1)), paths[paths.Length - 1]);
            }

            if (AssetDatabase.CreateFolder(basePath, name) == "") return false;
            return true;
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

        private static Texture2D GetThumbnail(string hash)
        {
            string filename = "thumbnail.jpg";
            string path = $"{EditorAssets.VideoMetadataFolder}/{hash}/{filename}";
            Texture2D result = AssetDatabase.LoadAssetAtPath<Texture2D>(path);

            return result;
        }

        public static void RefreshMetadata(string url, Action OnFinish)
        {
            FetchMetadata(new string[] { url }, OnFinish);
        }

        public static void RefreshMetadata(string[] urls, Action OnFinish)
        {
            if (urls.Length == 0) return;
            FetchMetadata(urls, OnFinish);
        }

        private static YTDLVideoJson? GetYTDLJson(string url)
        {
            if (!ValidateUrl(url)) return null;

            string filename = "metadata.json";
            string hash = GetHash(url);
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

        public static VideoMetadata GetMetadata(string url)
        {
            VideoMetadata metadata = new VideoMetadata();

            if (!ValidateUrl(url)) return metadata;

            string hash = GetHash(url);

            try
            {
                YTDLVideoJson? jsonOrNull = GetYTDLJson(url);
                if (jsonOrNull == null) return metadata;
                YTDLVideoJson json = (YTDLVideoJson)jsonOrNull;

                metadata.fps = json.fps;
                metadata.duration = json.duration_string;
                metadata.title = json.title;
                metadata.resolution = json.resolution;
                metadata.uploader = json.uploader;
                metadata.viewCount = 0;
                metadata.likeCount = 0;
                metadata.thumbnail = GetThumbnail(hash);
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

        private async static Task FetchComments(string url, string path)
        {
            await YTDLCommands.DownloadMetadataWithComments(url, path);
        }

        private static void CompileSubtitles(string inputPath, string outputPath)
        {
            if (!File.Exists(inputPath)) return;

            try
            {
                string sourceContents = File.ReadAllText(inputPath);
                string extension = Path.GetExtension(inputPath);
                SubtitleCompiler.CompilationResult result = SubtitleCompiler.Compile(sourceContents, extension);

                File.WriteAllBytes(outputPath, Encoding.UTF8.GetBytes(result.output));
                File.Delete(inputPath);
                File.Delete($"{inputPath}.meta");
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

        private static Task FetchSubtitles(string url, string hash)
        {
            string path = $"{EditorAssets.VideoMetadataFolder}/{hash}";

            return YTDLCommands.DownloadSubtitles(url, $"{path}/Subtitles", false);
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

        private static void FetchMetadataCallback(string hash, YTDLVideoJson? jsonOrNull)
        {
            if (jsonOrNull == null) return;

            YTDLVideoJson json = (YTDLVideoJson)jsonOrNull;

            string path = $"{EditorAssets.VideoMetadataFolder}/{hash}/metadata.json";
            byte[] bytes = Encoding.UTF8.GetBytes(JsonUtility.ToJson(json));
            File.WriteAllBytes(path, bytes);
        }

        private async static Task FetchMetadata(string url)
        {
            if (string.IsNullOrEmpty(url)) return;
            if (!ValidateUrl(url)) return;

            string hash = GetHash(url);

            try
            {
                YTDLVideoJson json = await YTDLCommands.GetMetadata(url);
                FetchMetadataCallback(hash, json);
                await FetchThumbnail(json, $"{EditorAssets.VideoMetadataFolder}/{hash}/thumbnail.jpg");
            } catch (Exception ex)
            {
                Debug.LogException(ex);
                Debug.LogWarning($"Error while downloading metadata for {url}, skipping this video...");
                return;
            }

            try
            {
                await FetchSubtitles(url, hash);
            } catch (Exception ex)
            {
                Debug.LogException(ex);
                Debug.LogWarning($"This video won't have subtitles because of the above error.");
            }

            try
            {
                await FetchComments(url, $"{EditorAssets.VideoMetadataFolder}/{hash}/Comments");
            } catch (Exception ex)
            {
                Debug.LogException(ex);
                Debug.LogWarning($"This video won't have comments because of the above error.");
            }
        }

        private static IEnumerator WaitForTask(Task task, Action OnSettled)
        {
            bool isSettled = false;

            while (!isSettled)
            {
                if (task.IsCompleted || task.IsCanceled || task.IsFaulted)
                {
                    if (task.IsFaulted)
                    {
                        Debug.LogError("The task has entered the faulted state, it will not continue.");
                    }

                    isSettled = true;
                }

                yield return new WaitForSeconds(0.25f);
            }

            if (isSettled)
            {
                OnSettled();
                yield return null;
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

                tasks.Add(FetchMetadata(url));
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

        private static void FetchMetadata(string[] urls, Action OnFinish)
        {
            Queue<string> queue = new Queue<string>();

            for (int i = 0; i < urls.Length; i++)
            {
                string url = urls[i];
                if (string.IsNullOrEmpty(url)) continue;
                if (!ValidateUrl(url)) continue;

                EditorUtility.DisplayProgressBar("Creating folders...", url, 1f * i / urls.Length);

                string hash = GetHash(url);
                CreateFolder($"{EditorAssets.VideoMetadataFolder}/{hash}", "Subtitles");
                CreateFolder($"{EditorAssets.VideoMetadataFolder}/{hash}", "Comments");
                queue.Enqueue(url);
            }

            EditorUtility.ClearProgressBar();

            Task allFetches = Task.Run(() => FetchMetadataAsync(queue, 4));

            EditorCoroutine.Start(
                WaitForTask(allFetches, () => {
                    CompileAllSubtitles();
                    ReapplyImportSettings();
                    AssetDatabase.Refresh();
                    OnFinish();
                })
            );
        }
    }
}
