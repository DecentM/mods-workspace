using System.Linq;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using VRC.SDKBase;

using UnityEditorInternal;
using DecentM.EditorTools;
using DecentM.VideoPlayer.Plugins;

namespace DecentM.VideoPlayer
{
    [CustomEditor(typeof(VideoPlayerUI))]
    public class VideoPlayerUIInspector : Inspector
    {
        public override void OnInspectorGUI()
        {
            VideoPlayerUI ui = (VideoPlayerUI)target;

            // GUI.DrawTexture(new Rect(10, 10, 60, 60), aTexture, ScaleMode.ScaleToFit, true, 10.0F);
            this.DrawImage(EditorAssets.VideoPlayerBanner);

            AutoPlayPlugin autoPlay = ui.GetComponentInChildren<AutoPlayPlugin>();
            if (autoPlay != null)
            {
                this.HelpBox(MessageType.Info, autoPlay.enabled
                    ? "When a URL loads, it will start playing on its own. If there are others in the world, the video player will wait until everyone has loaded. While waiting for everyone to load, the play button will be available to skip waiting."
                    : "When a URL loads, the play button will become available to start playback"
                );

                autoPlay.enabled = this.Toggle("Autoplay", autoPlay.enabled);
            }

            LoadingOverlayPlugin loading = ui.GetComponentInChildren<LoadingOverlayPlugin>();
            if (loading != null)
            {
                this.HelpBox(MessageType.Info, loading.enabled
                    ? "While the video is loading, an animation will play to let people know about it"
                    : "A black screen will be visible while videos are loading"
                );

                loading.enabled = this.Toggle("Loading screen", loading.enabled);
            }

            UIPlugin uiPlugin = ui.GetComponentInChildren<UIPlugin>();
            if (uiPlugin != null)
            {
                this.HelpBox(MessageType.Info, uiPlugin.enabled
                    ? "A user interface will be visible when the player aims at the screen, that lets them control playback and ownership"
                    : "Users will not be able to control the video playback"
                );

                if (!uiPlugin.enabled && (autoPlay == null || !autoPlay.enabled)) this.HelpBox(MessageType.Warning, "If the UI is turned off, disabling autoplay will make the player unusable!");

                uiPlugin.enabled = this.Toggle("UI", uiPlugin.enabled);
            }

            GlobalSyncPlugin globalSync = ui.GetComponentInChildren<GlobalSyncPlugin>();
            if (globalSync != null)
            {
                this.HelpBox(MessageType.Info, globalSync.enabled
                    ? "The video player is global, everyone will be seeing the same video"
                    : "The video player is local, each player will only see their own video"
                );

                globalSync.enabled = this.Toggle("Global sync", globalSync.enabled);
            }

            SoundEffectsPlugin soundEffects = ui.GetComponentInChildren<SoundEffectsPlugin>();
            if (soundEffects != null)
            {
                this.HelpBox(MessageType.Info, soundEffects.enabled
                    ? "Sound effects will play when there's no video playing"
                    : "Sound effects will not play under any circumstance"
                );

                soundEffects.enabled = this.Toggle("Sound effects", soundEffects.enabled);
            }

            this.SaveModifications();
            this.ApplySettings();
        }

        private void ApplySettings()
        {
            VideoPlayerUI ui = (VideoPlayerUI)target;

            if (ui == null) return;

            PlaylistPlayerPlugin playlistPlayer = ui.GetComponentInChildren<PlaylistPlayerPlugin>();
            if (playlistPlayer != null) playlistPlayer.enabled = ui.hasDefaultPlaylist;

            /*
            Plugin  = ui.GetComponentInChildren<>();
            if ( != null) .enabled = ui.;
            */
        }
    }
}
