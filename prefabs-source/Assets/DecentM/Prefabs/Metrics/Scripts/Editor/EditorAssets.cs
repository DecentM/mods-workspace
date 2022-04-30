using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace DecentM.EditorTools
{
    public static partial class EditorAssets
    {
        private static T GetAsset<T>(string location) where T : UnityEngine.Object
        {
            return AssetDatabase.LoadAssetAtPath<T>(location);
        }
    }
}

