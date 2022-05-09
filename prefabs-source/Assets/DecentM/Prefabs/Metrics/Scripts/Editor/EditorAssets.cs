using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

using DecentM.EditorTools.SelfLocator;

namespace DecentM.EditorTools
{
    public static partial class EditorAssets
    {
        private static T GetAsset<T>(params string[] location) where T : UnityEngine.Object
        {
            string baseLocation = SelfLocatorAsset.LocateSelf();

            string path = $"{baseLocation}/{string.Join("/", location)}";
            return AssetDatabase.LoadAssetAtPath<T>(path);
        }
    }
}
