using UnityEngine;

using DecentM.Prefabs.VideoPlayer.Handlers;

namespace DecentM.Prefabs.VideoRatelimit
{
    [RequireComponent(typeof(PlayerHandler))]
    public class PlayerLoadMonitoring : MonoBehaviour
    {
        public VideoRatelimitSystem system;

        public void OnVideoReady()
        {
            this.system.OnPlayerLoad();
        }
    }
}
