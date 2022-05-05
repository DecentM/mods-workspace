using System;
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

#if UNITY_EDITOR && !COMPILER_UDONSHARP
using UnityEditor;
#endif

using DecentM.VideoPlayer.Plugins;

namespace DecentM.VideoPlayer
{
    [UdonBehaviourSyncMode(BehaviourSyncMode.None)]
    public class VideoPlaylist : UdonSharpBehaviour, ISerializationCallbackReceiver
    {
        /*
         * subtitle structure
         * 0 - string lang
         * 1 - string instructions
         */

        /*
         * item structure:
         * 0 - VRCUrl url
         * 1 - Sprite thumbnail
         * 2 - string title
         * 3 - string uploader
         * 4 - string platform
         * 5 - int views
         * 6 - int likes
         * 7 - string resolution
         * 8 - int fps
         * 9 - string description
         * 10 - string duration
         * 11 - string[][] subtitles
         */

        [Space]
        [Header("Settings")]
        public string title = "";
        public string author = "";
        [TextArea]
        public string description = "";
        [Space]
        public bool looping = true;
        public bool shuffle = true;
        [Space]
        public PlaylistPlayerPlugin playlistPlayer;
        public TextureUpdaterPlugin textureUpdater;
        [HideInInspector]
        public VideoPlaylistUI ui;

        [HideInInspector]
        public object[][] urls;

        #region Serialisation

        [SerializeField, HideInInspector]
        public VRCUrl[] serialisedUrls;
        [SerializeField, HideInInspector]
        private Sprite[] serialisedThumbnails;
        [SerializeField, HideInInspector]
        private string[] serialisedTitles;
        [SerializeField, HideInInspector]
        private string[] serialisedUploaders;
        [SerializeField, HideInInspector]
        private string[] serialisedPlatforms;
        [SerializeField, HideInInspector]
        private int[] serialisedViews;
        [SerializeField, HideInInspector]
        private int[] serialisedLikes;
        [SerializeField, HideInInspector]
        private string[] serialisedResolutions;
        [SerializeField, HideInInspector]
        private int[] serialisedFpses;
        [SerializeField, HideInInspector]
        private string[] serialisedDescriptions;
        [SerializeField, HideInInspector]
        private string[] serialisedDurations;
        [SerializeField, HideInInspector]
        private string[] serialisedSubtitles;

        private const char SubtitleSeparator = 'ˠ';

