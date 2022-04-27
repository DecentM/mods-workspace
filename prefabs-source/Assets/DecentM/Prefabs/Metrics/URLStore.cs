
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

namespace DecentM.Metrics
{
    [UdonBehaviourSyncMode(BehaviourSyncMode.None)]
    public class URLStore : UdonSharpBehaviour
    {
        // item structure:
        // 0 - object[] { Metric metric, string value }
        // 1 - VRCUrl url
        public object[][] urls;

        public VRCUrl[] debugUrls;

        public VRCUrl GetPlayerCountUrl(int count)
        {
            return null;
        }

        public VRCUrl GetMetricUrl(Metric metric)
        {
            return null;
        }
    }
}
