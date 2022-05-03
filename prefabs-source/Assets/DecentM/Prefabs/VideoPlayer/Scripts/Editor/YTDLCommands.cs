using System;
using System.Linq;
using System.Web;
using System.Net;
using System.IO;
using System.Text;
using System.Diagnostics;
using System.Collections;

using UnityEngine;
using UnityEngine.Networking;
using UnityEditor;

using DecentM.EditorTools;

namespace DecentM.VideoPlayer
{
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
                10000,
                (Process process) => callback(process.StandardOutput.ReadToEnd())
            );
        }

        public static EditorCoroutine GetVideoUrl(string url, int resolution, Action<string> callback)
        {
            return ProcessManager.RunProcessCoroutine(
                EditorAssets.YtDlpPath,
                $"--no-check-certificate -f \"mp4[height<=?{resolution}]/best[height<=?{resolution}]\" --get-url {url}",
                10000,
                (Process process) => callback(process.StandardOutput.ReadToEnd())
            );
        }

        public static EditorCoroutine GetVideoMetadata(string url, Action<YTDLVideoJson?> callback)
        {
            return ProcessManager.RunProcessCoroutine(
                EditorAssets.YtDlpPath,
                $"--no-check-certificate -J {url}",
                10000,
                (Process process) => callback(JsonUtility.FromJson<YTDLVideoJson>(process.StandardOutput.ReadToEnd()))
            );
        }

        public static EditorCoroutine GetPlaylistVideos(string url, Action<YTDLFlatPlaylistJson?> callback)
        {
            return ProcessManager.RunProcessCoroutine(
                EditorAssets.YtDlpPath,
                $"--no-check-certificate -J --flat-playlist {url}",
                10000,
                (Process process) => callback(JsonUtility.FromJson<YTDLFlatPlaylistJson>(process.StandardOutput.ReadToEnd()))
            );
        }
    }
}
