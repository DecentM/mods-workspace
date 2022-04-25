using System;
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

namespace DecentM.VideoPlayer
{
    [UdonBehaviourSyncMode(BehaviourSyncMode.None)]
    public class VideoPlaylist : UdonSharpBehaviour
    {
        public bool looping = true;
        public VRCUrl[] urls;

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

            return this.urls[this.currentIndex];
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

            return this.urls[this.currentIndex];
        }

        public VRCUrl GetCurrentUrl()
        {
            if (this.urls == null || this.urls.Length == 0 || this.currentIndex >= urls.Length) return null;

            return this.urls[this.currentIndex];
        }

        public VRCUrl[] GetAllUrls()
        {
            return this.urls;
        }

        public int AddUrl(VRCUrl url)
        {
            if (this.urls != null)
            {
                VRCUrl[] tmp = new VRCUrl[this.urls.Length + 1];
                Array.Copy(this.urls, 0, tmp, 0, this.urls.Length);
                tmp[tmp.Length - 1] = url;
                this.urls = tmp;
            }
            else
            {
                VRCUrl[] tmp = new VRCUrl[1];
                tmp[0] = url;
                this.urls = tmp;
            }

            return this.urls.Length - 1;
        }

        public bool RemoveUrl(int index)
        {
            if (this.urls == null || this.urls.Length == 0 || index < 0 || index >= this.urls.Length) return false;

            VRCUrl[] tmp = new VRCUrl[urls.Length + 1];
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

            VRCUrl[] tmp = new VRCUrl[this.urls.Length];
            Array.Copy(this.urls, tmp, this.urls.Length);
            tmp[indexB] = this.urls[indexA];
            tmp[indexA] = this.urls[indexB];
            this.urls = tmp;
            return true;
        }
    }
}
