using System;
using System.Collections.Generic;

using UnityEditor;

using VRC.SDKBase;

using DecentM.EditorTools;
using DecentM.Metrics.Plugins;

namespace DecentM.Metrics
{
    [CustomEditor(typeof(MetricsUI))]
    public class MetricsUIInspector : DEditor
    {
        MetricsUI ui;
        URLStore urlStore;

        public override void OnInspectorGUI()
        {
            this.ui = (MetricsUI)target;
            this.urlStore = this.ui.GetComponentInChildren<URLStore>();

            this.ui.metricsServerBaseUrl = EditorGUILayout.TextField("Metrics server base URL:", this.ui.metricsServerBaseUrl);
            this.ui.worldCapacity = EditorGUILayout.IntField("World capacity", this.ui.worldCapacity);
            this.ui.instanceCapacity = EditorGUILayout.IntField("Instance capacity", this.ui.instanceCapacity);

            if (this.urlStore != null && this.Button("Save"))
            {
                MetricsUrlGenerator.SaveUrls(this.ui, this.urlStore);
            }
        }
    }
}
