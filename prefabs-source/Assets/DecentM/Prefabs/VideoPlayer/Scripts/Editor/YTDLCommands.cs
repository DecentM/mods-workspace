using System;
using System.Linq;
using System.Web;
using System.Net;
using System.IO;
using System.Text;
using System.Diagnostics;
using System.Collections.Generic;

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
        private static bool ValidateUrl(string url)
        {
            return Uri.TryCreate(url, UriKind.Absolute, out Uri result);
        }

        public static YTDLVideoJson? GetVideoMetadata(string url)
        {
            try
            {
                ProcessResult ytdl = ProcessManager.RunProcess(EditorAssets.YtDlpPath, $"--no-check-certificate -J {url}", 10000);
                return JsonUtility.FromJson<YTDLVideoJson>(ytdl.stdout);
            } catch
            {
                return null;
            }
        }

        public static YTDLFlatPlaylistJson? GetPlaylistVideos(string url)
        {
            try
            {
                ProcessResult ytdl = ProcessManager.RunProcess(EditorAssets.YtDlpPath, $"--no-check-certificate -J --flat-playlist {url}", 10000);
                return JsonUtility.FromJson<YTDLFlatPlaylistJson>(ytdl.stdout);
            } catch
            {
                return null;
            }
        }
    }
}
