using UnityEditor;
using UdonSharp;
using UdonSharpEditor;
using DecentM.EditorTools;

namespace DecentM.Metrics
{
    [CustomEditor(typeof(MetricsUI))]
    public class MetricsUIInspector : Inspector
    {
        public override void OnInspectorGUI()
        {
            MetricsUI ui = (MetricsUI)target;
            URLStore urlStore = ui.GetComponentInChildren<URLStore>();

            ui.metricsServerBaseUrl = EditorGUILayout.TextField("Metrics server base URL:", ui.metricsServerBaseUrl);
            ui.worldCapacity = EditorGUILayout.IntField("World capacity", ui.worldCapacity);
            ui.instanceCapacity = EditorGUILayout.IntField("Instance capacity", ui.instanceCapacity);

            EditorGUI.BeginDisabledGroup(true);
            if (urlStore != null) EditorGUILayout.TextField("Currently stored URLs", urlStore.urls == null ? "<no URLs stored>" : urlStore.urls.Length.ToString());;
            EditorGUILayout.Space();
            EditorGUILayout.TextField("Built at", ui.builtAt);
            EditorGUILayout.TextField("Unity version", ui.unity);
            EditorGUILayout.TextField("SDK Version", ui.sdk);
            EditorGUI.EndDisabledGroup();

            if (urlStore != null && this.Button("Clear URLs"))
            {
                MetricsUrlGenerator.ClearUrls(urlStore);
            }

            if (urlStore != null && this.Button("Bake URLs"))
            {
                MetricsUrlGenerator.SaveUrls(ui, urlStore);
            }

            this.SaveModifications();
        }
    }
}
