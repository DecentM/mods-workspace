using System.IO;
using UnityEditor;
using UnityEditor.Experimental.AssetImporters;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace DecentM.EditorTools.SelfLocator
{
    [CustomEditor(typeof(SelfLocatorImporter))]
    public class SelfLocatorInspector : ScriptedImporterEditor
    {
        public override void OnInspectorGUI()
        {
            SelfLocatorImporter importer = (SelfLocatorImporter)target;

            GUIStyle style = new GUIStyle(GUI.skin.box);

            style.fixedWidth = Screen.width - 32;
            style.margin = new RectOffset(16, 16, 16, 16);
            style.padding = new RectOffset(16, 16, 8, 0);
            style.alignment = TextAnchor.MiddleLeft;
            style.fontSize = 12;
            style.normal.textColor = Color.white;

            EditorGUILayout.LabelField(File.ReadAllText(importer.assetPath), style);

            base.ApplyRevertGUI();
        }
    }
}
