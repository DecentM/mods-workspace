using System;
using UnityEngine;
using TMPro;

namespace DecentM.Prefabs.VideoPlayer.Plugins
{
    public sealed class SkipToTimestampPlugin : VideoPlayerPlugin
    {
        private float GetTimestamp(string url)
        {
            string[] queries = url.Split('&', '?');

            // YouTube timestamp is t=234s, where 234 is the number of seconds to skip
            foreach (string query in queries)
            {
                if (!query.StartsWith("t="))
                    continue;

                string[] parts = query.Split('=', 's');
                float result;
                bool parsed = float.TryParse(parts[1], out result);

                if (parsed)
                    return result;
            }

            // Twitch timestamp is t=00h44m07s
            foreach (string query in queries)
            {
                if (!query.StartsWith("t="))
                    continue;

                string[] parts = query.Split('=');
                if (parts.Length != 2)
                    continue;

                string[] numbers = parts[1].Split('h', 'm');
                if (numbers.Length != 3)
                    continue;

                int hours = 0;
                int minutes = 0;
                int seconds = 0;
                int.TryParse(numbers[0], out hours);
                int.TryParse(numbers[1], out minutes);
                int.TryParse(numbers[2], out seconds);

                int allSeconds = (hours * 60 * 60) + (minutes * 60) + seconds;

                if (allSeconds > 0)
                    return allSeconds;
            }

            return -1;
        }

        protected override void OnLoadReady(float duration)
        {
            string url = this.system.GetCurrentUrl();

            if (url == null)
                return;

            float timestamp = this.GetTimestamp(url.ToString());
            if (timestamp == -1)
                return;

            this.system.Seek(timestamp);
        }
    }
}
