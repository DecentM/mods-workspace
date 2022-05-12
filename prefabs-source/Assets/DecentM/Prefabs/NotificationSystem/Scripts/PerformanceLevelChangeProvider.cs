using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;
using UnityEngine.UI;
using DecentM.Pubsub;
using DecentM.Performance.Plugins;

namespace DecentM.Notifications.Providers
{
    public class PerformanceLevelChangeProvider : PerformanceGovernorPlugin
    {
        public NotificationSystem notifications;
        public Toggle toggle;

        public Sprite iconHigh;
        public Sprite iconMed;
        public Sprite iconLow;

        protected override void OnPerformanceModeChange(PerformanceGovernorMode mode, float fps)
        {
            switch (mode)
            {
                case PerformanceGovernorMode.Low:
                    this.OnPerformanceLow();
                    return;

                case PerformanceGovernorMode.Medium:
                    this.OnPerformanceMedium();
                    return;

                case PerformanceGovernorMode.High:
                    this.OnPerformanceHigh();
                    return;
            }
        }

        private void OnPerformanceHigh()
        {
            if (!this.toggle.isOn)
                return;
            this.notifications.SendNotification(this.iconHigh, "Performance mode changed to High");
        }

        private void OnPerformanceMedium()
        {
            if (!this.toggle.isOn)
                return;
            this.notifications.SendNotification(this.iconMed, "Performance mode changed to Medium");
        }

        private void OnPerformanceLow()
        {
            if (!this.toggle.isOn)
                return;
            this.notifications.SendNotification(this.iconLow, "Performance mode changed to Low");
        }
    }
}
