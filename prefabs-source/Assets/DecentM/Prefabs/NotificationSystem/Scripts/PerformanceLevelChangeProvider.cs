
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;
using UnityEngine.UI;
using DecentM.Pubsub;

namespace DecentM.Notifications.Providers
{
    public class PerformanceLevelChangeProvider : PubsubSubscriber<PerformanceGovernorEvent>
    {
        public NotificationSystem notifications;
        public Toggle toggle;

        public Sprite iconHigh;
        public Sprite iconMed;
        public Sprite iconLow;

        protected override void OnPubsubEvent(object name, object[] data)
        {
            switch (name)
            {
                case PerformanceGovernorEvent.OnPerformanceLow:
                    this.OnPerformanceLow();
                    return;

                case PerformanceGovernorEvent.OnPerformanceMedium:
                    this.OnPerformanceMedium();
                    return;

                case PerformanceGovernorEvent.OnPerformanceHigh:
                    this.OnPerformanceHigh();
                    return;
            }
        }

        private void OnPerformanceHigh()
        {
            if (!this.toggle.isOn) return;
            this.notifications.SendNotification(this.iconHigh, "Performance mode changed to High");
        }

        private void OnPerformanceMedium()
        {
            if (!this.toggle.isOn) return;
            this.notifications.SendNotification(this.iconMed, "Performance mode changed to Medium");
        }

        private void OnPerformanceLow()
        {
            if (!this.toggle.isOn) return;
            this.notifications.SendNotification(this.iconLow, "Performance mode changed to Low");
        }
    }
}
