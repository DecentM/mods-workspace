using System;
using UdonSharp;
using UnityEngine;
using VRC.SDK3.Components.Video;
using VRC.SDKBase;
using VRC.Udon;
using TMPro;

namespace DecentM.VideoPlayer.Plugins
{
    public class LoadingOverlayPlugin : VideoPlayerPlugin
    {
        public Animator animator;

        protected override void OnLoadBegin(VRCUrl url)
        {
            this.animator.SetBool("Loading", true);
        }

        protected override void OnLoadApproved(VRCUrl url)
        {
            this.animator.SetBool("Loading", true);
        }

        protected override void OnLoadBegin()
        {
            this.animator.SetBool("Loading", true);
        }

        protected override void OnAutoRetry(int attempt)
        {
            this.animator.SetBool("Loading", true);
        }

        protected override void OnLoadReady(float duration)
        {
            this.animator.SetBool("Loading", false);
        }

        protected override void OnLoadError(VideoError videoError)
        {
            this.animator.SetBool("Loading", false);
        }

        protected override void OnUnload()
        {
            this.animator.SetBool("Loading", false);
        }
    }
}
