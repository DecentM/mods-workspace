using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

namespace DecentM.VideoPlayer.Plugins
{
    public class URLVerifierPlugin : VideoPlayerPlugin
    {
        private bool ValidateUrl(string url)
        {
            // Some super vague heuristics to prevent some InvalidURL errors and keep the rate limit from being hit
            // by other video players in the world unnecessarily
            if (url == null || url == "")
                return false;

            if (
                url.StartsWith("http://")
                || url.StartsWith("https://")
                || url.StartsWith("localhost")
            )
                return true;

            if (url.Split('/').Length >= 2)
                return true;

            return false;
        }

        protected override void OnLoadRequested(VRCUrl url)
        {
            if (this.ValidateUrl(url.ToString()))
                return;

            this.events.OnLoadDenied(url, "URL failed validation");
        }
    }
}
