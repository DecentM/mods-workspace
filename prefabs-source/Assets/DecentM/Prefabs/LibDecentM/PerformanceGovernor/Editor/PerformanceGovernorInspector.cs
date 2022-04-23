using UnityEditor;
using UnityEngine;

namespace DecentM
{
    [CustomEditor(typeof(PerformanceGovernor))]
    public class PerformanceGovernorInspector : Editor
    {
        private const int AbsoluteHigh = 90;
        private const int AbsoluteLow = 20;
        private const int VerticalSpacing = 30;

        public override void OnInspectorGUI()
        {
            PerformanceGovernor governor = (PerformanceGovernor)target;

            Rect hArea = EditorGUILayout.BeginVertical(GUILayout.Height(VerticalSpacing));
            float newHigh = EditorGUILayout.Slider(new GUIContent("High", "Current performance will be considered high above this value"), governor.high, governor.low + 1, AbsoluteHigh);
            governor.high = Mathf.RoundToInt(newHigh);
            EditorGUILayout.EndVertical();

            Rect lArea = EditorGUILayout.BeginVertical(GUILayout.Height(VerticalSpacing));
            float newLow = EditorGUILayout.Slider(new GUIContent("Low", "Current performance will be considered low below this value"), governor.low, AbsoluteLow, governor.high - 1);
            governor.low = Mathf.RoundToInt(newLow);
            EditorGUILayout.EndVertical();

            Rect debugArea = EditorGUILayout.BeginVertical(GUILayout.Height(VerticalSpacing));
            float newFps = EditorGUILayout.Slider(new GUIContent("Editor framerate (for debugging)", "Sets the target FPS in the editor, for testing. Has no actual effect in game."), Application.targetFrameRate, AbsoluteLow, 300);
            Application.targetFrameRate = Mathf.RoundToInt(newFps);
            EditorGUILayout.EndVertical();

            DrawDefaultInspector();
        }
    }
}
