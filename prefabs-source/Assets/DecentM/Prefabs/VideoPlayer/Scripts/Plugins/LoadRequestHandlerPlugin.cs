using System;
using UdonSharp;
using UnityEngine;
using VRC.SDK3.Components.Video;
using VRC.SDKBase;
using VRC.Udon;
using TMPro;

using DecentM.VideoRatelimit;

namespace DecentM.VideoPlayer.Plugins
{
    public class LoadRequestHandlerPlugin : VideoPlayerPlugin
    {
        public VideoRatelimitSystem ratelimit;

        private VRCUrl approvalPending;
        public float approvalTimeout = 0.3f;

        protected override void OnLoadRequested(VRCUrl vrcUrl)
        {
            if (this.approvalPending != null)
                return;

            this.denials = 0;
            this.approvalPending = vrcUrl;
            this.SendCustomEventDelayedSeconds(nameof(CheckForDenials), this.approvalTimeout);
        }

        private int denials = 0;

        protected override void OnLoadDenied(VRCUrl url, string reason)
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
