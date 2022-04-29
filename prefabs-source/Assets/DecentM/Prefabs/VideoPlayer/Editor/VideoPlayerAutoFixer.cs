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
                VideoPlayerPlugin[] plugins = player.GetComponentsInChildren<VideoPlayerPlugin>();
                PluginRequirements requirements = PluginManager.GetRequirements(player);

                foreach (VideoPlayerPlugin plugin in plugins)
                {
                    plugin.events = requirements.events;
                    plugin.system = requirements.system;
                    plugin.pubsubHosts = new Pubsub.PubsubHost[] { requirements.events };

                    DEditor.SavePrefabModifications(plugin);
                }
            }

            return true;
        }
    }
}
