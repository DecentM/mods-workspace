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

using DecentM.Subtitles;

namespace DecentM.EditorTools
{
    public struct CachedSubtitle
    {
        public string language;
        public string contents;
    }

    public class SubtitleStore : AutoSceneFixer
    {
        private static bool ValidateUrl(string url)
        {
            return Uri.TryCreate(url, UriKind.Absolute, out Uri result);
        }

        private static void CompileSubtitles(string inputPath, string outputPath)
        {
            if (!File.Exists(inputPath) || File.Exists(outputPath)) return;

            try
            {
                string sourceContents = File.ReadAllText(inputPath);
                string extension = Path.GetExtension(inputPath);
                SubtitleCompiler.CompilationResult result = SubtitleCompiler.Compile(sourceContents, extension);

                File.WriteAllBytes(outputPath, Encoding.UTF8.GetBytes(result.output));

                // Get rid of the source files so that they're not included in the asset database refresh
                // later on.
                if (File.Exists(inputPath)) File.Delete(inputPath);
                if (File.Exists($"{inputPath}.meta")) File.Delete($"{inputPath}.meta");
                if (File.Exists($"{inputPath}.part")) File.Delete($"{inputPath}.part");
                if (File.Exists($"{inputPath}.part.meta")) File.Delete($"{inputPath}.part.meta");
            }
            catch (Exception ex)
            {
                Debug.LogWarning(ex);
                Debug.LogWarning($"Failed to compile subtitles, skipping file: {inputPath}");
            }
        }

        private static void CompileAllSubtitles()
        {
            List<string> files = new List<string>();

            try
            {
                files = Directory.GetFiles($"{EditorAssets.SubtitleCacheFolder}", "*.*", SearchOption.AllDirectories)
                .Where(name => name.EndsWith(".srt") || name.EndsWith(".vtt"))
                .ToList();
            } catch (Exception ex)
            {
                if (!(ex is DirectoryNotFoundException))
                {
                    throw ex;
                }
            }

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

        private static string GetPathFromUrl(string url)
        {
            if (!ValidateUrl(url)) return null;

            string hash = Hash.String(url);
            return $"{EditorAssets.SubtitleCacheFolder}/{hash}";
        }

        private static void CreateFolders(string[] urls)
        {
            List<Tuple<string, string>> folders = new List<Tuple<string, string>>();

            foreach (string url in urls)
            {
                string hash = Hash.String(url);
                folders.Add(new Tuple<string, string>($"{EditorAssets.SubtitleCacheFolder}", hash));
            }

            Assets.CreateFolders(folders);
        }

        private static Queue<string> pendingUrls = new Queue<string>();

        private static bool isLocked = false;

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
            isLocked = true;

            return queue;
        }

        private static void PostprocessAssets()
        {
            CompileAllSubtitles();
            AssetDatabase.Refresh();
            isLocked = false;
        }

        protected override bool OnPerformFixes()
        {
            if (isLocked) return true;

            PreprocessAssets(pendingUrls.ToArray());

            Task allFetches = Task.Run(() => FetchAsync(pendingUrls, 4));

            EditorCoroutine.Start(
                Parallelism.WaitForTask(allFetches, (bool success) => {
                    PostprocessAssets();
                    if (!success) Debug.LogError("An error occurred during batched subtitle downloading, there is likely more information about it above.");
                })
            );

            return true;
        }

        private async static Task FetchInParallel(List<string> urls)
        {
            List<Task> tasks = new List<Task>();

            for (int i = 0; i < urls.Count; i++)
            {
                string url = urls[i];
                if (string.IsNullOrEmpty(url)) continue;
                if (!ValidateUrl(url)) continue;

                tasks.Add(FetchAsync(url));
            }

            await Task.WhenAll(tasks);
        }

        private async static Task FetchAsync(Queue<string> urls, int batchSize)
        {
            List<string> batch = new List<string>();

            while (urls.Count > 0)
            {
                batch.Add(urls.Dequeue());

                if (batch.Count >= batchSize)
                {
                    await FetchInParallel(batch);
                    batch.Clear();
                }
            }

            // Process the last batch
            if (batch.Count > 0)
            {
                await FetchInParallel(batch);
                batch.Clear();
            }
        }

        [PublicAPI]
        public static CachedSubtitle[] GetFromCache(string url)
        {
            if (!ValidateUrl(url)) return null;

            try
            {
                string path = GetPathFromUrl(url);

                CreateFolders(new string[] { url });

                List<string> files = Directory.GetFiles(path, "*.*", SearchOption.TopDirectoryOnly)
                    .Where(name => name.EndsWith(".txt"))
                    .ToList();

                if (files.Count == 0 && !pendingUrls.Contains(url))
                {
                    pendingUrls.Enqueue(url);
                    return null;
                }

                // UnityEngine.Object[] assets = AssetDatabase.LoadAllAssetsAtPath(path);
                List<CachedSubtitle> result = new List<CachedSubtitle>();

                foreach (string file in files)
                {
                    TextAsset asset = AssetDatabase.LoadAssetAtPath<TextAsset>(file);

                    if (asset == null && !pendingUrls.Contains(url))
                    {
                        pendingUrls.Enqueue(url);
                        return result.ToArray();
                    }

                    CachedSubtitle cachedSubtitle = new CachedSubtitle();
                    cachedSubtitle.contents = asset.text;
                    cachedSubtitle.language = Path.GetFileNameWithoutExtension(asset.name);
                    result.Add(cachedSubtitle);
                }

                return result.ToArray();
            } catch (Exception ex)
            {
                Debug.LogException(ex);
                throw ex;
            }
        }

        [PublicAPI]
        public static void FetchSync(string url)
        {
            if (string.IsNullOrEmpty(url) || IsCached(url)) return;

            string path = GetPathFromUrl(url);
            YTDLCommands.DownloadSubtitlesSync(url, path, false);
        }

        [PublicAPI]
        public async static Task FetchAsync(string url)
        {
            string path = GetPathFromUrl(url);
            if (string.IsNullOrEmpty(url)) return;
            if (!ValidateUrl(url)) return;

            string hash = Hash.String(url);

            try
            {
                await YTDLCommands.DownloadSubtitles(url, path, false);
            }
            catch (Exception ex)
            {
                Debug.LogException(ex);
                Debug.LogWarning($"Error while downloading metadata for {url}, skipping this video...");
                return;
            }
        }

        [PublicAPI]
        public static bool IsCached(string url)
        {
            if (!ValidateUrl(url)) return false;

            CachedSubtitle[] assets = GetFromCache(url);

            return assets.Length != 0;
        }
    }
}
