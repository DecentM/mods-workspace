using System;
using UnityEngine;
using TMPro;

using DecentM.Prefabs.VideoRatelimit;

namespace DecentM.Prefabs.VideoPlayer.Plugins
{
    public class LoadRequestHandlerPlugin : VideoPlayerPlugin
    {
        public VideoRatelimitSystem ratelimit;

        private string approvalPending;
        public float approvalTimeout = 0.3f;

        protected override void OnLoadRequested(string vrcUrl)
        {
            if (this.approvalPending != null)
                return;

            this.denials = 0;
            this.approvalPending = vrcUrl;
            Invoke(nameof(CheckForDenials), this.approvalTimeout);
        }

        private int denials = 0;

        protected override void OnLoadDenied(string url, string reason)
        {
            if (this.approvalPending == null || url != this.approvalPending)
                return;

            this.denials++;
        }

        public void CheckForDenials()
        {
            if (this.denials != 0 || this.approvalPending == null)
            {
                this.approvalPending = null;
                return;
            }

            this.denials = 0;

            if (this.ratelimit == null)
            {
                this.events.OnLoadApproved(this.approvalPending);
                this.system.LoadVideo(this.approvalPending);
                this.approvalPending = null;
            }
            else
            {
                this.events.OnLoadRatelimitWaiting();
                this.ratelimit.RequestPlaybackWindow(this);
            }
        }

        public void OnPlaybackWindow()
        {
            if (this.approvalPending == null)
                return;

            this.events.OnLoadApproved(this.approvalPending);
            this.system.LoadVideo(this.approvalPending);
            this.approvalPending = null;
        }
    }
}
