using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DecentM.EditorTools;
using DecentM.Metrics.Plugins;

namespace DecentM.Metrics
{
    public static class UIManager
    {
        public static void SetWorldVersion(string worldVersion)
        {
            ComponentCollector<MetricsUI> collector = new ComponentCollector<MetricsUI>();
            List<MetricsUI> uis = collector.CollectFromActiveScene();

            foreach (MetricsUI ui in uis)
            {
                ui.worldVersion = worldVersion;
            }
        }
    }
}
