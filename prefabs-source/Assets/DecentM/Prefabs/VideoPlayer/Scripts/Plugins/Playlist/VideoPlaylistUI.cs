using System;
using UnityEngine;

using UnityEngine.UI;

namespace DecentM.Prefabs.VideoPlayer.Plugins
{
    public class VideoPlaylistUI : MonoBehaviour
    {
        public Transform itemsRoot;
        public GameObject itemRendererTemplate;
        public VideoPlaylist playlist;
        public RectTransform lockedOverlay;
        private Canvas canvas;

        private PlaylistItemRenderer[] instances;

        private bool isInstantiating = false;
        private int instantiatingIndex = 0;
        private float elapsed = 0;
        private float instantiatingInterval = 1;

        private void FixedUpdate()
        {
            if (!this.isInstantiating || Time.deltaTime >= 1 / 49f)
                return;

            elapsed += Time.fixedUnscaledDeltaTime;
            if (elapsed <= this.instantiatingInterval)
                return;
            elapsed = 0;
            this.instantiatingInterval = UnityEngine.Random.Range(.05f, .5f);

            if (this.instantiatingIndex >= playlist.urls.Length)
            {
                this.isInstantiating = false;
                return;
            }

            object[] item = playlist.urls[this.instantiatingIndex];
            this.instantiatingIndex++;

            if (item == null)
                return;

            GameObject instance = Instantiate(this.itemRendererTemplate);
            PlaylistItemRenderer renderer = instance.GetComponent<PlaylistItemRenderer>();

            if (renderer == null)
                return;

            string url = (string)item[0];
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

            instance.transform.SetPositionAndRotation(
                this.itemRendererTemplate.transform.position,
                this.itemRendererTemplate.transform.rotation
            );
            instance.transform.SetParent(this.itemsRoot, true);
            instance.transform.localScale = this.itemRendererTemplate.transform.localScale;
            instance.name = $"{renderer.name}_{this.instantiatingIndex - 1}";
            renderer.SetData(
                this.instantiatingIndex - 1,
                url,
                thumbnail,
                title,
                uploader,
                platform,
                views,
                likes,
                resolution,
                fps,
                description,
                duration
            );
            instance.gameObject.SetActive(true);

            PlaylistItemRenderer[] tmp = new PlaylistItemRenderer[this.instances.Length + 1];
            Array.Copy(this.instances, tmp, this.instances.Length);
            tmp[tmp.Length - 1] = renderer;
            this.instances = tmp;
        }

        void Start()
        {
            this.playlist.SetUI(this);
            this.canvas = GetComponent<Canvas>();
            this.instances = new PlaylistItemRenderer[playlist.urls.Length];
            this.itemRendererTemplate.SetActive(false);

            this.isInstantiating = true;
        }

        public void SetIsOwner(bool isOwner)
        {
            this.canvas.enabled = isOwner;
            this.lockedOverlay.gameObject.SetActive(!isOwner);
        }
    }
}
