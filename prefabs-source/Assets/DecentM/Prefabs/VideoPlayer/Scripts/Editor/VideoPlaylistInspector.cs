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
        public override void OnInspectorGUI()
        {
            VideoPlaylist playlist = (VideoPlaylist)target;

            for (int i = 0; i < playlist.urls.Length; i++)
            {
                Rect regionOuter = this.DrawRegion(150, new Vector4(8, 8, 8, 8));
                Rect region = this.GetRectInside(regionOuter, new Vector4(8, 8, 8, 8));
                EditorGUI.DrawRect(region, Color.grey);

                Rect thumbnailRectOuter = this.GetRectInside(region, new Vector2(region.width / 3, region.height));
                Rect thumbnailRectInner = this.GetRectInside(thumbnailRectOuter, new Vector4(8, 8, 8, 8));
                Texture thumbnail = VideoThumbnailStore.GetThumbnail(playlist.urls[i]);

                if (thumbnail == null)
                {
                    VideoThumbnailStore.FetchThumbnail(playlist.urls[i]);
                    thumbnail = VideoThumbnailStore.GetThumbnail(playlist.urls[i]);
                }

                this.DrawImage(thumbnail, thumbnailRectInner);
                // this.DrawImage(EditorAssets.FallbackVideoThumbnail, thumbnailRectInner);

                Rect textRectOuter = this.GetRectInside(region, new Vector2(region.width / 3 * 2, region.height), new Vector4(region.width / 3, 0, 0, 0));
                Rect textRectInner = this.GetRectInside(textRectOuter, new Vector4(8, 8, 8, 8));

                int count = 0;
                float height = textRectInner.height / 3;

                Rect titleRect = this.GetRectInside(textRectInner, new Vector2(textRectInner.width, height), new Vector4(8, count * height, 8, 0));
                this.DrawLabel(titleRect, "The Original Was Better TBH - Let's Play Stanley Parable Ultra Deluxe Part 2 [Blind PC Gameplay]", 4, FontStyle.Bold);

                count++;

                Rect uploaderRect = this.GetRectInside(textRectInner, new Vector2(textRectInner.width, height), new Vector4(8, count * height, 8, 0));
                this.DrawLabel(uploaderRect, "Materwelonz", 5);

                count++;

                Rect urlRect = this.GetRectInside(textRectInner, new Vector2(textRectInner.width, 20), new Vector4(8, (count * height) + 16, 8, 0));
                playlist.urls[i] = new VRCUrl(EditorGUI.TextField(urlRect, playlist.urls[i].ToString()));
            }

            // Rect thumbnailRect = this.GetRectInside(region, new Vector2(region.width / 3, 1));
            // EditorGUI.DrawRect(thumbnailRect, Color.grey);

            /*
            this.HelpBox(MessageType.Info, playlist.looping
                ? "When the last item is reached, the playlist will start over from the beginning"
                : "When the last item is reached, playback will end"
            );

            playlist.looping = this.Toggle("Loop playlist", playlist.looping);

            EditorGUI.BeginDisabledGroup(true);
            EditorGUILayout.TextField(Application.dataPath);
            EditorGUI.EndDisabledGroup();

            for (int i = 0; i < playlist.urls.Length; i++)
            {
                Rect region = this.DrawRegion(50);

                Rect urlField = new Rect(region);
                urlField.height = 20;

                playlist.urls[i] = new VRCUrl(EditorGUI.TextField(urlField, playlist.urls[i].ToString()));
                Texture thumbnail = VideoThumbnailStore.GetThumbnail(playlist.urls[i]);

                if (thumbnail == null)
                {
                    VideoThumbnailStore.FetchThumbnail(playlist.urls[i]);
                    thumbnail = VideoThumbnailStore.GetThumbnail(playlist.urls[i]);
                }

                this.DrawImage(thumbnail);
            }
            */
        }
    }
}

