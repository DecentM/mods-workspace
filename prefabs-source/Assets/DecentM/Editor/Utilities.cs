using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using System.Linq;
using System.Reflection;
using System.Text;

using UnityEditor;
using UnityEngine;

namespace DecentM.EditorTools
{
    public static class AsyncProgress
    {
        private static readonly MethodInfo _displayProgressBar = typeof(Editor).Assembly.GetTypes().FirstOrDefault(e => e.Name == "AsyncProgressBar")?.GetMethod("Display");
        private static readonly MethodInfo _clearProgressBar = typeof(Editor).Assembly.GetTypes().FirstOrDefault(e => e.Name == "AsyncProgressBar")?.GetMethod("Clear");

        public static void Display(string text, float progress)
        {
            _displayProgressBar.Invoke(null, new object[] { text, progress });
        }

        public static void Clear()
        {
            _clearProgressBar.Invoke(null, null);
        }
    }

    public static class Parallelism
    {
        public static IEnumerator WaitForTask(Task task, Action<bool> OnSettled)
        {
            bool isSettled = false;
            bool isFaulted = false;

            while (!isSettled)
            {
                if (task.IsCompleted || task.IsCanceled || task.IsFaulted)
                {
                    isFaulted = task.IsFaulted;
                    isSettled = true;
                }

                yield return new WaitForSeconds(0.25f);
            }

            if (isSettled)
            {
                OnSettled(!isFaulted);
                yield return null;
            }
        }

        public static async Task WhenAllBatched(IEnumerable<Task> tasks, int maxConcurrency)
        {
            SemaphoreSlim semaphore = new SemaphoreSlim(maxConcurrency);

            var pendingTasks = tasks.Select(async task =>
            {
                try
                {
                    semaphore.Wait();
                    await task;
                }
                finally
                {
                    semaphore.Release();
                }
            });

            await Task.WhenAll(pendingTasks);
        }
    }

    public static class Hash
    {
        public static string String(string text)
        {
            if (string.IsNullOrEmpty(text))
                return string.Empty;

            using (var sha = new System.Security.Cryptography.SHA256Managed())
            {
                byte[] textData = Encoding.UTF8.GetBytes(text);
                byte[] hash = sha.ComputeHash(textData);
                return BitConverter.ToString(hash).Replace("-", string.Empty);
            }
        }
    }

    public static class Assets
    {
        public static bool CreateFolder(string basePath, string name)
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

        public static bool CreateFolders(List<Tuple<string, string>> input)
        {
            bool success = true;

            for (int i = 0; i < input.Count; i++)
            {
                Tuple<string, string> tuple = input[i];
                EditorUtility.DisplayProgressBar("Creating folders...", tuple.Item2, 1f * i / input.Count);

                if (!CreateFolder(tuple.Item1, tuple.Item2))
                {
                    success = false;
                }
            }

            EditorUtility.ClearProgressBar();

            return success;
        }
    }
}