        public void OnBeforeSerialize()
        {
            if (this.urls == null) return;

            if (
                this.serialisedUrls != null
                && this.serialisedThumbnails != null
                && this.serialisedTitles != null
                && this.serialisedUploaders != null
                && this.serialisedPlatforms != null
                && this.serialisedViews != null
                && this.serialisedLikes != null
                && this.serialisedResolutions != null
                && this.serialisedFpses != null
                && this.serialisedDescriptions != null
                && this.serialisedDurations != null
                && this.serialisedSubtitles != null

                && this.serialisedUrls.Length == this.urls.Length
                && this.serialisedThumbnails.Length == this.urls.Length
                && this.serialisedTitles.Length == this.urls.Length
                && this.serialisedUploaders.Length == this.urls.Length
                && this.serialisedPlatforms.Length == this.urls.Length
                && this.serialisedViews.Length == this.urls.Length
                && this.serialisedLikes.Length == this.urls.Length
                && this.serialisedResolutions.Length == this.urls.Length
                && this.serialisedFpses.Length == this.urls.Length
                && this.serialisedDescriptions.Length == this.urls.Length
                && this.serialisedDurations.Length == this.urls.Length
                && this.serialisedSubtitles.Length == this.urls.Length
            ) return;

            this.serialisedUrls = new VRCUrl[this.urls.Length];
            this.serialisedThumbnails = new Sprite[this.urls.Length];
            this.serialisedTitles = new string[this.urls.Length];
            this.serialisedUploaders = new string[this.urls.Length];
            this.serialisedPlatforms = new string[this.urls.Length];
            this.serialisedViews = new int[this.urls.Length];
            this.serialisedLikes = new int[this.urls.Length];
            this.serialisedResolutions = new string[this.urls.Length];
            this.serialisedFpses = new int[this.urls.Length];
            this.serialisedDescriptions = new string[this.urls.Length];
            this.serialisedDurations = new string[this.urls.Length];
            this.serialisedSubtitles = new string[this.urls.Length];

            for (int i = 0; i < this.urls.Length; i++)
            {
#if UNITY_EDITOR && !COMPILER_UDONSHARP
                bool showProgress = this.urls.Length > 35;
                if (showProgress) EditorUtility.DisplayProgressBar($"[DecentM.VideoPlayer] Serializing playlist...", $"{i}/{this.urls.Length}", (float)i / this.urls.Length);
#endif

                object[] item = this.urls[i];
                if (item == null) continue;

                VRCUrl url = (VRCUrl)item[0];
                Sprite thumbnail = (Sprite)item[1];
                string title = (string)item[2];
                string uploader = (string)item[3];
                string platform = (string)item[4];
                int views = (int)item[5];
                int likes = (int)item[6];
                string resolution = (string)item[7];
                int fps = (int)item[8];
                string description = (string)item[9];
                string duration = (string)item[10];
                string[][] subtitles = (string[][])item[11];

                this.serialisedUrls[i] = url;
                this.serialisedThumbnails[i] = thumbnail;
                this.serialisedTitles[i] = title;
                this.serialisedUploaders[i] = uploader;
                this.serialisedPlatforms[i] = platform;
                this.serialisedViews[i] = views;
                this.serialisedLikes[i] = likes;
                this.serialisedResolutions[i] = resolution;
                this.serialisedFpses[i] = fps;
                this.serialisedDescriptions[i] = description;
                this.serialisedDurations[i] = duration;

                string serialisedSubtitle = "";

                for (int j = 0; j < subtitles.Length; j++)
                {
                    string[] subtitle = subtitles[j];
                    if (subtitle == null) continue;

                    serialisedSubtitle += subtitle[0]; // lang
                    serialisedSubtitle += SubtitleSeparator;
                    serialisedSubtitle += subtitle[1]; // content
                    serialisedSubtitle += SubtitleSeparator;
                }

                this.serialisedSubtitles[i] = serialisedSubtitle;
            }

#if UNITY_EDITOR && !COMPILER_UDONSHARP
            EditorUtility.ClearProgressBar();
#endif
        }

        public void OnAfterDeserialize()
        {
            if (
                this.urls != null

                && this.serialisedUrls.Length == this.urls.Length
                && this.serialisedThumbnails.Length == this.urls.Length
                && this.serialisedTitles.Length == this.urls.Length
                && this.serialisedUploaders.Length == this.urls.Length
                && this.serialisedPlatforms.Length == this.urls.Length
                && this.serialisedViews.Length == this.urls.Length
                && this.serialisedLikes.Length == this.urls.Length
                && this.serialisedResolutions.Length == this.urls.Length
                && this.serialisedFpses.Length == this.urls.Length
                && this.serialisedDescriptions.Length == this.urls.Length
                && this.serialisedDurations.Length == this.urls.Length
                && this.serialisedSubtitles.Length == this.urls.Length
            ) return;

            this.urls = new object[this.serialisedUrls.Length][];

            for (var i = 0; i < this.serialisedUrls.Length; i++)
            {
                VRCUrl url = this.serialisedUrls[i];
                if (url == null) continue;

                Sprite thumbnail = this.serialisedThumbnails[i];
                string title = this.serialisedTitles[i];
                string uploader = this.serialisedUploaders[i];
                string platform = this.serialisedPlatforms[i];
                int views = this.serialisedViews[i];
                int likes = this.serialisedLikes[i];
                string resolution = this.serialisedResolutions[i];
                int fps = this.serialisedFpses[i];
                string description = this.serialisedDescriptions[i];
                string duration = this.serialisedDurations[i];
                string subtitles = this.serialisedSubtitles[i];

                string[][] deserialisedSubtitles = new string[0][];
                string[] subtitleParts = subtitles.Split(SubtitleSeparator);

                // Debug.Log($"[Deserialisation] parts length {subtitleParts.Length}");

                for (int j = 0; j < subtitleParts.Length - 1; j += 2)
                {
                    string lang = subtitleParts[j];
                    string content = subtitleParts[j + 1];

                    if (lang == null || content == null)
                    {
                        // Debug.LogWarning($"[Deserialisation] subtitle parts parity mismatch!");
                        continue;
                    }

                    string[][] tmp = new string[deserialisedSubtitles.Length + 1][];
                    Array.Copy(deserialisedSubtitles, tmp, deserialisedSubtitles.Length);
                    tmp[tmp.Length - 1] = new string[] { lang, content };
                    deserialisedSubtitles = tmp;
                }

                object[] item = new object[] { url, thumbnail, title, uploader, platform, views, likes, resolution, fps, description, duration, deserialisedSubtitles };
                this.urls[i] = item;
            }
        }

