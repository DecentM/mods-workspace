using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;
using VRC.SDK3.Video.Components.Base;

namespace DecentM.VideoRatelimit
{
    [UdonBehaviourSyncMode(BehaviourSyncMode.None), RequireComponent(typeof(BaseVRCVideoPlayer))]
    public class PlayerLoadMonitoring : UdonSharpBehaviour
    {
        public VideoRatelimitSystem system;

        public override void OnVideoReady()
        {
            this.system.OnPlayerLoad();
        }
    }
}
