using System;
using System.Collections.Generic;

using MelonLoader;
using HarmonyLib;

using ABI_RC.Core;

using UnityEngine;
using UnityEngine.Tilemaps;
using UnityEngine.Rendering;
using UnityEngine.Playables;
using UnityEngine.AI;

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
        public override void OnApplicationStart()
        {
            LoggerInstance.Msg("Patching NetworkManager...");
            HarmonyLib.Harmony.CreateAndPatchAll(typeof(NetworkManagerPatches));
        }

        public override void OnApplicationLateStart()
        {
            LoggerInstance.Msg("Extending component whitelist...");
            HashSet<Type> whiteList = (HashSet<Type>)Traverse.Create<CVRTools>().Field("componentWhiteList").GetValue();

            /*
             * Unity stuff
             */
            whiteList.Add(typeof(WindZone));

            whiteList.Add(typeof(Tilemap));
            whiteList.Add(typeof(TilemapRenderer));

            whiteList.Add(typeof(Terrain));
            whiteList.Add(typeof(Tree));

            whiteList.Add(typeof(Grid));
            whiteList.Add(typeof(GridLayout));

            whiteList.Add(typeof(AudioSource));
            whiteList.Add(typeof(AudioReverbZone));
            whiteList.Add(typeof(AudioLowPassFilter));
            whiteList.Add(typeof(AudioHighPassFilter));
            whiteList.Add(typeof(AudioDistortionFilter));
            whiteList.Add(typeof(AudioEchoFilter));
            whiteList.Add(typeof(AudioChorusFilter));
            whiteList.Add(typeof(AudioReverbFilter));

            whiteList.Add(typeof(PlayableDirector));
            whiteList.Add(typeof(TerrainCollider));
            whiteList.Add(typeof(Canvas));
            whiteList.Add(typeof(CanvasGroup));
            whiteList.Add(typeof(CanvasRenderer));
            whiteList.Add(typeof(NavMeshAgent));
            whiteList.Add(typeof(NavMeshObstacle));
            whiteList.Add(typeof(OffMeshLink));
            whiteList.Add(typeof(Joint));
            whiteList.Add(typeof(CharacterJoint));
            whiteList.Add(typeof(ConstantForce));

            whiteList.Add(typeof(MeshCollider));
            whiteList.Add(typeof(CapsuleCollider));

            whiteList.Add(typeof(ParticleSystem));
            whiteList.Add(typeof(ParticleSystemRenderer));

            whiteList.Add(typeof(BillboardRenderer));
            whiteList.Add(typeof(Camera));
            whiteList.Add(typeof(FlareLayer));

            whiteList.Add(typeof(Light));
            whiteList.Add(typeof(LightProbeGroup));

            whiteList.Add(typeof(LODGroup));

            whiteList.Add(typeof(ReflectionProbe));
            whiteList.Add(typeof(SpriteRenderer));
            whiteList.Add(typeof(SortingGroup));
            whiteList.Add(typeof(Projector));
            whiteList.Add(typeof(OcclusionPortal));
            whiteList.Add(typeof(OcclusionArea));
            whiteList.Add(typeof(LensFlare));
            whiteList.Add(typeof(Skybox));

            // TODO: Add more components from UnityEngine.VehiclesModule?
            whiteList.Add(typeof(WheelCollider));

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
