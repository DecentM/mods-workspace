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
        public TextAsset contents;
    }

    public class SubtitleStore
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
            CompileAllSubtitles();
            AssetDatabase.Refresh();
        }

        private static IEnumerator Fetch(string url)
        {
            if (string.IsNullOrEmpty(url)) return null;
            if (!ValidateUrl(url)) return null;

            string path = GetPathFromUrl(url);

            return YTDLCommands.DownloadSubtitles(url, path, false);
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

        [PublicAPI]
        public static TextAsset[] GetFromCache(string url)
        {
            if (!ValidateUrl(url)) return null;

            List<TextAsset> result = new List<TextAsset>();

            try
            {
                string path = GetPathFromUrl(url);

                CreateFolders(new string[] { url });

                List<string> files = Directory.GetFiles(path, "*.txt", SearchOption.TopDirectoryOnly)
                    .ToList();

                if (files.Count == 0) return null;

                foreach (string file in files)
                {
                    TextAsset asset = AssetDatabase.LoadAssetAtPath<TextAsset>(file);

                    if (asset == null) continue;

                    result.Add(asset);
                }
            } catch (Exception ex)
            {
                Debug.LogException(ex);
            }

            return result.ToArray();
        }

        [PublicAPI]
        public static bool IsCached(string url)
        {
            if (!ValidateUrl(url)) return false;

            TextAsset[] assets = GetFromCache(url);

            return assets != null && assets.Length != 0;
        }
    }
}
