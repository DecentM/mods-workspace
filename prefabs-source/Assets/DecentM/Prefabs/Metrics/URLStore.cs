
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

namespace DecentM.Metrics
{
    [UdonBehaviourSyncMode(BehaviourSyncMode.None)]
    public class URLStore : UdonSharpBehaviour
    {
        // parameter structure
        // 0 - name
        // 1 - value

        // item structure:
        // 0 - object[] { Metric metric, object[][] {} parameters }
        // 1 - VRCUrl url
        public object[][] urls;

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
