using System;
using UdonSharp;
using UnityEngine;
using VRC.SDK3.Components.Video;
using VRC.SDKBase;
using VRC.Udon;
using TMPro;

namespace DecentM.VideoPlayer.Plugins
{
    public sealed class SkipToTimestampPlugin : VideoPlayerPlugin
    {
        private float GetTimestamp(string url)
        {
            string[] queries = url.Split('&', '?');

            foreach (string query in queries)
            {
                if (!query.StartsWith("t=")) continue;

                string[] parts = query.Split('=', 's');
                float result;
                bool parsed = float.TryParse(parts[1], out result);

                if (parsed) return result;
            }

            return -1;
        }

        protected override void OnLoadReady(float duration)
        {
            VRCUrl url = this.system.GetCurrentUrl();

            if (url == null) return;

            float timestamp = this.GetTimestamp(url.ToString());
            if (timestamp == -1) return;

            this.system.Seek(timestamp);
        }
    }
}
