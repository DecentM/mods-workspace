using System.IO;
using UnityEditor;
using UnityEditor.Experimental.AssetImporters;
using UnityEngine;

using DecentM.Prefabs.Subtitles;

namespace DecentM.Prefabs.VideoPlayer.EditorTools.Importers
{
    [CustomEditor(typeof(SubtitleImporter))]
    public class SubtitleImporterInspector : ScriptedImporterEditor
    {
        string output = string.Empty;

        public override void OnInspectorGUI()
        {
            if (!string.IsNullOrEmpty(output))
            {
                EditorGUILayout.SelectableLabel(
                    this.output,
                    EditorStyles.textArea,
                    GUILayout.Height(512)
                );
                base.ApplyRevertGUI();
                return;
            }

            SubtitleImporter importer = (SubtitleImporter)target;

            string contents = File.ReadAllText(importer.assetPath);
            Compiler.CompilationResult compiled = SubtitleCompiler.Compile(
                contents,
                Path.GetExtension(importer.assetPath),
                SubtitleFormat.Vsi
            );

            if (compiled.errors.Count > 0)
            {
                EditorGUILayout.HelpBox(
                    $"{compiled.errors.Count} errors found while compiling.",
                    MessageType.Warning
                );

                string errors = "";

                foreach (CompilationResultError error in compiled.errors)
                {
                    errors += $"{error.value}\n";
                }

                EditorGUILayout.HelpBox(errors, MessageType.Warning);
            }
            else
            {
                this.output = compiled.output;
            }

            EditorGUILayout.SelectableLabel(contents, EditorStyles.textArea, GUILayout.Height(512));

            base.ApplyRevertGUI();
        }
    }
}
