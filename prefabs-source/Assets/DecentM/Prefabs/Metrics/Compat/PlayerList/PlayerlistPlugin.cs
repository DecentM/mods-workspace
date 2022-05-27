using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;
using DecentM;

namespace DecentM.Metrics.Plugins
{
    public enum PlayerlistMetric
    {
        Joined,
    }

    public enum ListBehaviour
    {
        Whitelist,
        Blacklist,
    }

    public class PlayerlistPlugin : IndividualTrackingPlugin
    {
        public PlayerList list;

        [Tooltip(
            "In whitelist mode, players who are on the list will send this metric. In blacklsit mode, players who aren't on the list will send the metric. The player's name will not be sent when in blacklist mode."
        )]
        public ListBehaviour listBehaviour = ListBehaviour.Whitelist;

        protected override void OnMetricsSystemInit()
        {
            if (Networking.LocalPlayer == null || !Networking.LocalPlayer.IsValid())
                return;

            switch (this.listBehaviour)
            {
                case ListBehaviour.Whitelist:
                    this.RunWhitelistMode();
                    return;

                case ListBehaviour.Blacklist:
                    this.RunBlacklistMode();
                    return;
            }
        }

        private void RunWhitelistMode()
        {
            if (!this.list.CheckPlayer(Networking.LocalPlayer))
                return;

            VRCUrl url = this.urlStore.GetPlayerListUrl(
                this.metricName,
                nameof(PlayerlistMetric.Joined),
                Networking.LocalPlayer.displayName
            );

            this.system.RecordMetric(url, Metric.PlayerList);
        }

        private void RunBlacklistMode()
        {
            if (this.list.CheckPlayer(Networking.LocalPlayer))
                return;

            VRCUrl url = this.urlStore.GetPlayerListUrl(
                this.metricName,
                nameof(PlayerlistMetric.Joined),
                ""
            );

            this.system.RecordMetric(url, Metric.PlayerList);
        }
    }
}
