using JetBrains.Annotations;
using VRC.SDKBase;

namespace DecentM.Metrics.Plugins
{
    public class CustomEventTrackingPlugin : IndividualTrackingPlugin
    {
        public float backoffSeconds = 5.2f;

        private bool locked = false;

        private void DoReport()
        {
            VRCUrl url = this.urlStore.GetCustomUrl(this.metricName);
            if (url == null)
                return;

            this.system.RecordMetric(url, Metric.Custom);

            this.locked = true;
            this.SendCustomEventDelayedSeconds(nameof(Unlock), this.backoffSeconds);
        }

        public void Unlock()
        {
            this.locked = false;
        }

        [PublicAPI]
        public bool TrackEvent()
        {
            if (this.locked)
                return false;

            this.DoReport();
            return true;
        }
    }
}
