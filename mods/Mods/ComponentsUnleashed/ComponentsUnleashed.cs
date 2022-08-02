using System;
using System.Collections.Generic;

using MelonLoader;
using HarmonyLib;

using ABI_RC.Core;

using DecentM.Prefabs.VideoPlayer;
using DecentM.Prefabs.VideoPlayer.Handlers;

namespace DecentM.Mods.ComponentsUnleashed
{
    public class Mod : MelonMod
    {
        public override void OnApplicationLateStart()
        {
            LoggerInstance.Msg("Extending component whitelist...");
            HashSet<Type> whiteList = (HashSet<Type>)Traverse.Create<CVRTools>().Field("componentWhiteList").GetValue();

            /*
             * PubSub
             */
            whiteList.Add(typeof(Pubsub.PubsubHost));
            whiteList.Add(typeof(Pubsub.PubsubSubscriber));

            /*
             * PerformanceGovernor
             */
            whiteList.Add(typeof(Prefabs.Performance.PerformanceGovernor));

            /*
             * UI
             */
            whiteList.Add(typeof(Prefabs.UI.Dropdown));
            whiteList.Add(typeof(Prefabs.UI.DropdownOption));

            /*
             * VideoPlayer
             */
            whiteList.Add(typeof(UnityEngine.Video.VideoPlayer));
            
            // Core
            whiteList.Add(typeof(PlayerHandler));
            whiteList.Add(typeof(ScreenHandler));
            whiteList.Add(typeof(VideoPlayerEvents));
            whiteList.Add(typeof(VideoPlayerSystem));
            whiteList.Add(typeof(VideoPlayerUI));

            // Plugins

            Traverse.Create<CVRTools>().Field("componentWhiteList").SetValue(whiteList);
        }
    }
}
