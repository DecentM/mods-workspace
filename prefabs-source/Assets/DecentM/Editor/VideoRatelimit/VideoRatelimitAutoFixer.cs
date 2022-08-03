using System.Collections.Generic;
using UnityEditor;

using DecentM.EditorTools;
using DecentM.Prefabs.VideoPlayer.Handlers;

namespace DecentM.Prefabs.VideoRatelimit.EditorTools
{
    public static class VideoRatelimitAutoFixer
    {
        [MenuItem("DecentM/VideoRatelimit/Run Autofixer")]
        public static void OnPerformFixes()
        {
            List<VideoRatelimitSystem> ratelimits =
                ComponentCollector<VideoRatelimitSystem>.CollectFromActiveScene();

            // Not being installed isn't an error, it just means the user doesn't want to rate limit video players (shrug)
            if (ratelimits.Count == 0)
                return;

            if (ratelimits.Count > 1)
            {
                EditorUtility.DisplayDialog("Autofixer error", $"{ratelimits.Count} VideoRatelimit systems detected, you must have only a single one. Please delete extra prefabs and try again.", "OK");
                return;
            }

            VideoRatelimitSystem system = ratelimits[0];

            #region Auto-attach to video players

            List<PlayerHandler> players =
                ComponentCollector<PlayerHandler>.CollectFromActiveScene();

            foreach (PlayerHandler player in players)
            {
                PlayerLoadMonitoring existing = player.gameObject.GetComponent<PlayerLoadMonitoring>();

                if (existing != null)
                    continue;

                PlayerLoadMonitoring monitoring =
                    player.gameObject.AddComponent<PlayerLoadMonitoring>();

                monitoring.system = system;

                Inspector.SaveModifications(player.gameObject);
                Inspector.SaveModifications(monitoring);
            }

            #endregion
        }
    }
}
