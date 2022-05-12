using UdonSharp;
using UnityEngine;
using VRC.SDK3.Components.Video;
using VRC.SDKBase;
using VRC.Udon;

namespace DecentM.VideoPlayer.Plugins
{
    public class AutoRetryPlugin : VideoPlayerPlugin
    {
        [Tooltip(
            "Switch to the next player handler after this many failures. Each attempt takes 5 seconds."
        )]
        public int failureCeiling = 3;
        public bool abortAfterAllPlayersFailed = true;
        private int failures = 0;

        public int videoLoadTimeout = 10;
        private float timeoutClock = 0;
        private bool waitingForLoad = false;

        private void FixedUpdate()
        {
            if (!this.waitingForLoad)
                return;

            this.timeoutClock += Time.fixedDeltaTime;
            if (this.timeoutClock > this.videoLoadTimeout + ((this.failures + 1) * 5))
            {
                this.timeoutClock = 0;
                this.OnLoadError(VideoError.Unknown);
                this.events.OnAutoRetryLoadTimeout((this.failures + 1) * 5);
            }
        }

        public void AttemptRetry()
        {
            VRCUrl url = this.system.GetCurrentUrl();
            if (url == null)
                return;

            this.system.RequestVideo(url);
        }

        protected override void OnLoadApproved(VRCUrl url)
        {
            this.timeoutClock = 0;
            this.waitingForLoad = true;
        }

        protected override void OnLoadError(VideoError videoError)
        {
            VRCUrl url = this.system.GetCurrentUrl();
            if (url == null || string.IsNullOrEmpty(this.system.GetCurrentUrl().ToString()))
                return;

            switch (videoError)
            {
                // Continue for rate limit errors and unknown ones
                // (we repurposed unknown to mean player timeout as well)
                case VideoError.Unknown:
                    break;
                case VideoError.RateLimited:
                    break;
                case VideoError.PlayerError:
                    break;
                // Don't retry for errors that are unlikely to be temporary
                case VideoError.InvalidURL:
                case VideoError.AccessDenied:
                    this.timeoutClock = 0;
                    this.failures = 0;
                    this.events.OnAutoRetryAbort();
                    return;
            }

            this.waitingForLoad = false;
            this.failures++;

            // If we reach a limit, try with the next player handler as the current one may be broken.
            if (this.failures >= this.failureCeiling)
            {
                this.failures = 0;

                // If the next player handler index is 0, it means we've gone around all of them
                // TODO: This above statement isn't true if we didn't start playback using the first player
                if (this.system.NextPlayerHandler() == 0)
                {
                    if (abortAfterAllPlayersFailed)
                    {
                        this.system.UnloadVideo();
                        this.events.OnAutoRetryAbort();
                        return;
                    }
                    else
                    {
                        this.events.OnAutoRetryAllPlayersFailed();
                    }
                }
            }

            // Schedule a retry after the rate limit expires
            this.events.OnAutoRetry(this.failures);
            this.SendCustomEventDelayedSeconds(nameof(AttemptRetry), 5.1f);
        }

        protected override void OnLoadReady(float duration)
        {
            // Reset the failure count after a video loads successfully
            this.failures = 0;
            this.waitingForLoad = false;
        }

        protected override void OnUnload()
        {
            this.timeoutClock = 0;
            this.failures = 0;
            this.waitingForLoad = false;
        }
    }
}
