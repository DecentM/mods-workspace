using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DecentM.EditorTools;
using DecentM.Metrics.Plugins;

namespace DecentM.Metrics
{
    public static class InstancePluginManager
    {
        public static void SetInstanceIds(List<string> instanceIds)
        {
            ComponentCollector<InstancePlugin> instancePluginCollector = new ComponentCollector<InstancePlugin>();
            List<InstancePlugin> instancePlugins = instancePluginCollector.CollectFromActiveScene();

            foreach (InstancePlugin instancePlugin in instancePlugins)
            {
                instancePlugin.instanceIds = instanceIds.ToArray();
            }
        }
    }
}
