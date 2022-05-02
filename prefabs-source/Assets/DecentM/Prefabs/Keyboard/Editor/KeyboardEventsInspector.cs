using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using UdonSharp;
using DecentM.EditorTools;

namespace DecentM.Keyboard
{
    [CustomEditor(typeof(KeyboardSystem))]
    public class KeyboardSystemInspector : Inspector
    {
        KeyboardSystem keyboard;

        private void OnDefaultLayoutChange(object layoutId)
        {
            this.SetUdonVariable(this.keyboard, "defaultLayout", layoutId);
        }

        public override void OnInspectorGUI()
        {
            this.keyboard = (KeyboardSystem) target;
            UdonSharpBehaviour events = this.keyboard.GetComponentInChildren<KeyboardEvents>();

            if (this.Button("Link all keyboard event listeners"))
            {
                this.LinkAllWithName(this.keyboard.gameObject, "events", events);
            }

            KeyboardLayout[] layouts = this.keyboard.GetComponentsInChildren<KeyboardLayout>();

            Rect position = GUILayoutUtility.GetRect(GUIContent.none, GUIStyle.none, GUILayout.Height(17.5f));
            List<EnumerableOption> options = new List<EnumerableOption>();

            foreach (KeyboardLayout layout in layouts)
            {
                string layoutName = (string) this.GetUdonVariable(layout, "layoutName");
                if (layoutName == null) continue;
                options.Add(new EnumerableOption(layoutName, layoutName));
            }

            this.Dropdown(position, "Default layout", options, this.GetUdonVariable(this.keyboard, "defaultLayout"), this.OnDefaultLayoutChange);
        }
    }
}
