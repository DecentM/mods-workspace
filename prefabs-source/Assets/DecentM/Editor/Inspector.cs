using System;
using System.Collections.Immutable;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditorInternal;
using VRC.Udon;
using VRC.Udon.Common;
using VRC.Udon.Common.Interfaces;
using UdonSharpEditor;
using UdonSharp;

namespace DecentM.EditorTools
{
    public class Inspector : Editor
    {
        public struct EnumerableOption
        {
            public EnumerableOption(string label, object id)
            {
                this.label = label;
                this.id = id;
            }

            public string label;
            public object id;
        }

        public delegate void OnDropdownChange(object parameter);

        protected void Dropdown(Rect position, string label, List<EnumerableOption> options, object current, OnDropdownChange OnChange)
        {
            void handleMenuItemClicked(object parameter)
            {
                OnChange(parameter);
            }

            GenericMenu menu = new GenericMenu();

            string activeLabel = "";

            foreach (EnumerableOption option in options)
            {
                if (option.id == current)
                {
                    menu.AddDisabledItem(new GUIContent(option.label));
                    activeLabel = option.label;
                }
                else
                {
                    menu.AddItem(new GUIContent(option.label), false, handleMenuItemClicked, option.id);
                }
            }

            GUIStyle dropdownStyle = new GUIStyle("DropDownButton");
            dropdownStyle.alignment = TextAnchor.MiddleRight;
            dropdownStyle.padding = new RectOffset(0, 18, 0, 0);

            if (EditorGUI.DropdownButton(position, new GUIContent(activeLabel), FocusType.Keyboard, dropdownStyle)) menu.ShowAsContext();
            EditorGUI.LabelField(position, $" {label}");
        }

        public static void SaveModifications(UnityEngine.Object @object)
        {
            SerializedObject serializedObject = new SerializedObject(@object);
            Inspector.SaveModifications(@object, serializedObject);
        }

        public static void SaveModifications(UnityEngine.Object @object, SerializedObject serializedObject)
        {
            if (PrefabUtility.IsPartOfAnyPrefab(@object))
            {
                PrefabUtility.RecordPrefabInstancePropertyModifications(@object);
                EditorUtility.SetDirty(@object);
            }

            serializedObject.ApplyModifiedProperties();
        }

        protected void SaveModifications()
        {
            Inspector.SaveModifications(this.target, this.serializedObject);
        }

        protected ReorderableList CreateReorderableList(SerializedObject serializedObject, string propertyPath)
        {
            SerializedProperty property = serializedObject.FindProperty(propertyPath);
            ReorderableList list = new ReorderableList(serializedObject, property, true, true, true, true);

            list.drawElementCallback = (Rect rect, int index, bool isActive, bool isFocused) =>
            {
                Rect testFieldRect = new Rect(rect.x, rect.y + 2, rect.width, EditorGUIUtility.singleLineHeight);

                EditorGUI.PropertyField(testFieldRect, list.serializedProperty.GetArrayElementAtIndex(index), label: new GUIContent());
            };

            list.drawHeaderCallback = (Rect rect) =>
            {
                EditorGUI.LabelField(rect, new GUIContent(
                    "Default Playlist URLs",
                    "URLs that will play in sequence when you join the world until someone puts in a video."
                ));
            };

            return list;
        }

        protected object TabBar(List<EnumerableOption> tabs, object current)
        {
            EditorGUILayout.Space();
            GUILayout.BeginHorizontal();

            object clickedTab = current;
            foreach (EnumerableOption tab in tabs)
            {
                GUIStyle buttonStyle = new GUIStyle("toolbarbutton");
                buttonStyle.fixedHeight = 25;

                if (current == tab.id)
                {
                    Texture2D activeBackground = new Texture2D(1, 1);
                    activeBackground.SetPixel(0, 0, new Color(56, 56, 56, .2f));
                    activeBackground.Apply();
                    activeBackground.wrapMode = TextureWrapMode.Repeat;
                    buttonStyle.normal.background = activeBackground;
                    
                    buttonStyle.fontStyle = FontStyle.Bold;
                    buttonStyle.normal.textColor = Color.white;
                }

                if (GUILayout.Button(tab.label, buttonStyle)) clickedTab = tab.id;
            }

            GUILayout.EndHorizontal();
            EditorGUILayout.Space();

            return clickedTab;
        }

        protected bool Toggle(string label, bool value)
        {
            return EditorGUILayout.Toggle(label, value);
        }

        protected void HelpBox(MessageType type, string contents)
        {
            EditorGUILayout.HelpBox(contents, type);
        }

        protected void DrawImage(Texture image, Rect rect)
        {
            if (image == null) return;

            EditorGUI.DrawPreviewTexture(rect, image);
        }

        protected void DrawImage(Texture image)
        {
            if (image == null) return;

            float imageAspectRatio = 1f * image.width / image.height;

            Rect drawRect = EditorGUILayout.GetControlRect(false, Screen.width / imageAspectRatio);
            drawRect.x = 0;
            drawRect.width = Screen.width;

            this.DrawImage(image, drawRect);
        }

