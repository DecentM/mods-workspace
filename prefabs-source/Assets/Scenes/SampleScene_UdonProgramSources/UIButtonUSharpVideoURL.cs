
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;
using UdonSharp.Video;

[UdonBehaviourSyncMode(BehaviourSyncMode.Manual)]
public class UIButtonUSharpVideoURL : UdonSharpBehaviour
{
    public USharpVideoPlayer player;
    public VRCUrl url;

    public void OnClick()
    {
        this.player.PlayVideo(this.url);
    }
}
