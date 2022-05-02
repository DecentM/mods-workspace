using UnityEditor;
using UnityEditorInternal;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DecentM.EditorTools;
using System.Linq;
using VRC.SDKBase;

namespace DecentM.VideoPlayer
{
    [CustomEditor(typeof(VideoPlaylist))]
    public class VideoPlaylistInspector : Inspector
    {
        public const int UrlHeight = 150;
        public const int paddingUnit = 8;

        public override void OnInspectorGUI()
        {
            VideoPlaylist playlist = (VideoPlaylist)target;

            Rect screenRect = new Rect(0, 0, Screen.width, Screen.height);
            EditorGUI.DrawRect(screenRect, new Color(38 / 255f, 38 / 255f, 38 / 255f));

            for (int i = 0; i < playlist.urls.Length; i++)
            {
                VideoMetadata videoMetadata = VideoMetadataStore.GetMetadata(playlist.urls[i].ToString());

                Rect regionOuter = this.DrawRegion(UrlHeight, new Vector4(paddingUnit, paddingUnit, paddingUnit, paddingUnit / 2));
                Rect region = this.GetRectInside(regionOuter, new Vector4(paddingUnit, paddingUnit, paddingUnit, paddingUnit));

                EditorGUI.DrawRect(region, new Color(56 / 255f, 56 / 255f, 56 / 255f));

                Rect thumbnailRectOuter = this.GetRectInside(region, new Vector2(region.width / 3, region.height));
                Rect thumbnailRectInner = this.GetRectInside(thumbnailRectOuter, new Vector4(paddingUnit, paddingUnit, paddingUnit, paddingUnit));

                if (videoMetadata.thumbnail != null)
                {
                    float thumbnailAspectRatio = 1f * videoMetadata.thumbnail.height / videoMetadata.thumbnail.width;
                    thumbnailRectInner.width = thumbnailRectInner.height / thumbnailAspectRatio;
                    this.DrawImage(videoMetadata.thumbnail, thumbnailRectInner);
                }
                else
                {
                    this.DrawImage(EditorAssets.FallbackVideoThumbnail, thumbnailRectInner);
                }

                Rect textRectOuter = this.GetRectInside(region, new Vector2(region.width - thumbnailRectInner.width, region.height), new Vector4(thumbnailRectInner.width + 8, 0, 0, 0));
                Rect textRectInner = this.GetRectInside(textRectOuter, new Vector4(paddingUnit, paddingUnit, paddingUnit, paddingUnit));

                int count = 0;
                float height = textRectInner.height / 5;

                Rect titleRect = this.GetRectInside(textRectInner, new Vector2(textRectInner.width, height), new Vector4(paddingUnit, count * height, paddingUnit, 0));
                this.DrawLabel(titleRect, videoMetadata.title, 3, FontStyle.Bold);

                count++;

                Rect uploaderRect = this.GetRectInside(textRectInner, new Vector2(textRectInner.width, height), new Vector4(paddingUnit, count * height, paddingUnit, 0));
                List<string> uploaderLabels = new List<string>();
                if (videoMetadata.uploader != null) uploaderLabels.Add(videoMetadata.uploader);
                if (videoMetadata.siteName != null) uploaderLabels.Add(videoMetadata.siteName);
                this.DrawLabel(uploaderRect, string.Join(" - ", uploaderLabels.ToArray()), 2);

                count++;

                Rect countersRect = this.GetRectInside(textRectInner, new Vector2(textRectInner.width, height), new Vector4(paddingUnit, count * height, paddingUnit, 0));
                List<string> counterLabels = new List<string>();
                if (videoMetadata.viewCount != null) counterLabels.Add($"{videoMetadata.viewCount} views");
                if (videoMetadata.likeCount != null) counterLabels.Add($"{videoMetadata.likeCount} likes");
                this.DrawLabel(countersRect, string.Join(", ", counterLabels.ToArray()), 2);

                count++;

                Rect specsRect = this.GetRectInside(textRectInner, new Vector2(textRectInner.width, height), new Vector4(paddingUnit, count * height, paddingUnit, 0));
                List<string> techLabels = new List<string>();
                if (videoMetadata.resolution != null) techLabels.Add(videoMetadata.resolution);
                if (videoMetadata.fps != 0) techLabels.Add($"{videoMetadata.fps}fps");
                this.DrawLabel(specsRect, string.Join("@", techLabels.ToArray()), 2);

                count++;

                Rect urlRect = this.GetRectInside(textRectInner, new Vector2(textRectInner.width, height), new Vector4(paddingUnit, count * height, paddingUnit, 0));
                playlist.urls[i] = new VRCUrl(EditorGUI.TextField(urlRect, playlist.urls[i].ToString()));
            }

            this.SaveModifications();
        }
    }
}

