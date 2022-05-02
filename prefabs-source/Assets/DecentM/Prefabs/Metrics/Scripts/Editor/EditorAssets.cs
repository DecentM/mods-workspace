using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace DecentM.EditorTools
{
    public static partial class EditorAssets
    {
        private static T GetAsset<T>(params string[] location) where T : UnityEngine.Object
        {
            string path = $"Assets/DecentM/{string.Join("/", location)}";
            return AssetDatabase.LoadAssetAtPath<T>(path);
        }
    }
}

