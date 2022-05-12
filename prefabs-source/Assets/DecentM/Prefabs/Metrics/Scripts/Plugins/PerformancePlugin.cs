using System;
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;
using DecentM.Performance.Plugins;

namespace DecentM.Metrics.Plugins
{
    public class PerformancePlugin : PerformanceGovernorPlugin
    {
        public MetricsSystem system;
        public URLStore urlStore;

        protected override void OnPerformanceModeChange(PerformanceGovernorMode mode, float fps)
        {
            int roundFps = Mathf.FloorToInt(fps);
            VRCUrl url = this.urlStore.GetPerformanceUrl(mode, roundFps);
            if (url == null)
                return;

            this.system.RecordMetric(url, Metric.PerformanceModeChange);
        }
    }
}
