using System;
using System.Diagnostics;
using System.Collections;
using System.Threading.Tasks;

using UnityEngine;

namespace DecentM.EditorTools
{
    [Serializable]
    public struct YTDLVideoJsonComment
    {
        public string id;
        public string text;
        public string timestamp;
        public int like_count;
        public string author;
        public string author_id;
        public string author_thumbnail;
        public bool author_is_uploader;
        public string parent;
    }

    [Serializable]
    public struct YTDLVideoJson
    {
        public string duration_string;
        public string title;
        public string uploader;
        public string view_count;
        public string like_count;
        public string thumbnail;
        public string resolution;
        public int fps;
        public string original_url;
        public string extractor_key;
        public string description;
        public YTDLVideoJsonComment[] comments;
    }

    [Serializable]
    public struct YTDLFlatPlaylistJsonEntry
    {
        public string _type;
        public string id;
        public string url;
        public string title;
        public string duration;
        public string uploader;
    }

    [Serializable]
    public struct YTDLFlatPlaylistJson
    {
        public string _type;
        public string uploader;
        public string availability;
        public string title;
        public string description;
        public YTDLFlatPlaylistJsonEntry[] entries;
    }

    public static class YTDLCommands
    {
        public static IEnumerator GetVideoUrlEnumerator(string url, int resolution, Action<string> OnSuccess)
        {
            void OnFinish(ProcessResult result)
            {
                if (!string.IsNullOrEmpty(result.stderr))
                {
                    throw new Exception(result.stderr);
                }

                OnSuccess(result.stdout);
            }

            return Parallelism.WaitForCallback((callback) => ProcessManager.RunProcess(
                EditorAssets.YtDlpPath,
                $"--no-check-certificate -f \"mp4[height<=?{resolution}]/best[height<=?{resolution}]\" --get-url {url}",
                ".",
                (result) => { callback(); OnFinish(result); }
            ));
        }

        public static void GetVideoUrl(string url, int resolution, Action<string> callback)
        {
            ProcessManager.RunProcess(
                EditorAssets.YtDlpPath,
                $"--no-check-certificate -f \"mp4[height<=?{resolution}]/best[height<=?{resolution}]\" --get-url {url}",
                ".",
                (ProcessResult result) => callback(result.stdout)
            );
        }

        public static IEnumerator GetMetadata(string url, Action<YTDLVideoJson> OnSuccess)
        {
            void OnFinish(ProcessResult result)
            {
                if (!string.IsNullOrEmpty(result.stderr))
                {
                    throw new Exception(result.stderr);
                }

                OnSuccess(JsonUtility.FromJson<YTDLVideoJson>(result.stdout));
            }

            return Parallelism.WaitForCallback((callback) => ProcessManager.RunProcess(
                EditorAssets.YtDlpPath,
                $"--no-check-certificate -J {url}",
                ".",
                (result) => { callback(); OnFinish(result); }
            ));
        }

        public static IEnumerator GetPlaylistVideos(string url, Action<YTDLFlatPlaylistJson> OnSuccess)
        {
            void OnFinish(ProcessResult result)
            {
                if (!string.IsNullOrEmpty(result.stderr))
                {
                    throw new Exception(result.stderr);
                }

                OnSuccess(JsonUtility.FromJson<YTDLFlatPlaylistJson>(result.stdout));
            }

            return Parallelism.WaitForCallback((callback) => ProcessManager.RunProcess(
                EditorAssets.YtDlpPath,
                $"--no-check-certificate -J --flat-playlist {url}",
                ".",
                (result) => { callback(); OnFinish(result); }
            ));
        }

        public static IEnumerator DownloadSubtitles(string url, string path, bool autoSubs)
        {
            void OnFinish(ProcessResult result)
            {
                if (!string.IsNullOrEmpty(result.stderr))
                {
                    throw new Exception(result.stderr);
                }
            }

            string arguments = autoSubs
                ? $"--no-check-certificate --skip-download --write-subs --write-auto-subs --sub-format vtt/srt --sub-langs all {url}"
                : $"--no-check-certificate --skip-download --write-subs --no-write-auto-subs --sub-format vtt/srt --sub-langs all {url}";

            return Parallelism.WaitForCallback((callback) => ProcessManager.RunProcess(
                EditorAssets.YtDlpPath,
                arguments,
                path,
                (result) => { callback(); OnFinish(result); }
            ));
        }

        public static IEnumerator GetMetadataWithComments(string url, Action<YTDLVideoJson> OnSuccess)
        {
            void OnFinish(ProcessResult result)
            {
                if (!string.IsNullOrEmpty(result.stderr))
                {
                    throw new Exception(result.stderr);
                }

                OnSuccess(JsonUtility.FromJson<YTDLVideoJson>(result.stdout));
            }

            return Parallelism.WaitForCallback((callback) => ProcessManager.RunProcess(
                EditorAssets.YtDlpPath,
                $"--no-check-certificate --skip-download --write-comments -J {url}",
                ".",
                (result) => { callback(); OnFinish(result); }
            ));
        }

        public static IEnumerator DownloadMetadataWithComments(string url, string path)
        {
            void OnFinish(ProcessResult result)
            {
                if (!string.IsNullOrEmpty(result.stderr))
                {
                    throw new Exception(result.stderr);
                }
            }

            return Parallelism.WaitForCallback((callback) => ProcessManager.RunProcess(
                EditorAssets.YtDlpPath,
                $"--no-check-certificate --skip-download --write-comments {url}",
                path,
                (result) => { callback(); OnFinish(result); }
            ));
        }
    }
}
