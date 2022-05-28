using System.Collections.Generic;

using DecentM.EditorTools;
using DecentM.VideoPlayer.Plugins;
using DecentM.VideoRatelimit;

namespace DecentM.VideoPlayer
{
    public class VideoPlayerAutoFixer : AutoSceneFixer
    {
        protected override bool OnPerformFixes()
        {
            this.FixRatelimits();

            List<VideoPlayerUI> players =
                ComponentCollector<VideoPlayerUI>.CollectFromActiveScene();

            foreach (VideoPlayerUI player in players)
            {
                VideoPlayerPlugin[] plugins = player.GetComponentsInChildren<VideoPlayerPlugin>();
                PluginRequirements requirements = PluginManager.GetRequirements(player);

                foreach (VideoPlayerPlugin plugin in plugins)
                {
                    plugin.events = requirements.events;
                    plugin.system = requirements.system;
                    plugin.pubsubHosts = new Pubsub.PubsubHost[] { requirements.events };

                    Inspector.SaveModifications(plugin);
                }
            }

            return true;
        }

        private void FixRatelimits()
        {
            List<VideoRatelimitSystem> ratelimits =
                ComponentCollector<VideoRatelimitSystem>.CollectFromActiveScene();

            if (ratelimits.Count <= 0)
                return;

            VideoRatelimitSystem ratelimit = ratelimits[0];

            if (ratelimit == null)
                return;

            List<LoadRequestHandlerPlugin> loadHandlers =
                ComponentCollector<LoadRequestHandlerPlugin>.CollectFromActiveScene();

            foreach (LoadRequestHandlerPlugin loadHandler in loadHandlers)
            {
                loadHandler.ratelimit = ratelimit;

                Inspector.SaveModifications(loadHandler);
            }
        }
    }
}
