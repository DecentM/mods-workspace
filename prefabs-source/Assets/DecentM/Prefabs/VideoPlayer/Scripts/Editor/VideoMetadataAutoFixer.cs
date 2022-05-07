using System.Linq;
using System.Collections.Generic;

using UnityEditor;
using UnityEngine;

using VRC.SDKBase;
using VRC.SDKBase.Editor.BuildPipeline;

using DecentM.EditorTools;

namespace DecentM.VideoPlayer.EditorTools
{
    public class VideoMetadataAutoFixer : AutoSceneFixer
    {
        private List<string> GetAllUrlsInScene()
        {
            ComponentCollector<VideoPlaylist> collector = new ComponentCollector<VideoPlaylist>();
            List<VideoPlaylist> playlists = collector.CollectFromActiveScene();

            List<string> urls = new List<string>();

            foreach (VideoPlaylist playlist in playlists)
            {
                foreach (object[] item in playlist.urls)
                {
                    VRCUrl url = (VRCUrl)item[0];
                    urls.Add(url.ToString());
                }
            }

            return urls;
        }

        protected override bool OnPerformFixes()
        {
            return true;

            List<string> urls = this.GetAllUrlsInScene();

            // bool accepted = VideoMetadataStore.Refresh(urls.ToArray());

            // if (!accepted) return false;

            return true;
        }

        public override bool OnBuildRequested(VRCSDKRequestedBuildType requestedBuildType)
        {
            if (requestedBuildType != VRCSDKRequestedBuildType.Scene) return true;

            List<string> urls = this.GetAllUrlsInScene();
            // VideoMetadataStore.Refresh(urls.ToArray());

            return true;
        }
    }
}
