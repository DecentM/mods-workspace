using System;
using System.Collections;

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
        private static string GetArguments(string arguments)
        {
            return string.Join(" ", new string[]
            {
                $"{arguments}",
                $"--no-check-certificate",
                $"--skip-download",
                $"--no-exec",
                $"--no-overwrites",
                $"--restrict-filenames",
                $"--geo-bypass",
                $"--ignore-config",
                $"--ignore-errors"
            });
        }

        public static IEnumerator GetVideoUrlEnumerator(string url, int resolution, Action<string> OnSuccess)
        {
            void OnFinish(ProcessResult result)
            {
                if (!string.IsNullOrEmpty(result.stderr))
                {
                    Debug.LogWarning(result.stderr);
                    Debug.LogWarning($"yt-dlp has outputted an error, ignoring results for {url}");
                    return;
                }

                OnSuccess(result.stdout);
            }

            return Parallelism.WaitForCallback((callback) => ProcessManager.RunProcess(
                EditorAssets.YtDlpPath,
                GetArguments($"-f \"mp4[height<=?{resolution}]/best[height<=?{resolution}]\" --get-url {url}"),
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
                    Debug.LogWarning(result.stderr);
                    Debug.LogWarning($"yt-dlp has outputted an error, ignoring results for {url}");
                    return;
                }

                OnSuccess(JsonUtility.FromJson<YTDLVideoJson>(result.stdout));
            }

            return Parallelism.WaitForCallback((callback) => ProcessManager.RunProcess(
                EditorAssets.YtDlpPath,
                GetArguments($"-J {url}"),
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
                    Debug.LogWarning(result.stderr);
                    Debug.LogWarning($"yt-dlp has outputted an error, ignoring results for {url}");
                    return;
                }

                OnSuccess(JsonUtility.FromJson<YTDLFlatPlaylistJson>(result.stdout));
            }

            return Parallelism.WaitForCallback((callback) => ProcessManager.RunProcess(
                EditorAssets.YtDlpPath,
                GetArguments($"-J --flat-playlist {url}"),
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
                    Debug.LogWarning(result.stderr);
                    Debug.LogWarning($"yt-dlp has outputted an error, ignoring results for {url}");
                    return;
                }
            }

            string arguments = autoSubs
                ? $"--write-subs --write-auto-subs --convert-subtitles srt --sub-langs all {url}"
                : $"--write-subs --no-write-auto-subs --convert-subtitles srt --sub-langs all {url}";

            return Parallelism.WaitForCallback((callback) => ProcessManager.RunProcess(
                EditorAssets.YtDlpPath,
                GetArguments(arguments),
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
                    Debug.LogWarning(result.stderr);
                    Debug.LogWarning($"yt-dlp has outputted an error, ignoring results for {url}");
                    return;
                }

                OnSuccess(JsonUtility.FromJson<YTDLVideoJson>(result.stdout));
            }

            return Parallelism.WaitForCallback((callback) => ProcessManager.RunProcess(
                EditorAssets.YtDlpPath,
                GetArguments($"--write-comments -J {url}"),
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
                    Debug.LogWarning(result.stderr);
                    Debug.LogWarning($"yt-dlp has outputted an error, ignoring results for {url}");
                    return;
                }
            }

            return Parallelism.WaitForCallback((callback) => ProcessManager.RunProcess(
                EditorAssets.YtDlpPath,
                GetArguments($"--write-comments {url}"),
                path,
                (result) => { callback(); OnFinish(result); }
            ));
        }

        public static IEnumerator DownloadThumbnail(string url, string path)
        {
            void OnFinish(ProcessResult result)
            {
                if (!string.IsNullOrEmpty(result.stderr))
                {
                    Debug.LogWarning(result.stderr);
                    Debug.LogWarning($"yt-dlp has outputted an error, ignoring results for {url}");
                    return;
                }
            }

            return Parallelism.WaitForCallback((callback) => ProcessManager.RunProcess(
                EditorAssets.YtDlpPath,
                GetArguments($"--convert-thumbnails jpg --write-thumbnail {url}"),
                path,
                (result) => { callback(); OnFinish(result); }
            ));
        }
    }
}
