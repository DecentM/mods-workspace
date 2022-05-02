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
        }
    }
}

