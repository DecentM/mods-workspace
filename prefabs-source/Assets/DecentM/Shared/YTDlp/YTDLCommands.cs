using System;
using System.Collections;

using UnityEngine;

namespace DecentM.Shared.YTdlp
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
            return string.Join(
                " ",
                new string[]
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
                }
            );
        }

        private static bool IsError(ProcessResult result)
        {
            if (string.IsNullOrEmpty(result.stderr))
                return false;
            if (result.stderr.Contains("Retrying"))
                return false;
            if (result.stderr.Contains("URL could be a direct video link"))
                return false;

            Debug.LogWarning(result.stderr);
            Debug.LogWarning($"yt-dlp has outputted an error, results may be incomplete.");

            return true;
        }

        public static IEnumerator GetVideoUrlEnumerator(
            string url,
            int resolution,
            Action<string> OnSuccess
        )
        {
            void OnFinish(ProcessResult result)
            {
                if (IsError(result))
                    return;

                OnSuccess(result.stdout);
            }

            return Parallelism.WaitForCallback(
                (callback) =>
                    ProcessManager.RunProcess(
                        AssetPaths.YtDlpPath,
                        GetArguments(
                            $"-f \"mp4[height<=?{resolution}]/best[height<=?{resolution}]\" --get-url {url}"
                        ),
                        ".",
                        (result) =>
                        {
                            callback();
                            OnFinish(result);
                        }
                    )
            );
        }

        public static void GetVideoUrl(string url, int resolution, Action<string> callback)
        {
            ProcessManager.RunProcess(
                AssetPaths.YtDlpPath,
                $"--no-check-certificate -f \"mp4[height<=?{resolution}]/best[height<=?{resolution}]\" --get-url {url}",
                ".",
                (ProcessResult result) => callback(result.stdout)
            );
        }

        public static IEnumerator GetMetadata(string url, Action<YTDLVideoJson> OnSuccess)
        {
            void OnFinish(ProcessResult result)
            {
                if (IsError(result))
                    return;

                OnSuccess(JsonUtility.FromJson<YTDLVideoJson>(result.stdout));
            }

            return Parallelism.WaitForCallback(
                (callback) =>
                    ProcessManager.RunProcess(
                        AssetPaths.YtDlpPath,
                        GetArguments($"-J {url}"),
                        ".",
                        (result) =>
                        {
                            callback();
                            OnFinish(result);
                        }
                    )
            );
        }

        public static IEnumerator GetPlaylistVideos(
            string url,
            Action<YTDLFlatPlaylistJson> OnSuccess
        )
        {
            void OnFinish(ProcessResult result)
            {
                if (IsError(result))
                    return;

                OnSuccess(JsonUtility.FromJson<YTDLFlatPlaylistJson>(result.stdout));
            }

            return Parallelism.WaitForCallback(
                (callback) =>
                    ProcessManager.RunProcess(
                        AssetPaths.YtDlpPath,
                        GetArguments($"-J --flat-playlist {url}"),
                        ".",
                        (result) =>
                        {
                            callback();
                            OnFinish(result);
                        }
                    )
            );
        }

        public static IEnumerator DownloadSubtitles(string url, string path, bool autoSubs = false)
        {
            void OnFinish(ProcessResult result)
            {
                if (IsError(result))
                    return;
            }

            string arguments = autoSubs
                ? $"--write-subs --write-auto-subs --sub-format srt/vtt --sub-langs all {url}"
                : $"--write-subs --no-write-auto-subs --sub-format srt/vtt --sub-langs all {url}";

            return Parallelism.WaitForCallback(
                (callback) =>
                    ProcessManager.RunProcess(
                        AssetPaths.YtDlpPath,
                        GetArguments(arguments),
                        path,
                        (result) =>
                        {
                            callback();
                            OnFinish(result);
                        }
                    )
            );
        }

        public static IEnumerator GetMetadataWithComments(
            string url,
            Action<YTDLVideoJson> OnSuccess
        )
        {
            void OnFinish(ProcessResult result)
            {
                if (IsError(result))
                    return;

                OnSuccess(JsonUtility.FromJson<YTDLVideoJson>(result.stdout));
            }

            return Parallelism.WaitForCallback(
                (callback) =>
                    ProcessManager.RunProcess(
                        AssetPaths.YtDlpPath,
                        GetArguments($"--write-comments -J {url}"),
                        ".",
                        (result) =>
                        {
                            callback();
                            OnFinish(result);
                        }
                    )
            );
        }

        public static IEnumerator DownloadMetadataWithComments(string url, string path)
        {
            void OnFinish(ProcessResult result)
            {
                if (IsError(result))
                    return;
            }

            return Parallelism.WaitForCallback(
                (callback) =>
                    ProcessManager.RunProcess(
                        AssetPaths.YtDlpPath,
                        GetArguments($"--write-comments {url}"),
                        path,
                        (result) =>
                        {
                            callback();
                            OnFinish(result);
                        }
                    )
            );
        }

        public static IEnumerator DownloadThumbnail(string url, string path)
        {
            void OnFinish(ProcessResult result)
            {
                if (IsError(result))
                    return;
            }

            return Parallelism.WaitForCallback(
                (callback) =>
                    ProcessManager.RunProcess(
                        AssetPaths.YtDlpPath,
                        GetArguments($"--convert-thumbnails jpg --write-thumbnail {url}"),
                        path,
                        (result) =>
                        {
                            callback();
                            OnFinish(result);
                        }
                    )
            );
        }
    }
}
