using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

using DecentM.EditorTools.SelfLocator;

namespace DecentM.EditorTools
{
    public static partial class EditorAssets
    {
        public static string SelfLocation = SelfLocatorAsset.LocateSelf();

        private static T GetAsset<T>(params string[] location) where T : UnityEngine.Object
        {
            string path = $"{SelfLocation}/{string.Join("/", location)}";
            return AssetDatabase.LoadAssetAtPath<T>(path);
        }
    }
}
