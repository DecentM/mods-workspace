using UdonSharp;
using UnityEngine;

using UNet;

namespace DecentM
{
    [UdonBehaviourSyncMode(BehaviourSyncMode.None)]
    public class LibDecentM : UdonSharpBehaviour
    {
        [Header("References")]
        public Permissions permissions;
        public Debugging debugging;
        public Scheduling scheduling;
        public Tools.ArrayTools arrayTools;
        public PerformanceGovernor performanceGovernor;
        public UdonHashLib hash;
        public NetworkInterface net;
    }
}
