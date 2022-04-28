
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

namespace DecentM.Metrics.Plugins
{
    public class RespawnPlugin : MetricsPlugin
    {
        public override void OnPlayerRespawn(VRCPlayerApi player)
        {
            VRCUrl url = this.urlStore.GetRespawnUrl();
            if (url == null) return;

            this.system.RecordMetric(url, Metric.Respawn);
        }
    }
}
