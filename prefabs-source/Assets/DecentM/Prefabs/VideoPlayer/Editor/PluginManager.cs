using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DecentM.EditorTools;
using DecentM.Metrics.Plugins;

namespace DecentM.VideoPlayer
{
    public struct PluginRequirements
    {
        public PluginRequirements(VideoPlayerEvents events, VideoPlayerSystem system)
        {
            this.events = events;
            this.system = system;
        }

        public VideoPlayerEvents events;
        public VideoPlayerSystem system;
    }

    public static class PluginManager
    {
        public static PluginRequirements GetRequirements(VideoPlayerUI ui)
        {
            PluginRequirements requirements = new PluginRequirements();

            requirements.system = ui.GetComponentInChildren<VideoPlayerSystem>();
            requirements.events = ui.GetComponentInChildren<VideoPlayerEvents>();

            return requirements;
        }
    }
}
