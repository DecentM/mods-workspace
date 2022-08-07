using System;
using System.Collections;

using UnityEngine;

namespace DecentM.Shared.YTdlp
{
    #region Structs

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

    #endregion

    public static class YTdlpCommands
    {
        #region Utilities

        private static string GetArguments(string arguments)
        {
            return string.Join(
                " ",
                new string[]
                {
                    $"--no-check-certificate",
                    $"--skip-download",
                    $"--no-exec",
                    $"--no-overwrites",
                    $"--restrict-filenames",
                    $"--geo-bypass",
                    $"--ignore-config",
                    $"--ignore-errors",
                    $"{arguments}"
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

#if UNITY_EDITOR
        private static string YtDlpPath = $"{Application.dataPath}/DecentM/Shared/YTdlp/yt-dlp.exe";
#else
        private static string YtDlpPath = $"{Application.dataPath}/UserLibs/yt-dlp.exe";
#endif

        private static void YTdlp(
            string arguments,
            Action<string> callback
        )
        {
            ProcessManager.RunProcess(
                YtDlpPath,
                GetArguments(arguments),
                ".",
                (ProcessResult result) => {
                    // TODO: Maybe not discard errors, but tell the user about them
                    if (IsError(result))
                        return;

                    callback(result.stdout);
                }
            );
        }

#endregion

#region Methods

        public static void GetVideoUrl(string url, Vector2Int resolution, Action<string> callback)
        {
            YTdlp($"--no-check-certificate -f \"mp4[height<=?{resolution.y}]/best[height<=?{resolution.x}]\" --get-url {url}", callback);
        }

        public static void GetMetadata(string url, Action<YTDLVideoJson> OnSuccess)
        {
            YTdlp($"-J {url}", (stdout) => OnSuccess(JsonUtility.FromJson<YTDLVideoJson>(stdout)));
        }

        public static void GetPlaylistVideos(
            string url,
            Action<YTDLFlatPlaylistJson> OnSuccess
        )
        {
            YTdlp($"-J --flat-playlist {url}", (stdout) => OnSuccess(JsonUtility.FromJson<YTDLFlatPlaylistJson>(stdout)));
        }

        public static void DownloadSubtitles(string url, string path, bool autoSubs, Action<string> OnFinish)
        {
            string arguments = autoSubs
                ? $"--write-subs --write-auto-subs --sub-format srt/vtt --sub-langs all {url}"
                : $"--write-subs --no-write-auto-subs --sub-format srt/vtt --sub-langs all {url}";

            YTdlp(arguments, OnFinish);
        }

        public static void GetMetadataWithComments(
            string url,
            Action<YTDLVideoJson> OnSuccess
        )
        {
            YTdlp($"--write-comments -J {url}", (stdout) => OnSuccess(JsonUtility.FromJson<YTDLVideoJson>(stdout)));
        }

        public static void DownloadMetadataWithComments(string url, string path, Action<string> OnFinish)
        {
            YTdlp($"--write-comments -J {url}", OnFinish);
        }

        public static void DownloadThumbnail(string url, string path, Action<string> OnFinish)
        {
            YTdlp($"--convert-thumbnails jpg --write-thumbnail {url}", OnFinish);
        }

#endregion
    }
}
