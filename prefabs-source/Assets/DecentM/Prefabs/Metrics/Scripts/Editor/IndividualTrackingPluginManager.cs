using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DecentM.EditorTools;
using DecentM.Metrics.Plugins;
using UnityEditor;

namespace DecentM.Metrics
{
    public class IndividualTrackingPluginManager<PluginType> where PluginType : IndividualTrackingPlugin
    {
        public static List<string> CollectInteractionNames()
        {
            ComponentCollector<PluginType> collector = new ComponentCollector<PluginType>();
            List<PluginType> plugins = collector.CollectFromActiveScene();

            List<string> result = new List<string>();

            foreach (PluginType plugin in plugins)
            {
                if (PrefabUtility.IsPartOfAnyPrefab(plugin)) PrefabUtility.UnpackPrefabInstance(plugin.gameObject, PrefabUnpackMode.OutermostRoot, InteractionMode.AutomatedAction);

                if (plugin.metricName == null || plugin.metricName == "")
                {
                    Debug.LogWarning($"Metric name is not set for the metrics plugin on {plugin.name}, generating a random name...");
                    plugin.metricName = RandomStringGenerator.GenerateRandomString(8);
                }

                result.Add(plugin.metricName);
            }

            return result;
        }
    }

    public class IndividualTrackingPluginTroubleshooter
    {
        public static void RelinkRequirements()
        {
            ComponentCollector<IndividualTrackingPlugin> collector = new ComponentCollector<IndividualTrackingPlugin>();
            List<IndividualTrackingPlugin> plugins = collector.CollectFromActiveScene();

            foreach (IndividualTrackingPlugin plugin in plugins)
            {
                if (PrefabUtility.IsPartOfAnyPrefab(plugin)) PrefabUtility.UnpackPrefabInstance(plugin.gameObject, PrefabUnpackMode.OutermostRoot, InteractionMode.AutomatedAction);

                PluginRequirements requirements = PluginManager.GetFirstRequirements();
                plugin.system = requirements.system;
                plugin.events = requirements.events;
                plugin.urlStore = requirements.urlStore;
                plugin.pubsubHosts = new Pubsub.PubsubHost[] { requirements.events };
            }
        }
    }
}
