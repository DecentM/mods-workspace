using System.Linq;
using System.Collections.Generic;

using UnityEditor;

using VRC.SDKBase;
using VRC.SDKBase.Editor.BuildPipeline;

using DecentM.EditorTools;
using DecentM.VideoPlayer;

public class VideoMetadataAutoFixer : AutoSceneFixer
{
    protected override bool OnPerformFixes()
    {
        return true;

        ComponentCollector<VideoPlaylist> collector = new ComponentCollector<VideoPlaylist>();
        List<VideoPlaylist> playlists = collector.CollectFromActiveScene();

        List<string> urls = new List<string>();
        List<string> refreshNeededUrls = new List<string>();

        foreach (VideoPlaylist playlist in playlists)
        {
            foreach (object[] item in playlist.urls)
            {
                VRCUrl url = (VRCUrl)item[0];
                urls.Add(url.ToString());
            }
        }

        foreach (string url in urls)
        {
            VideoMetadata metadata = VideoMetadataStore.GetCachedMetadata(url);

            if (string.IsNullOrEmpty(metadata.title) && metadata.thumbnail == null)
            {
                refreshNeededUrls.Add(url);
            }
        }

        bool accepted = VideoMetadataStore.RefreshMetadataAsync(refreshNeededUrls.ToArray());

        if (!accepted) return false;

        return true;
    }

    public override bool OnBuildRequested(VRCSDKRequestedBuildType requestedBuildType)
    {
        if (requestedBuildType != VRCSDKRequestedBuildType.Scene) return true;

        if (VideoMetadataStore.IsLocked)
        {
            int response = EditorUtility.DisplayDialogComplex(
                "Video metadata fetching in progress",
                "A background task is currently fetching video metadata. Aborting this will mean that some playlists will not have thumbnails and text on them. What would you like to do?",
                "Cancel build and continue fetching in background",
                "Switch to foreground and continue",
                "Continue build and cancel fetching"
            );

            switch (response)
            {
                case 0: return false;
                case 2: return true;
            }
        }

        return true;
    }
}
