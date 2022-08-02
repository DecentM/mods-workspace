using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DecentM.EditorTools;
using DecentM.Metrics.Plugins;
using UnityEditor;

namespace DecentM.Metrics
{
    public class IndividualTrackingPluginManager<PluginType>
        where PluginType : IndividualTrackingPlugin
    {
        public static List<string> CollectInteractionNames()
        {
            List<PluginType> plugins = ComponentCollector<PluginType>.CollectFromActiveScene();

            List<string> result = new List<string>();

            foreach (PluginType plugin in plugins)
            {
                if (plugin.metricName == null || plugin.metricName == "")
                {
                    Debug.LogWarning(
                        $"Metric name is not set for the metrics plugin on {plugin.name}, generating a random name..."
                    );
                    plugin.metricName = RandomStringGenerator.GenerateRandomString(8);
                    Inspector.SaveModifications(plugin);
                }

                result.Add(plugin.metricName);
            }

            return result;
        }
    }
}