        protected Rect DrawRegion(float height, Vector4 margin, Vector4 padding)
        {
            Rect drawRect = EditorGUILayout.GetControlRect(false, height);

            /**
             * =================================|
             * |              ^ y               |
             * |--------------------------------|
             * |< x |                       z > |
             * |--------------------------------|
             * |              v w               |
             * =================================|
             */

            drawRect.width -= (margin.z + margin.x);
            drawRect.height -= (margin.w + margin.y);
            drawRect.x += margin.x;
            drawRect.y += margin.y;

            return this.GetRectInside(drawRect, padding);
        }

        protected Rect GetRectInside(Rect parent, Vector4 padding)
        {
            Rect contentRect = new Rect(parent);

            contentRect.width -= (padding.z + padding.x);
            contentRect.height -= (padding.w + padding.y);
            contentRect.x += padding.x;
            contentRect.y += padding.y;

            return contentRect;
        }

        protected Rect DrawRegion(float height)
        {
            return this.DrawRegion(height, new Vector4(0, 0, 0, 0), new Vector4(0, 0, 0, 0));
        }

        protected bool Button(string label, float progress = 0f)
        {
            // TODO: Return to this shitfest when I need to display a progress bar inside a button
            /*
            GUIStyle boxStyle = new GUIStyle();
            Texture2D loadingBackground = new Texture2D(1, 1);
            loadingBackground.SetPixel(0, 0, new Color(0, 56, 0, .2f));
            loadingBackground.Apply();
            loadingBackground.wrapMode = TextureWrapMode.Repeat;
            boxStyle.normal.background = loadingBackground;
            boxStyle.fixedHeight = 17.5f;

            // Rect rect = GUILayoutUtility.GetRect(GUIContent.none, boxStyle, GUILayout.Height(17.5f));
            GUILayout.Box(loadingBackground, boxStyle);
            */
            GUIStyle buttonStyle = new GUIStyle("LargeButton");

            bool pressed = GUILayout.Button(label, buttonStyle);

            return pressed;
        }

        private IUdonVariable CreateUdonVariable(string symbolName, object value, System.Type type)
        {
            Type udonVariableType = typeof(UdonVariable<>).MakeGenericType(type);
            return (IUdonVariable)Activator.CreateInstance(udonVariableType, symbolName, value);
        }

        protected void LinkAllWithName(GameObject root, string name, UdonSharpBehaviour linkTarget)
        {
            UdonBehaviour[] allBehaviours = root.GetComponentsInChildren<UdonBehaviour>();
            int failCount = 0;

            for (int i = 0; i < allBehaviours.Length; i++)
            {
                UdonBehaviour behaviour = allBehaviours[i];
                EditorUtility.DisplayProgressBar($"Linking... ({i}/{allBehaviours.Length})", behaviour == null ? "Skipping..." : behaviour.name, (float)i / allBehaviours.Length);

                if (behaviour == null) continue;

                IUdonProgram program = behaviour.programSource.SerializedProgramAsset.RetrieveProgram();
                ImmutableArray<string> exportedSymbolNames = program.SymbolTable.GetExportedSymbols();

                foreach (string exportedSymbolName in exportedSymbolNames)
                {
                    if (!exportedSymbolName.Equals(name)) continue;

                    UdonBehaviour variableValue = UdonSharpEditorUtility.GetBackingUdonBehaviour(linkTarget);
                    Type symbolType = program.SymbolTable.GetSymbolType(exportedSymbolName);

                    if (behaviour.publicVariables.TrySetVariableValue(exportedSymbolName, variableValue)) continue;
                    if (behaviour.publicVariables.TryAddVariable(CreateUdonVariable(exportedSymbolName, variableValue, symbolType))) continue;

                    Debug.LogError($"Failed to set public variable '{exportedSymbolName}' value.");
                    failCount++;
                }

                if (PrefabUtility.IsPartOfPrefabInstance(behaviour)) PrefabUtility.RecordPrefabInstancePropertyModifications(behaviour);
            }

            if (failCount > 0) EditorUtility.DisplayDialog("Linking report", $"Failed to link {failCount} UdonBehaviour(s), check the console for details!", "OK");
            EditorUtility.ClearProgressBar();
        }

        protected object GetUdonVariable(UdonSharpBehaviour udonSharpBehaviour, string variableName)
        {
            UdonBehaviour behaviour = UdonSharpEditorUtility.GetBackingUdonBehaviour(udonSharpBehaviour);

            behaviour.publicVariables.TryGetVariableValue(variableName, out object result);
            return result;
        }

        protected bool SetUdonVariable(UdonSharpBehaviour udonSharpBehaviour, string variableName, object variableValue)
        {
            UdonBehaviour behaviour = UdonSharpEditorUtility.GetBackingUdonBehaviour(udonSharpBehaviour);

            return behaviour.publicVariables.TrySetVariableValue(variableName, variableValue);
        }
    }
}
