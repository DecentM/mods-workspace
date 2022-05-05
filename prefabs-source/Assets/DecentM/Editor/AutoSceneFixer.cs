using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

using VRC.SDKBase.Editor.BuildPipeline;

namespace DecentM.EditorTools
{
    public class SceneFixingException : Exception
    {
        public SceneFixingException(string message, Exception inner)
            : base(message, inner) { }

        public SceneFixingException(string message)
            : base(message) { }
    }

    public enum SceneFixEvent
    {
        SceneOpened,
        EditMode,
        HierarchyChanged,
        SceneSave,
    }

    public abstract class AutoSceneFixer : IVRCSDKBuildRequestedCallback
    {
        private void AttachEvents(SceneFixEvent[] events)
        {
            if (events.Contains(SceneFixEvent.SceneOpened)) EditorSceneManager.sceneOpened += OnSceneOpened;
            if (events.Contains(SceneFixEvent.EditMode)) EditorApplication.playModeStateChanged += OnChangePlayMode;
            if (events.Contains(SceneFixEvent.HierarchyChanged)) EditorApplication.hierarchyChanged += OnHierarchyChanged;
            if (events.Contains(SceneFixEvent.SceneSave)) EditorSceneManager.sceneSaving += OnBeforeSave;
        }

        public AutoSceneFixer(params SceneFixEvent[] events)
        {
            this.AttachEvents(events);
        }

        public AutoSceneFixer()
        {
            this.AttachEvents(new SceneFixEvent[] { SceneFixEvent.SceneOpened, SceneFixEvent.EditMode, SceneFixEvent.SceneSave });
        }

        protected abstract bool OnPerformFixes();

        public int callbackOrder => 8;

        public virtual bool OnBuildRequested(VRCSDKRequestedBuildType requestedBuildType)
        {
            if (requestedBuildType != VRCSDKRequestedBuildType.Scene) return true;

            bool success = this.OnPerformFixes();
            if (!success) Debug.LogError("Failed to perform fixes while building the world");
            return success;
        }

        private void OnChangePlayMode(PlayModeStateChange state)
        {
            if (state != PlayModeStateChange.EnteredEditMode) return;

            bool success = this.OnPerformFixes();
            if (!success) throw new SceneFixingException("Failed to perform fixes while entering edit mode");
        }

        private void OnSceneOpened(Scene scene, OpenSceneMode mode)
        {
            bool success = this.OnPerformFixes();
            if (!success) throw new SceneFixingException("Failed to perform fixes while opening scene");
        }

        private void OnHierarchyChanged()
        {
            bool success = this.OnPerformFixes();
            if (!success) throw new SceneFixingException("Failed to perform fixes during hierarchy change");
        }

        private void OnBeforeSave(Scene scene, string path)
        {
            bool success = this.OnPerformFixes();
            if (!success) throw new SceneFixingException("Failed to perform fixes during scene save");
        }
    }
}