        #endregion

        private int currentIndex = 0;

        public object[] Next()
        {
            if (this.urls == null || this.urls.Length == 0) return null;

            this.currentIndex++;

            if (this.currentIndex >= urls.Length)
            {
                if (this.looping) this.currentIndex = 0;
                else return null;
            }

            return this.urls[this.currentIndex];
        }

        public object[] Previous()
        {
            if (this.urls == null || this.urls.Length == 0) return null;

            this.currentIndex--;

            if (this.currentIndex < 0)
            {
                if (this.looping) this.currentIndex = this.urls.Length - 1;
                else return null;
            }

            return this.urls[this.currentIndex];
        }

        public object[] GetCurrent()
        {
            if (this.urls == null || this.urls.Length == 0 || this.currentIndex >= urls.Length) return null;

            return this.urls[this.currentIndex];
        }

        public int AddUrl(VRCUrl url)
        {
            object[] item = new object[] { url, null, "", "", "", 0, 0, "", 0, "", "", new string[][] { } };

            if (this.urls != null)
            {
                object[][] tmp = new object[this.urls.Length + 1][];
                Array.Copy(this.urls, 0, tmp, 0, this.urls.Length);
                tmp[tmp.Length - 1] = item;
                this.urls = tmp;
            }
            else
            {
                object[][] tmp = new object[1][];
                tmp[0] = item;
                this.urls = tmp;
            }

            return this.urls.Length - 1;
        }

        public bool RemoveUrl(int index)
        {
            if (this.urls == null || this.urls.Length == 0 || index < 0 || index >= this.urls.Length) return false;

            object[][] tmp = new object[urls.Length - 1][];
            Array.Copy(this.urls, 0, tmp, 0, index);
            Array.Copy(this.urls, index + 1, tmp, index, this.urls.Length - 1 - index);
            this.urls = tmp;

            return true;
        }

        public bool Swap(int indexA, int indexB)
        {
            if (
                indexA < 0
                || indexB < 0
                || indexA >= this.urls.Length
                || indexA >= this.urls.Length
            ) return false;

            object[][] tmp = new object[this.urls.Length][];
            Array.Copy(this.urls, tmp, this.urls.Length);
            tmp[indexB] = this.urls[indexA];
            tmp[indexA] = this.urls[indexB];
            this.urls = tmp;
            return true;
        }

        public object[] GetIndex(int index)
        {
            if (index < 0 || index >= this.urls.Length) return null;

            object[] item = this.urls[index];
            if (item == null || item.Length == 0) return null;

            return item;
        }

        public bool PlayIndex(int index)
        {
            object[] item = this.GetIndex(index);
            if (item == null) return false;

            this.currentIndex = index;
            this.playlistPlayer.SetCurrentPlaylist(this);
            this.playlistPlayer.PlayItem(item);
            return true;
        }
    }
}
