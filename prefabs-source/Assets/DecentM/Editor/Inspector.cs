using System.Reflection;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditorInternal;

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

        protected void Dropdown(
            Rect position,
            string label,
            List<EnumerableOption> options,
            object current,
            OnDropdownChange OnChange
        )
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
                    menu.AddItem(
                        new GUIContent(option.label),
                        false,
                        handleMenuItemClicked,
                        option.id
                    );
                }
            }

            GUIStyle dropdownStyle = new GUIStyle("DropDownButton");
            dropdownStyle.alignment = TextAnchor.MiddleRight;
            dropdownStyle.padding = new RectOffset(0, 18, 0, 0);

            if (
                EditorGUI.DropdownButton(
                    position,
                    new GUIContent(activeLabel),
                    FocusType.Keyboard,
                    dropdownStyle
                )
            )
                menu.ShowAsContext();
            EditorGUI.LabelField(position, $" {label}");
        }

        public static void SaveModifications(UnityEngine.Object @object)
        {
            SerializedObject serializedObject = new SerializedObject(@object);
            Inspector.SaveModifications(@object, serializedObject);
        }

        public static void SaveModifications(
            UnityEngine.Object @object,
            SerializedObject serializedObject
        )
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

        protected ReorderableList CreateReorderableList(
            SerializedObject serializedObject,
            string propertyPath
        )
        {
            SerializedProperty property = serializedObject.FindProperty(propertyPath);
            ReorderableList list = new ReorderableList(
                serializedObject,
                property,
                true,
                true,
                true,
                true
            );

            list.drawElementCallback = (Rect rect, int index, bool isActive, bool isFocused) =>
            {
                Rect testFieldRect = new Rect(
                    rect.x,
                    rect.y + 2,
                    rect.width,
                    EditorGUIUtility.singleLineHeight
                );

                EditorGUI.PropertyField(
                    testFieldRect,
                    list.serializedProperty.GetArrayElementAtIndex(index),
                    label: new GUIContent()
                );
            };

            list.drawHeaderCallback = (Rect rect) =>
            {
                EditorGUI.LabelField(
                    rect,
                    new GUIContent(
                        "Default Playlist URLs",
                        "URLs that will play in sequence when you join the world until someone puts in a video."
                    )
                );
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

                if (GUILayout.Button(tab.label, buttonStyle))
                    clickedTab = tab.id;
            }

            GUILayout.EndHorizontal();
            EditorGUILayout.Space();

            return clickedTab;
        }

        protected bool ToolbarButton(Rect rect, string label)
        {
            GUIStyle buttonStyle = new GUIStyle("toolbarbutton");
            buttonStyle.fixedHeight = rect.height;

            return GUI.Button(rect, label, buttonStyle);
        }

        protected bool Toggle(string label, bool value)
        {
            return EditorGUILayout.Toggle(label, value);
        }

        protected void HelpBox(MessageType type, string contents)
        {
            EditorGUILayout.HelpBox(contents, type);
        }

        protected void DrawImage(Sprite image)
        {
            if (image == null)
                return;

            EditorGUI.DrawPreviewTexture(image.rect, image.texture);
        }

        protected void DrawImage(Sprite image, Rect rect)
        {
            if (image == null)
                return;

            EditorGUI.DrawPreviewTexture(rect, image.texture);
        }

        protected void DrawImage(Texture image, Rect rect)
        {
            if (image == null)
                return;

            EditorGUI.DrawPreviewTexture(rect, image);
        }

        protected void DrawImage(Texture image)
        {
            if (image == null)
                return;

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

        protected Rect GetRectInside(Rect parent, Vector2 size)
        {
            Rect result = new Rect(parent);

            result.width = size.x;
            result.height = size.y;

            return result;
        }

        protected Rect GetRectInside(Rect parent, Vector4 margin)
        {
            Rect result = new Rect(parent);

            result.width -= margin.z * 2;
            result.height -= margin.w * 2;
            result.x += margin.x;
            result.y += margin.y;

            return result;
        }

        protected Rect GetRectInside(Rect parent, Vector2 size, Vector4 margin)
        {
            Rect result = this.GetRectInside(parent, size);
            return this.GetRectInside(result, margin);
        }

        protected Rect DrawRegion(float height, Vector4 padding)
        {
            return this.DrawRegion(height, new Vector4(0, 0, 0, 0), padding);
        }

        protected Rect DrawRegion(float height)
        {
            return this.DrawRegion(height, new Vector4(0, 0, 0, 0), new Vector4(0, 0, 0, 0));
        }

        protected bool Button(string label)
        {
            GUIStyle buttonStyle = new GUIStyle("LargeButton");

            bool pressed = GUILayout.Button(label, buttonStyle);

            return pressed;
        }

        protected bool Button(Rect rect, string label)
        {
            GUIStyle buttonStyle = new GUIStyle("toolbarbutton");

            buttonStyle.fixedHeight = rect.height;

            bool pressed = GUI.Button(rect, label, buttonStyle);

            return pressed;
        }

        protected bool Button(Rect rect, Texture texture)
        {
            GUIStyle buttonStyle = new GUIStyle("toolbarbutton");

            buttonStyle.fixedHeight = rect.height;

            int targetSize = 20;
            int horizontalPadding = Mathf.FloorToInt((rect.width - targetSize) / 2);
            buttonStyle.padding = new RectOffset(horizontalPadding, horizontalPadding, 0, 0);

            bool pressed = GUI.Button(rect, texture, buttonStyle);

            return pressed;
        }

        protected void DrawLabel(
            Rect rect,
            string contents,
            int size,
            FontStyle fontStyle,
            Color color
        )
        {
            GUIStyle style = new GUIStyle();
            style.fontSize = Mathf.CeilToInt((1 + size) * 4);
            style.fontStyle = fontStyle;
            style.normal.textColor = color;
            style.wordWrap = false;
            style.clipping = TextClipping.Clip;
            EditorGUI.LabelField(rect, contents, style);
        }

        protected void DrawLabel(Rect rect, string contents)
        {
            this.DrawLabel(rect, contents, 1, FontStyle.Normal, Color.white);
        }

        protected void DrawLabel(Rect rect, string contents, int size)
        {
            this.DrawLabel(rect, contents, size, FontStyle.Normal, Color.white);
        }

        protected void DrawLabel(Rect rect, string contents, int size, FontStyle style)
        {
            this.DrawLabel(rect, contents, size, style, Color.white);
        }

        protected void LinkAllWithName(GameObject root, string name, MonoBehaviour linkTarget)
        {
            MonoBehaviour[] allBehaviours = root.GetComponentsInChildren<MonoBehaviour>();
            int failCount = 0;

            for (int i = 0; i < allBehaviours.Length; i++)
            {
                MonoBehaviour behaviour = allBehaviours[i];
                EditorUtility.DisplayProgressBar(
                    $"Linking... ({i}/{allBehaviours.Length})",
                    behaviour == null ? "Skipping..." : behaviour.name,
                    (float)i / allBehaviours.Length
                );

                if (behaviour == null)
                    continue;

                // Set the value using Reflection, as it might be private
                FieldInfo prop = behaviour.GetType().GetField(name, BindingFlags.NonPublic | BindingFlags.Instance);
                prop.SetValue(behaviour, root);

                if (PrefabUtility.IsPartOfPrefabInstance(behaviour))
                    PrefabUtility.RecordPrefabInstancePropertyModifications(behaviour);
            }

            if (failCount > 0)
                EditorUtility.DisplayDialog(
                    "Linking report",
                    $"Failed to link {failCount} UdonBehaviour(s), check the console for details!",
                    "OK"
                );
            EditorUtility.ClearProgressBar();
        }
    }
}
