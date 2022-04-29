using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DecentM.EditorTools;
using DecentM.Metrics.Plugins;
using UnityEditor;

namespace DecentM.Metrics
{
    public class TriggerVolumePluginManager
    {
        public static List<string> CollectInteractionNames()
        {
            ComponentCollector<TriggerVolumePlugin> collector = new ComponentCollector<TriggerVolumePlugin>();
            List<TriggerVolumePlugin> plugins = collector.CollectFromActiveScene();

            List<string> result = new List<string>();

            foreach (TriggerVolumePlugin plugin in plugins)
            {
                if (plugin.metricName == null || plugin.metricName == "")
                {
                    Debug.LogWarning($"Metric name is not set for the trigger volume plugin on {plugin.name}, generating a random name...");
                    plugin.metricName = RandomStringGenerator.GenerateRandomString(8);
                }

                result.Add(plugin.metricName);
            }

            return result;
        }

        public static void RelinkRequirements()
        {
            ComponentCollector<TriggerVolumePlugin> collector = new ComponentCollector<TriggerVolumePlugin>();
            List<TriggerVolumePlugin> plugins = collector.CollectFromActiveScene();

            foreach (TriggerVolumePlugin plugin in plugins)
            {
                PluginRequirements requirements = PluginManager.GetFirstRequirements();
                plugin.system = requirements.system;
                plugin.events = requirements.events;
                plugin.urlStore = requirements.urlStore;
                plugin.pubsubHosts = new Pubsub.PubsubHost[] { requirements.events };
            }
        }
    }
}
