#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using UdonSharpEditor;
using DecentM.EditorTools;
using VRC.Udon;

namespace DecentM.Keyboard
{
    [CustomEditor(typeof(KeyboardKey))]
    public class KeyboardKeyInspector : Inspector
    {
        KeyboardKey key;

        UdonBehaviour currentLayout
        {
            get { return (UdonBehaviour)this.GetUdonVariable(this.key, "layoutTemplate"); }
        }

        KeyLayout[] layouts
        {
            get { return this.currentLayout.transform.parent.GetComponentsInChildren<KeyLayout>(); }
        }

        private void OnKeyLayoutChange(object layoutIndex)
        {
            KeyLayout layout = this.layouts[(int)layoutIndex];
            this.SetUdonVariable(
                this.key,
                "layoutTemplate",
                UdonSharpEditorUtility.GetBackingUdonBehaviour(layout)
            );
        }

        private int currentTab = 0;

        public override void OnInspectorGUI()
        {
            List<EnumerableOption> tabs = new List<EnumerableOption>();
            tabs.Add(new EnumerableOption("Simple", 0));
            tabs.Add(new EnumerableOption("Advanced", 1));

            this.currentTab = (int)this.TabBar(tabs, this.currentTab);

            switch (this.currentTab)
            {
                case 0:
                    this.DrawSimpleTab();
                    break;

                case 1:
                    this.DrawDefaultInspector();
                    break;
            }
        }

        private void DrawSimpleTab()
        {
            this.key = (KeyboardKey)target;

            Rect position = GUILayoutUtility.GetRect(
                GUIContent.none,
                GUIStyle.none,
                GUILayout.Height(17.5f)
            );
            List<EnumerableOption> options = new List<EnumerableOption>();

            for (int i = 0; i < layouts.Length; i++)
            {
                options.Add(new EnumerableOption(layouts[i].name, i));
            }

            this.Dropdown(
                position,
                "Key layout",
                options,
                this.currentLayout.name,
                this.OnKeyLayoutChange
            );

            EditorGUILayout.Space();

            EditorGUILayout.BeginHorizontal();

            EditorGUILayout.BeginVertical();
            Rect pRect1 = GUILayoutUtility.GetRect(
                GUIContent.none,
                GUIStyle.none,
                GUILayout.Height(12)
            );
            EditorGUI.PrefixLabel(pRect1, new GUIContent("Primary symbol"));
            Rect pRect2 = GUILayoutUtility.GetRect(
                GUIContent.none,
                GUIStyle.none,
                GUILayout.Height(17.5f)
            );
            EditorGUI.TextField(pRect2, "");
            EditorGUILayout.EndVertical();

            EditorGUILayout.BeginVertical();
            Rect sRect1 = GUILayoutUtility.GetRect(
                GUIContent.none,
                GUIStyle.none,
                GUILayout.Height(12)
            );
            EditorGUI.PrefixLabel(sRect1, new GUIContent("Secondary symbol"));
            Rect sRect2 = GUILayoutUtility.GetRect(
                GUIContent.none,
                GUIStyle.none,
                GUILayout.Height(17.5f)
            );
            EditorGUI.TextField(sRect2, "");
            EditorGUILayout.EndVertical();

            EditorGUILayout.BeginVertical();
            Rect tRect1 = GUILayoutUtility.GetRect(
                GUIContent.none,
                GUIStyle.none,
                GUILayout.Height(12)
            );
            EditorGUI.PrefixLabel(pRect1, new GUIContent("Tertiary symbol"));
            Rect tRect2 = GUILayoutUtility.GetRect(
                GUIContent.none,
                GUIStyle.none,
                GUILayout.Height(17.5f)
            );
            EditorGUI.TextField(tRect2, "");
            EditorGUILayout.EndVertical();

            EditorGUILayout.EndHorizontal();
        }
    }
}
#endif
