#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using UdonSharpEditor;
using VRC.Udon;
using VRC.Udon.Common;
using System.Collections.Immutable;
using VRC.Udon.Common.Interfaces;
using System;
using UdonSharp;

namespace DecentM.Keyboard
{
    [CustomEditor(typeof(SymbolStorage))]
    public class SymbolStorageInspector : Editor
    {
        public override void OnInspectorGUI()
        {
            if (UdonSharpGUI.DrawDefaultUdonSharpBehaviourHeader(target))
                return;
            EditorGUILayout.Space();

            if (
                GUILayout.Button(
                    new GUIContent(
                        "Link all keys to this Symbol Storage",
                        "Links all UdonBehaviours with 'symbolStorage' parameter to this object."
                    )
                )
            )
            {
                this.LinkAll();
            }

            EditorGUILayout.Space();
            base.OnInspectorGUI();
        }

        private IUdonVariable CreateUdonVariable(string symbolName, object value, System.Type type)
        {
            System.Type udonVariableType = typeof(UdonVariable<>).MakeGenericType(type);
            return (IUdonVariable)Activator.CreateInstance(udonVariableType, symbolName, value);
        }

        private void LinkAll()
        {
            UdonBehaviour[] allBehaviours = UnityEngine.Object.FindObjectsOfType<UdonBehaviour>();
            foreach (UdonBehaviour behaviour in allBehaviours)
            {
                var program = behaviour.programSource.SerializedProgramAsset.RetrieveProgram();
                ImmutableArray<string> exportedSymbolNames =
                    program.SymbolTable.GetExportedSymbols();
                foreach (string exportedSymbolName in exportedSymbolNames)
                {
                    if (!exportedSymbolName.Equals("symbolStorage"))
                        continue;

                    var variableValue = UdonSharpEditorUtility.GetBackingUdonBehaviour(
                        (UdonSharpBehaviour)target
                    );
                    System.Type symbolType = program.SymbolTable.GetSymbolType(exportedSymbolName);

                    if (
                        behaviour.publicVariables.TrySetVariableValue(
                            "symbolStorage",
                            variableValue
                        )
                    )
                        continue;

                    if (
                        !behaviour.publicVariables.TryAddVariable(
                            CreateUdonVariable(exportedSymbolName, variableValue, symbolType)
                        )
                    )
                    {
                        Debug.LogError(
                            $"Failed to set public variable '{exportedSymbolName}' value."
                        );
                    }

                    if (PrefabUtility.IsPartOfPrefabInstance(behaviour))
                    {
                        PrefabUtility.RecordPrefabInstancePropertyModifications(behaviour);
                    }
                }
            }
        }
    }
}
#endif
