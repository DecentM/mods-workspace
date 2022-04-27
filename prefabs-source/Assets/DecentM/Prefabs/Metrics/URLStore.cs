
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

namespace DecentM.Metrics
{
    [UdonBehaviourSyncMode(BehaviourSyncMode.None)]
    public class URLStore : UdonSharpBehaviour
    {
        public VRCUrl[] playerCountUrls;
        public VRCUrl respawnUrl;
        public VRCUrl heartbeatUrl;

        public VRCUrl GetPlayerCountUrl(int count)
        {
            if (count < 0 || count >= playerCountUrls.Length) return null;

            return playerCountUrls[count];
        }

        public VRCUrl GetMetricUrl(Metric metric)
        {
            switch (metric)
            {
                case Metric.Respawn: return respawnUrl;
                case Metric.Heartbeat: return heartbeatUrl;
                case Metric.PlayerCount: return this.GetPlayerCountUrl(VRCPlayerApi.GetPlayerCount());
            }

            return null;
        }
    }
}
