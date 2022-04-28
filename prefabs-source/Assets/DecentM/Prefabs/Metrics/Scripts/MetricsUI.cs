
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

namespace DecentM.Metrics
{
    // This class only exists so there's something to attach the inspector to
    [UdonBehaviourSyncMode(BehaviourSyncMode.None)]
    public class MetricsUI : UdonSharpBehaviour
    {
        public string worldVersion = "";
        public int worldCapacity = 64;
        public int instanceCapacity = 64;
        public string metricsServerBaseUrl = "";
    }
}
