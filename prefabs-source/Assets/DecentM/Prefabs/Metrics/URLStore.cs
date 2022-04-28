
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;
using DecentM.Metrics.Plugins;

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

        private VRCUrl GetMetricUrl(Metric metric, object[][] parameters)
        {
            // Search through all the URLs for the one that matches
            // all parameters
            foreach (object[] item in this.urls)
            {
                object[] data = (object[])item[0];
                VRCUrl url = (VRCUrl)item[1];
                if (url == null || data == null) continue;

                Metric itemMetric = (Metric)data[0];
                object[][] itemParameters = (object[][])data[1];

                // No need to scan the parameters if there aren't any, and we have a URL for the current metric
                if (itemMetric == metric && itemParameters.Length == 0 && parameters.Length == 0) return url;
                if (itemMetric != metric || parameters.Length != itemParameters.Length) continue;

                int matches = 0;

                for (int i = 0; i < itemParameters.Length; i++)
                {
                    if (itemParameters[i] == null || parameters[i] == null) break;

                    string itemName = (string)itemParameters[i][0];
                    string itemValue = (string)itemParameters[i][1];

                    string name = (string)parameters[i][0];
                    string value = (string)parameters[i][1];

                    // Break out of the loop as soon as we see a single variable that doesn't match for performance
                    if (name != itemName || value != itemValue) break;

                    matches++;
                }

                if (matches >= itemParameters.Length) return url;
            }

            return null;
        }

        private VRCUrl GetMetricUrl(Metric metric)
        {
            // Metrics with no parameters are stripped out from the list of URLs in the editor, so
            // they exist in here as metrics with an empty parameter. Therefore, metric URLs with no parameter
            // actually have an empty parameter and so we need to insert that to be able to find it
            return this.GetMetricUrl(metric, new object[][]
                {
                    new object[] { "", "" }
                }
            );
        }

        public VRCUrl GetHeartbeatUrl(bool isMaster, bool isVr, bool isFbt, int timezone, VrPlatform vrPlatform)
        {
            return this.GetMetricUrl(Metric.Heartbeat, new object[][]
                {
                    new object[] { "isMaster", isMaster.ToString() },
                    new object[] { "isVr", isVr.ToString() },
                    new object[] { "isFbt", isFbt.ToString() },
                    new object[] { "timezone", timezone.ToString() },
                    new object[] { "vrPlatform", vrPlatform },
                }
            );
        }

        public VRCUrl GetPlayerCountUrl(int playerCount)
        {
            return this.GetMetricUrl(Metric.PlayerCount, new object[][]
                {
                    new object[] { "playerCount", playerCount.ToString() },
                }
            );
        }

        public VRCUrl GetRespawnUrl()
        {
            return this.GetMetricUrl(Metric.Respawn);
        }
    }
}
