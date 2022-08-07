using System;
using System.Collections.Generic;

using MelonLoader;
using HarmonyLib;

using ABI_RC.Core;

using DecentM.Prefabs;
using DecentM.Prefabs.Pubsub;
using DecentM.Prefabs.Performance;
using DecentM.Prefabs.UI;
using DecentM.Prefabs.VideoPlayer;
using DecentM.Prefabs.VideoPlayer.Plugins;
using DecentM.Prefabs.VideoPlayer.Handlers;
using DecentM.Prefabs.Notifications;
using DecentM.Prefabs.Notifications.Providers;
using DecentM.Prefabs.PlayerList;
using DecentM.Prefabs.VideoRatelimit;

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
            whiteList.Add(typeof(PubsubHost));
            whiteList.Add(typeof(PubsubSubscriber));

            /*
             * PerformanceGovernor
             */
            whiteList.Add(typeof(PerformanceGovernor));

            /*
             * UI
             */
            whiteList.Add(typeof(Dropdown));
            whiteList.Add(typeof(DropdownOption));

            /*
             * VideoPlayer
             */
            whiteList.Add(typeof(UnityEngine.Video.VideoPlayer));
            
            // Core
            whiteList.Add(typeof(UnityVideo));
            whiteList.Add(typeof(ScreenHandler));
            whiteList.Add(typeof(VideoPlayerEvents));
            whiteList.Add(typeof(VideoPlayerSystem));
            whiteList.Add(typeof(VideoPlayerUI));

            // Plugins
            whiteList.Add(typeof(AutoPlayPlugin));
            whiteList.Add(typeof(AutoRetryPlugin));
            whiteList.Add(typeof(DebugPlugin));
            whiteList.Add(typeof(LoadRequestHandlerPlugin));
            whiteList.Add(typeof(RenderTextureExportPlugin));
            whiteList.Add(typeof(ResolutionUpdaterPlugin));
            whiteList.Add(typeof(ScreenAnalysisPlugin));
            whiteList.Add(typeof(SkipToTimestampPlugin));
            whiteList.Add(typeof(SoundEffectsPlugin));
            whiteList.Add(typeof(SubtitlesPlugin));
            whiteList.Add(typeof(TextureReferencePlugin));
            whiteList.Add(typeof(TextureUpdaterPlugin));
            whiteList.Add(typeof(UIPlugin));
            whiteList.Add(typeof(URLVerifierPlugin));

            // Playlist Plugins
            whiteList.Add(typeof(PlaylistItemRenderer));
            whiteList.Add(typeof(PlaylistPlayerPlugin));
            whiteList.Add(typeof(VideoPlaylist));
            whiteList.Add(typeof(VideoPlaylistUI));

            /*
             * VideoRatelimit
             */
            whiteList.Add(typeof(PlayerLoadMonitoring));
            whiteList.Add(typeof(VideoRatelimitSystem));

            /*
             * NotificationSystem
             */
            whiteList.Add(typeof(NotificationSystem));
            whiteList.Add(typeof(PerformanceLevelChangeProvider));

            /*
             * ManualCamera
             */
            whiteList.Add(typeof(ManualCamera));

            /*
             * LibDecentM
             */
            whiteList.Add(typeof(Debugging));
            whiteList.Add(typeof(LibDecentM));
            whiteList.Add(typeof(PlayerList));
            whiteList.Add(typeof(Scheduling));

            /*
             * CustomEvent
             */
            whiteList.Add(typeof(TimedCustomEvent));

            Traverse.Create<CVRTools>().Field("componentWhiteList").SetValue(whiteList);
        }
    }
}
