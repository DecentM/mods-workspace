using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DecentM.EditorTools;
using DecentM.Metrics.Plugins;
using UdonSharp;
using UnityEditor.SceneManagement;

namespace DecentM.Metrics
{
    public static class UIManager
    {
        public static void SetVersionData()
        {
            ComponentCollector<MetricsUI> collector = new ComponentCollector<MetricsUI>();
            List<MetricsUI> uis = collector.CollectFromActiveScene();

            foreach (MetricsUI ui in uis)
            {
                ui.builtAt = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds().ToString();
                ui.sdk = VRC.Tools.ClientVersion;
                ui.unity = VRC.Tools.UnityVersion.ToString();
                ui.sceneName = EditorSceneManager.GetActiveScene().name;
            }
        }
    }
}
