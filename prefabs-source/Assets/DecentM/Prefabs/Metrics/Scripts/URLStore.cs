#if UNITY_EDITOR && !COMPILER_UDONSHARP
using UnityEditor;
#endif
using System;
using UnityEngine;
using UdonSharp;
using VRC.SDKBase;

namespace DecentM.Metrics
{
    [UdonBehaviourSyncMode(BehaviourSyncMode.None)]
    public class URLStore : UdonSharpBehaviour, ISerializationCallbackReceiver
    {
        // parameter structure
        // 0 - name
        // 1 - value

        // item structure:
        // 0 - object[] { Metric metric, object[][] {} parameters }
        // 1 - VRCUrl url
        public object[][] urls;

        public void OnBeforeSerialize()
        {
            if (
                this.serializedUrls != null
                && this.serializedUrlData != null
                && this.urls.Length == this.serializedUrls.Length
                && this.urls.Length == this.serializedUrlData.Length
            )
                return;

            this.serializedUrlData = new string[this.urls.Length];
            this.serializedUrls = new VRCUrl[this.urls.Length];

            for (int i = 0; i < this.urls.Length; i++)
            {
#if UNITY_EDITOR && !COMPILER_UDONSHARP
                EditorUtility.DisplayProgressBar($"[DecentM.Metrics] Serialising URL combinations...", $"{i}/{this.urls.Length}", (float)i / this.urls.Length);
#endif

                object[] item = this.urls[i];

                if (item == null)
                    continue;

                object[] data = (object[])item[0];
                VRCUrl url = (VRCUrl)item[1];

                this.serializedUrls[i] = url;
                this.serializedUrlData[i] = $"{(int)data[0]}";

                foreach (object[] parameter in (object[][])data[1])
                {
                    if (parameter == null)
                        continue;

                    string paramName = (string)parameter[0];
                    string paramValue = (string)parameter[1];

                    this.serializedUrlData[i] += $";{paramName}={paramValue}";
                }
            }

#if UNITY_EDITOR && !COMPILER_UDONSHARP
            EditorUtility.ClearProgressBar();
#endif
        }

        public void OnAfterDeserialize()
        {
            if (this.urls != null && this.serializedUrls.Length == this.urls.Length)
                return;

            this.urls = new object[this.serializedUrls.Length][];

            for (var i = 0; i < this.serializedUrls.Length; i++)
            {
                string data = this.serializedUrlData[i];

                if (data == null)
                    continue;

                string[] dataParts = data.Split(';');
                int metric;
                bool metricParsed = int.TryParse(dataParts[0], out metric);

                if (!metricParsed)
                    continue;

                object[][] parameters = new object[dataParts.Length - 1][];

                for (int j = 1; j < dataParts.Length; j++)
                {
                    string[] parts = dataParts[j].Split('=');

                    if (parts.Length == 2)
                        parameters[j - 1] = new object[] { parts[0], parts[1] };
                    else
                        parameters[j - 1] = new object[] { "", "" };
                }

                object[] item = new object[]
                {
                    new object[] { metric, parameters },
                    this.serializedUrls[i]
                };

                this.urls[i] = item;
            }
        }

        //[HideInInspector]
        public VRCUrl[] serializedUrls;

        //[HideInInspector]
        public string[] serializedUrlData;

        private VRCUrl GetMetricUrl(Metric metric, object[][] parameters)
        {
            // Search through all the URLs for the one that matches
            // all parameters
            foreach (object[] item in this.urls)
            {
                object[] data = (object[])item[0];
                VRCUrl url = (VRCUrl)item[1];

                if (url == null || data == null)
                    continue;

                Metric itemMetric = (Metric)data[0];
                object[][] itemParameters = (object[][])data[1];

                // No need to scan the parameters if there aren't any, and we have a URL for the current metric
                if (itemMetric == metric && itemParameters.Length == 0 && parameters.Length == 0)
                    return url;

                if (itemMetric != metric || parameters.Length != itemParameters.Length)
                    continue;

                int matches = 0;

                for (int i = 0; i < itemParameters.Length; i++)
                {
                    if (itemParameters[i] == null || parameters[i] == null)
                        continue;

                    string itemName = (string)itemParameters[i][0];
                    string itemValue = (string)itemParameters[i][1];

                    string name = (string)parameters[i][0];
                    string value = (string)parameters[i][1];

                    // Break out of the loop as soon as we see a single variable that doesn't match for performance
                    if (name != itemName || value != itemValue)
                        break;

                    matches++;
                }

                if (matches >= itemParameters.Length)
                    return url;
            }

            return null;
        }

        private VRCUrl GetMetricUrl(Metric metric)
        {
            // Metrics with no parameters are stripped out from the list of URLs in the editor, so
            // they exist in here as metrics with an empty parameter. Therefore, metric URLs with no parameter
            // actually have an empty parameter and so we need to insert that to be able to find it
            return this.GetMetricUrl(metric, new object[][] { new object[] { "", "" } });
        }

        public VRCUrl GetHeartbeatUrl(bool isMaster, bool isVr, int fps)
        {
            return this.GetMetricUrl(
                Metric.Heartbeat,
                new object[][]
                {
                    new object[] { "isMaster", isMaster.ToString() },
                    new object[] { "isVr", isVr.ToString() },
                    new object[] { "fps", fps.ToString() },
                }
            );
        }

        public VRCUrl GetInstanceUrl(string instanceId, int playerCount)
        {
            return this.GetMetricUrl(
                Metric.Instance,
                new object[][]
                {
                    new object[] { "instanceId", instanceId },
                    new object[] { "playerCount", playerCount.ToString() },
                }
            );
        }

        public VRCUrl GetRespawnUrl()
        {
            return this.GetMetricUrl(Metric.Respawn);
        }

        public VRCUrl GetInteractionUrl(string objectName)
        {
            return this.GetMetricUrl(
                Metric.Interaction,
                new object[][] { new object[] { "objectName", objectName }, }
            );
        }

        public VRCUrl GetTriggerUrl(string objectName, bool state)
        {
            return this.GetMetricUrl(
                Metric.Trigger,
                new object[][]
                {
                    new object[] { "objectName", objectName },
                    new object[] { "state", state.ToString() },
                }
            );
        }

        public VRCUrl GetStationUrl(string objectName, bool state)
        {
            return this.GetMetricUrl(
                Metric.Station,
                new object[][]
                {
                    new object[] { "objectName", objectName },
                    new object[] { "state", state.ToString() },
                }
            );
        }

        public VRCUrl GetPickupUrl(string objectName, bool state)
        {
            return this.GetMetricUrl(
                Metric.Pickup,
                new object[][]
                {
                    new object[] { "objectName", objectName },
                    new object[] { "state", state.ToString() },
                }
            );
        }

        public VRCUrl GetCustomUrl(string metricName)
        {
            return this.GetMetricUrl(
                Metric.Custom,
                new object[][] { new object[] { "metricName", metricName }, }
            );
        }

        public VRCUrl GetVideoPlayerUrl(string playerName, string eventName)
        {
            return this.GetMetricUrl(
                Metric.VideoPlayer,
                new object[][]
                {
                    new object[] { "playerName", playerName },
                    new object[] { "eventName", eventName }
                }
            );
        }

        public VRCUrl GetPlayerListUrl(string listName, string eventName, string player)
        {
            return this.GetMetricUrl(
                Metric.PlayerList,
                new object[][]
                {
                    new object[] { "listName", listName },
                    new object[] { "eventName", eventName },
                    new object[] { "player", player }
                }
            );
        }
    }
}
