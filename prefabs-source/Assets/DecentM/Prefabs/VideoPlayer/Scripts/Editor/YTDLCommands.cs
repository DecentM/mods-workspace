using System;
using System.Diagnostics;
using System.Collections;
using System.Threading.Tasks;

using UnityEngine;

using DecentM.EditorTools;

namespace DecentM.VideoPlayer
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
        public static IEnumerator GetVideoUrlEnumerator(string url, int resolution, Action<string> callback)
        {
            return ProcessManager.CreateProcessCoroutine(
                EditorAssets.YtDlpPath,
                $"--no-check-certificate -f \"mp4[height<=?{resolution}]/best[height<=?{resolution}]\" --get-url {url}",
                ".",
                BlockingBehaviour.NonBlocking,
                (Process process) => callback(process.StandardOutput.ReadToEnd())
            );
        }

        public static void GetVideoUrl(string url, int resolution, Action<string> callback)
        {
            ProcessManager.RunProcessAsync(
                EditorAssets.YtDlpPath,
                $"--no-check-certificate -f \"mp4[height<=?{resolution}]/best[height<=?{resolution}]\" --get-url {url}",
                ".",
                (ProcessResult result) => callback(result.stdout)
            );
        }

        public async static Task<YTDLVideoJson> GetMetadata(string url)
        {
            ProcessResult result = await ProcessManager.RunProcessAsync(
                EditorAssets.YtDlpPath,
                $"--no-check-certificate -J {url}",
                "."
            );

            if (!string.IsNullOrEmpty(result.stderr))
            {
                throw new Exception(result.stderr);
            }

            return JsonUtility.FromJson<YTDLVideoJson>(result.stdout);
        }

        public async static Task<YTDLFlatPlaylistJson> GetPlaylistVideos(string url)
        {
            ProcessResult result = await ProcessManager.RunProcessAsync(
                EditorAssets.YtDlpPath,
                $"--no-check-certificate -J --flat-playlist {url}",
                "."
            );

            if (!string.IsNullOrEmpty(result.stderr))
            {
                throw new Exception(result.stderr);
            }

            return JsonUtility.FromJson<YTDLFlatPlaylistJson>(result.stdout);
        }

        public static YTDLFlatPlaylistJson GetPlaylistVideosSync(string url)
        {
            ProcessResult result = ProcessManager.RunProcessSync(
                EditorAssets.YtDlpPath,
                $"--no-check-certificate -J --flat-playlist {url}",
                ".",
                10000
            );

            return JsonUtility.FromJson<YTDLFlatPlaylistJson>(result.stdout);
        }

        public async static Task DownloadSubtitles(string url, string path, bool autoSubs)
        {
            string arguments = autoSubs
                ? $"--no-check-certificate --skip-download --write-subs --write-auto-subs --sub-format vtt --sub-langs all {url}"
                : $"--no-check-certificate --skip-download --write-subs --no-write-auto-subs --sub-format vtt --sub-langs all {url}";

            ProcessResult result = await ProcessManager.RunProcessAsync(
                EditorAssets.YtDlpPath,
                arguments,
                path
            );

            if (!string.IsNullOrEmpty(result.stderr))
            {
                throw new Exception(result.stderr);
            }
        }

        public async static Task<YTDLVideoJson> GetMetadataWithComments(string url)
        {
            ProcessResult result = await ProcessManager.RunProcessAsync(
                EditorAssets.YtDlpPath,
                $"--no-check-certificate --skip-download --write-comments -J {url}",
                "."
            );

            if (!string.IsNullOrEmpty(result.stderr))
            {
                throw new Exception(result.stderr);
            }

            return JsonUtility.FromJson<YTDLVideoJson>(result.stdout);
        }

        public async static Task DownloadMetadataWithComments(string url, string path)
        {
            ProcessResult result = await ProcessManager.RunProcessAsync(
                EditorAssets.YtDlpPath,
                $"--no-check-certificate --skip-download --write-comments {url}",
                path
            );

            if (!string.IsNullOrEmpty(result.stderr))
            {
                throw new Exception(result.stderr);
            }
        }
    }
}
