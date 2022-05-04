using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;
using VRC.SDK3.Components.Video;
using VRC.SDK3.Video.Components;
using VRC.SDKBase;
using DecentM.EditorTools;
using System.Diagnostics;

namespace DecentM.VideoPlayer
{
    public static class EditorURLResolverShim
    {

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        static void SetupURLResolveCallback()
        {
            if (!File.Exists(EditorAssets.YtDlpPath))
            {
                UnityEngine.Debug.LogWarning("[DecentM.VideoPlayer YTDL] Unable to find yt-dlp, URLs will not be resolved. Did you move the root folder after importing it?");
                UnityEngine.Debug.LogWarning($"[DecentM.VideoPlayer YTDL] File missing from {EditorAssets.YtDlpPath}");
                return;
            }

            VRCUnityVideoPlayer.StartResolveURLCoroutine += ResolveURLCallback;
        }

        static void ResolveURLCallback(VRCUrl url, int resolution, UnityEngine.Object videoPlayer, Action<string> urlResolvedCallback, Action<VideoError> errorCallback)
        {
            UnityEngine.Debug.Log($"[DecentM.VideoPlayer YTDL] Attempting to resolve URL '{url}'");

            EditorCoroutine.Start(YTDLCommands.GetVideoUrlEnumerator(url.ToString(), resolution, (string result) =>
            {
                UnityEngine.Debug.Log($"[DecentM.VideoPlayer YTDL] Resolved '{url}' to '{result}'");
                urlResolvedCallback(result);
            }));
        }
    }
}
