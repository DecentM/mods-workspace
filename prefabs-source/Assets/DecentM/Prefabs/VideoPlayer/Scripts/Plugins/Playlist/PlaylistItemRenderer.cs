using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;
using UnityEngine.UI;
using TMPro;

namespace DecentM.VideoPlayer.Plugins
{
    [UdonBehaviourSyncMode(BehaviourSyncMode.None)]
    public class PlaylistItemRenderer : UdonSharpBehaviour
    {
        public VideoPlaylist playlist;

        public int index;
        public VRCUrl url;
        public Image thumbnailSlot;
        public TextMeshProUGUI titleSlot;
        public TextMeshProUGUI durationSlot;
        public TextMeshProUGUI uploaderSlot;
        public TextMeshProUGUI statsSlot;
        public TextMeshProUGUI specsSlot;
        public TextMeshProUGUI descriptionSlot;

        public void SetData(
            int index,
            VRCUrl url,
            Sprite thumbnail,
            string title,
            string uploader,
            string platform,
            int views,
            int likes,
            string resolution,
            int fps,
            string description,
            string duration
        )
        {
            this.index = index;
            this.url = url;
            this.thumbnailSlot.sprite = thumbnail;
            this.titleSlot.text = title;
            this.durationSlot.text = duration;
            this.uploaderSlot.text = $"{uploader} - {platform}";

            if (views > 0 && likes > 0)
                this.statsSlot.text = $"{views} views, {likes} likes";
            else if (views > 0)
                this.statsSlot.text = $"{views} views";
            else if (likes > 0)
                this.statsSlot.text = $"{likes} likes";
            else
                this.statsSlot.text = $"";

            if (!string.IsNullOrEmpty(resolution) && fps > 0)
                this.specsSlot.text = $"{resolution}@{fps}fps";
            else if (!string.IsNullOrEmpty(resolution))
                this.specsSlot.text = resolution;
            else if (fps > 0)
                this.specsSlot.text = $"{fps}fps";
            else
                this.specsSlot.text = $"";

            this.descriptionSlot.text = description;
        }

        public void OnClick()
        {
            this.playlist.PlayIndex(this.index);
        }
    }
}
