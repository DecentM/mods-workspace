
#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using DecentM.Subtitles;
using System.Diagnostics;
using System.Collections.Generic;
using VRC.Udon;

[CustomEditor(typeof(SubtitleManager))]
public class EditorParserInspector : Editor
{
    private UnityImporter importer = new UnityImporter();

    public override void OnInspectorGUI()
    {
        EditorGUILayout.HelpBox("Subtitle files need to be converted into a machine readable format, so that performance remains high.\nAfter adding, removing, or editing subtitles, press this button below to regenerate subtitle files.", MessageType.Info);

        if (GUILayout.Button("Rebuild subtitle library"))
        {
            Stopwatch sw = new Stopwatch();
            sw.Start();
            List<TextAsset> assets = this.importer.ImportAll();
            sw.Stop();
            UnityEngine.Debug.Log($"Subtitle library rebuilt in {sw.Elapsed}");

            SubtitleManager manager = Selection.activeGameObject.GetComponent<SubtitleManager>();
            manager.instructionsFiles = assets.ToArray();
        }

        EditorGUILayout.Separator();

        DrawDefaultInspector();
    }
}
#endif
