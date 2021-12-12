
using UnityEngine;
using UnityEditor;
using DecentM.Subtitles;

[CustomEditor(typeof(InstructionRunner))]
public class EditorParserInspector : Editor
{
    private Compiler compiler = new Compiler();

    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        EditorGUILayout.HelpBox("This is a help box", MessageType.Info);

        if (GUILayout.Button("Test"))
        {
            Debug.Log("I pressed the dang button");
            this.compiler.Compile();
        }
    }
}
