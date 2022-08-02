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
        public static PluginRequirements GetRequirements(MetricsUI ui)
        {
            PluginRequirements requirements = new PluginRequirements();

            requirements.system = ui.GetComponentInChildren<MetricsSystem>();
            requirements.urlStore = ui.GetComponentInChildren<URLStore>();
            requirements.events = ui.GetComponentInChildren<MetricsEvents>();

            return requirements;
        }
    }
}
