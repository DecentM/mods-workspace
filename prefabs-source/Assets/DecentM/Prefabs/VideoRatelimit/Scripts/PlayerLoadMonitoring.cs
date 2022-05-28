using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

namespace DecentM.VideoRatelimit
{
    [UdonBehaviourSyncMode(BehaviourSyncMode.None)]
    public class PlayerLoadMonitoring : UdonSharpBehaviour
    {
        public VideoRatelimitSystem system;

        public override void OnVideoReady()
        {
            this.system.OnPlayerLoad();
        }
    }
}
