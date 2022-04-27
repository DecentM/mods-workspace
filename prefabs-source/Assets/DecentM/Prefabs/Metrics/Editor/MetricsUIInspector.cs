using System;
using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using UdonSharp;
using DecentM.EditorTools;
using VRC.SDKBase;

namespace DecentM.Metrics
{
    [CustomEditor(typeof(MetricsUI))]
    public class MetricsUIInspector : DEditor
    {
        MetricsUI ui;
        URLStore urlStore;

        private string metricsServerBaseUrl = "http://localhost:3000";
        private int worldCapacity = 64;

        public override void OnInspectorGUI()
        {
            this.ui = (MetricsUI)target;
            this.urlStore = this.ui.GetComponentInChildren<URLStore>();

            this.metricsServerBaseUrl = EditorGUILayout.TextField("Metrics server base URL:", this.metricsServerBaseUrl);
            this.worldCapacity = EditorGUILayout.IntField("World capacity", this.worldCapacity);

            if (this.urlStore != null && this.Button("Save"))
            {
                this.SaveUrls();
            }
        }

        private VRCUrl MakeUrl(string metricName, string metricData)
        {
            return new VRCUrl($"{this.metricsServerBaseUrl}/api/v1/metrics/ingest/{metricName}/{metricData}");
        }

        private VRCUrl MakeUrl(string metricName)
        {
            return this.MakeUrl(metricName, "");
        }

        private void SaveUrls()
        {
            this.urlStore.respawnUrl = this.MakeUrl("respawn");
            this.urlStore.heartbeatUrl = this.MakeUrl("heartbeat");

            VRCUrl[] playerCountUrls = new VRCUrl[this.worldCapacity + 1];

            for (int i = 0; i < playerCountUrls.Length; i++)
            {
                playerCountUrls[i] = this.MakeUrl("player-count", i.ToString());
            }

            this.urlStore.playerCountUrls = playerCountUrls;
        }
    }
}
