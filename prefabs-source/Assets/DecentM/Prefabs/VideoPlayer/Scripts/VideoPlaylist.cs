using System;
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

#if UNITY_EDITOR && !COMPILER_UDONSHARP
using UnityEditor;
#endif

namespace DecentM.VideoPlayer
{
    [UdonBehaviourSyncMode(BehaviourSyncMode.None)]
    public class VideoPlaylist : UdonSharpBehaviour, ISerializationCallbackReceiver
    {
        /*
         * item structure:
         * 0 - VRCUrl url
         * 1 - Texture2D thumbnail
         * 2 - string title
         * 3 - string uploader
         * 4 - string platform
         * 5 - int views
         * 6 - int likes
         * 7 - string resolution
         * 8 - int fps
         */

        public bool looping = true;
        public object[][] urls;

        [SerializeField]
        public VRCUrl[] serialisedUrls;
        [SerializeField]
        private Texture2D[] serialisedThumbnails;
        [SerializeField]
        private string[] serialisedTitles;
        [SerializeField]
        private string[] serialisedUploaders;
        [SerializeField]
        private string[] serialisedPlatforms;
        [SerializeField]
        private int[] serialisedViews;
        [SerializeField]
        private int[] serialisedLikes;
        [SerializeField]
        private string[] serialisedResolutions;
        [SerializeField]
        private int[] serialisedFpses;

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

                && this.serialisedUrls.Length == this.urls.Length
                && this.serialisedThumbnails.Length == this.urls.Length
                && this.serialisedTitles.Length == this.urls.Length
                && this.serialisedUploaders.Length == this.urls.Length
                && this.serialisedPlatforms.Length == this.urls.Length
                && this.serialisedViews.Length == this.urls.Length
                && this.serialisedLikes.Length == this.urls.Length
                && this.serialisedResolutions.Length == this.urls.Length
                && this.serialisedFpses.Length == this.urls.Length
            ) return;

            this.serialisedUrls = new VRCUrl[this.urls.Length];
            this.serialisedThumbnails = new Texture2D[this.urls.Length];
            this.serialisedTitles = new string[this.urls.Length];
            this.serialisedUploaders = new string[this.urls.Length];
            this.serialisedPlatforms = new string[this.urls.Length];
            this.serialisedViews = new int[this.urls.Length];
            this.serialisedLikes = new int[this.urls.Length];
            this.serialisedResolutions = new string[this.urls.Length];
            this.serialisedFpses = new int[this.urls.Length];

            for (int i = 0; i < this.urls.Length; i++)
            {
#if UNITY_EDITOR && !COMPILER_UDONSHARP
                bool showProgress = this.urls.Length > 35;
                if (showProgress) EditorUtility.DisplayProgressBar($"[DecentM.VideoPlayer] Serializing playlist...", $"{i}/{this.urls.Length}", (float)i / this.urls.Length);
#endif

                object[] item = this.urls[i];
                if (item == null) continue;

                VRCUrl url = (VRCUrl)item[0];
                Texture2D thumbnail = (Texture2D)item[1];
                string title = (string)item[2];
                string uploader = (string)item[3];
                string platform = (string)item[4];
                int views = (int)item[5];
                int likes = (int)item[6];
                string resolution = (string)item[7];
                int fps = (int)item[8];

                this.serialisedUrls[i] = url;
                this.serialisedThumbnails[i] = thumbnail;
                this.serialisedTitles[i] = title;
                this.serialisedUploaders[i] = uploader;
                this.serialisedPlatforms[i] = platform;
                this.serialisedLikes[i] = likes;
                this.serialisedViews[i] = views;
                this.serialisedResolutions[i] = resolution;
                this.serialisedFpses[i] = fps;
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
            ) return;

            this.urls = new object[this.serialisedUrls.Length][];

            for (var i = 0; i < this.serialisedUrls.Length; i++)
            {
                VRCUrl url = this.serialisedUrls[i];
                if (url == null) continue;

                Texture2D thumbnail = this.serialisedThumbnails[i];
                string title = this.serialisedTitles[i];
                string uploader = this.serialisedUploaders[i];
                string platform = this.serialisedPlatforms[i];
                int likes = this.serialisedLikes[i];
                int views = this.serialisedViews[i];
                string resolution = this.serialisedResolutions[i];
                int fps = this.serialisedFpses[i];

                object[] item = new object[] { url, thumbnail, title, uploader, platform, likes, views, resolution, fps };
                this.urls[i] = item;
            }
        }

        private int currentIndex = 0;

        public VRCUrl Next()
        {
            if (this.urls == null || this.urls.Length == 0) return null;

            this.currentIndex++;

            if (this.currentIndex >= urls.Length)
            {
                if (this.looping) this.currentIndex = 0;
                else return null;
            }

            return (VRCUrl)this.urls[this.currentIndex][0];
        }

        public VRCUrl Previous()
        {
            if (this.urls == null || this.urls.Length == 0) return null;

            this.currentIndex--;

            if (this.currentIndex < 0)
            {
                if (this.looping) this.currentIndex = this.urls.Length - 1;
                else return null;
            }

            return (VRCUrl)this.urls[this.currentIndex][0];
        }

        public VRCUrl GetCurrentUrl()
        {
            if (this.urls == null || this.urls.Length == 0 || this.currentIndex >= urls.Length) return null;

            return (VRCUrl)this.urls[this.currentIndex][0];
        }

        public int AddUrl(VRCUrl url)
        {
            object[] item = new object[] { url, null, "", "", "", 0, 0, "", 0 };

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
    }
}
