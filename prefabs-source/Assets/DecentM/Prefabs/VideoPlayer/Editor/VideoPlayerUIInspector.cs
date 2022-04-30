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
    public class VideoPlayerUIInspector : DEditor
    {
        public override void OnInspectorGUI()
        {
            VideoPlayerUI ui = (VideoPlayerUI)target;

            // GUI.DrawTexture(new Rect(10, 10, 60, 60), aTexture, ScaleMode.ScaleToFit, true, 10.0F);
            this.DrawImage(EditorAssets.VideoPlayerBanner);

            this.HelpBox(MessageType.Info, ui.autoPlay
                ? "When a URL loads, it will start playing on its own. If there are others in the world, the video player will wait until everyone has loaded. While waiting for everyone to load, the play button will be available to skip waiting."
                : "When a URL loads, the play button will become available to start playback"
            );

            ui.autoPlay = this.Toggle("Autoplay", ui.autoPlay);

            this.HelpBox(MessageType.Info, ui.loadingScreen
                ? "While the video is loading, an animation will play to let people know about it"
                : "A black screen will be visible while videos are loading"
            );

            ui.loadingScreen = this.Toggle("Loading screen", ui.loadingScreen);

            this.HelpBox(MessageType.Info, ui.ui
                ? "A user interface will be visible when the player aims at the screen, that lets them control playback and ownership"
                : "Users will not be able to control the video playback"
            );

            if (!ui.ui && !ui.autoPlay) this.HelpBox(MessageType.Warning, "If the UI is turned off, disabling autoplay will make the player unusable!");

            ui.ui = this.Toggle("UI", ui.ui);

            this.HelpBox(MessageType.Info, ui.globalSync
                ? "The video player is global, everyone will be seeing the same video"
                : "The video player is local, each player will only see their own video"
            );

            ui.globalSync = this.Toggle("Global sync", ui.globalSync);

            this.HelpBox(MessageType.Info, ui.soundEffects
                ? "Sound effects will play when there's no video playing"
                : "Sound effects will not play under any circumstance"
            );

            ui.soundEffects = this.Toggle("Sound effects", ui.soundEffects);

            serializedObject.ApplyModifiedProperties();
            DEditor.SavePrefabModifications(ui);
            this.ApplySettings();
        }

        private void ApplySettings()
        {
            VideoPlayerUI ui = (VideoPlayerUI)target;

            if (ui == null) return;

            PlaylistPlayerPlugin playlistPlayer = ui.GetComponentInChildren<PlaylistPlayerPlugin>();
            if (playlistPlayer != null) playlistPlayer.enabled = ui.hasDefaultPlaylist;

            AutoPlayPlugin autoPlay = ui.GetComponentInChildren<AutoPlayPlugin>();
            if (autoPlay != null) autoPlay.enabled = ui.autoPlay;

            LoadingOverlayPlugin loading = ui.GetComponentInChildren<LoadingOverlayPlugin>();
            if (loading != null) loading.enabled = ui.loadingScreen;

            UIPlugin uiPlugin = ui.GetComponentInChildren<UIPlugin>();
            if (uiPlugin != null) uiPlugin.enabled = ui.ui;

            GlobalSyncPlugin globalSync = ui.GetComponentInChildren<GlobalSyncPlugin>();
            if (globalSync != null) globalSync.enabled = ui.globalSync;

            SoundEffectsPlugin soundEffects = ui.GetComponentInChildren<SoundEffectsPlugin>();
            if (soundEffects != null) soundEffects.enabled = ui.soundEffects;

            /*
            Plugin  = ui.GetComponentInChildren<>();
            if ( != null) .enabled = ui.;
            */
        }
    }
}
