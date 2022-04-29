using System;
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

namespace DecentM.Metrics.Plugins
{
    public class TriggerVolumePlugin : MetricsPlugin
    {
        public string metricName = "";

        private bool locked = true;
        private bool reportedState = false;

        private void DoReport(bool state)
        {
            VRCUrl url = this.urlStore.GetTriggerUrl(this.metricName, state);
            if (url == null)
            {
                Debug.Log("URL missing");
                return;
            }

            this.system.RecordMetric(url, Metric.Trigger);
            this.reportedState = state;

            // Only set the lock after exiting the trigger, as we'd rather miss reporting a second entry than miss
            // reporting the exit due to the lock
            if (!state)
            {
                this.locked = true;
                this.SendCustomEventDelayedSeconds(nameof(Unlock), 5.2f);
            }
        }

        protected override void OnMetricsSystemInit()
        {
            this.Unlock();
        }

        public void Unlock()
        {
            this.locked = false;
        }

        public override void OnPlayerTriggerEnter(VRCPlayerApi player)
        {
            if (player == null || !player.IsValid() || player != Networking.LocalPlayer) return;
            if (this.locked || this.reportedState) return;

            this.DoReport(true);
        }

        public override void OnPlayerTriggerExit(VRCPlayerApi player)
        {
            if (player == null || !player.IsValid() || player != Networking.LocalPlayer) return;
            if (this.locked || !this.reportedState) return;

            this.DoReport(false);
        }
    }
}
