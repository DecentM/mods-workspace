using UnityEditor;
using DecentM.EditorTools;

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

            EditorGUI.BeginDisabledGroup(true);
            EditorGUILayout.TextField("Built at", this.ui.builtAt);
            EditorGUILayout.TextField("Unity version", this.ui.unity);
            EditorGUILayout.TextField("SDK Version", this.ui.sdk);
            EditorGUI.EndDisabledGroup();

            if (this.urlStore != null && this.Button("Clear URLs"))
            {
                MetricsUrlGenerator.ClearUrls(this.urlStore);
            }

            if (this.urlStore != null && this.Button("Bake URLs"))
            {
                MetricsUrlGenerator.SaveUrls(this.ui, this.urlStore);
            }
        }
    }
}
