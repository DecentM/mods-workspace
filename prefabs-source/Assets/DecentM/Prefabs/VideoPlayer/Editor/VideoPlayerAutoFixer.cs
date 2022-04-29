using System.Collections;
using System.Collections.Generic;

using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;

using DecentM.EditorTools;
using DecentM.VideoPlayer.Plugins;

namespace DecentM.VideoPlayer
{
    public class VideoPlayerAutoFixer : AutoSceneFixer
    {
        protected override bool OnPerformFixes()
        {
            ComponentCollector<VideoPlayerUI> collector = new ComponentCollector<VideoPlayerUI>();
            List<VideoPlayerUI> players = collector.CollectFromActiveScene();

            foreach (VideoPlayerUI player in players)
            {
                if (PrefabUtility.IsPartOfAnyPrefab(player)) PrefabUtility.UnpackPrefabInstance(player.gameObject, PrefabUnpackMode.OutermostRoot, InteractionMode.AutomatedAction);

                VideoPlayerPlugin[] plugins = player.GetComponentsInChildren<VideoPlayerPlugin>();
                PluginRequirements requirements = PluginManager.GetRequirements(player);

                foreach (VideoPlayerPlugin plugin in plugins)
                {
                    if (PrefabUtility.IsPartOfAnyPrefab(plugin)) PrefabUtility.UnpackPrefabInstance(plugin.gameObject, PrefabUnpackMode.OutermostRoot, InteractionMode.AutomatedAction);

                    plugin.events = requirements.events;
                    plugin.system = requirements.system;
                    plugin.pubsubHosts = new Pubsub.PubsubHost[] { requirements.events };
                }
            }

            return true;
        }
    }
}
