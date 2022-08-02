using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using DecentM.EditorTools;
using DecentM.Metrics.Plugins;
using VRC.SDK3.Video.Components.Base;

namespace DecentM.VideoRatelimit
{
    public class VideoRatelimitAutoFixer : AutoSceneFixer
    {
        protected override bool OnPerformFixes()
        {
            List<VideoRatelimitSystem> ratelimits =
                ComponentCollector<VideoRatelimitSystem>.CollectFromActiveScene();

            // Not being installed isn't an error, it just means the user doesn't want to rate limit video players (shrug)
            if (ratelimits.Count == 0)
                return true;

            if (ratelimits.Count > 1)
            {
                Debug.LogError(
                    $"{ratelimits.Count} VideoRatelimit systems detected, you must have only a single one. Please delete extra prefabs and try again."
                );
                return false;
            }

            VideoRatelimitSystem system = ratelimits[0];

            #region Auto-attach to video players

            List<BaseVRCVideoPlayer> players =
                ComponentCollector<BaseVRCVideoPlayer>.CollectFromActiveScene();

            foreach (BaseVRCVideoPlayer player in players)
            {
                PlayerLoadMonitoring monitoring =
                    player.gameObject.GetOrAddComponent<PlayerLoadMonitoring>();

                monitoring.system = system;

                Inspector.SaveModifications(player.gameObject);
                Inspector.SaveModifications(monitoring);
            }

            #endregion

            return true;
        }
    }
}
