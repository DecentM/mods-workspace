using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DecentM.EditorTools;
using DecentM.Metrics.Plugins;

namespace DecentM.Metrics
{
    public struct PluginRequirements
    {
        public PluginRequirements(MetricsEvents events, MetricsSystem system, URLStore urlStore)
        {
            this.events = events;
            this.system = system;
            this.urlStore = urlStore;
        }

        public MetricsEvents events;
        public MetricsSystem system;
        public URLStore urlStore;
    }

    public static class PluginManager
    {
        public static List<PluginRequirements> GetAllRequirements()
        {
            ComponentCollector<MetricsUI> collector = new ComponentCollector<MetricsUI>();
            List<MetricsUI> uis = collector.CollectFromActiveScene();

            List<PluginRequirements> result = new List<PluginRequirements>();

            foreach (MetricsUI ui in uis)
            {
                PluginRequirements requirements = new PluginRequirements();

                requirements.system = ui.GetComponentInChildren<MetricsSystem>();
                requirements.urlStore = ui.GetComponentInChildren<URLStore>();
                requirements.events = ui.GetComponentInChildren<MetricsEvents>();

                result.Add(requirements);
            }

            return result;
        }

        public static PluginRequirements GetFirstRequirements()
        {
            return GetAllRequirements()[0];
        }
    }
}
