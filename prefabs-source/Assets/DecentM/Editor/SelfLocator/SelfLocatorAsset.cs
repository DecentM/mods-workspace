using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;

using DecentM.EditorTools;

namespace DecentM.EditorTools.SelfLocator
{
    public class SelfLocatingException : Exception
    {
        public SelfLocatingException() : base() { }
        public SelfLocatingException(string message) : base(message) { }
    }

    public sealed class SelfLocatorAsset : ScriptableObject
    {
        private static string SelfLocation = string.Empty;

        public static string LocateSelf()
        {
            if (!string.IsNullOrEmpty(SelfLocation)) return SelfLocation;

            List<string> files = Directory
                .GetFiles(Application.dataPath, $"*.{SelfLocatorId}", SearchOption.AllDirectories)
                .ToList();

            if (files.Count > 1) throw new SelfLocatingException("You have DecenM's prefabs imported more than once. Please delete all of DecenM's prefabs and import the unitypackage just once. You may move the imported folder after.");
            if (files.Count == 0) throw new SelfLocatingException("Could not find the self-locator file. Please delete all of DecenM's prefabs and import the unitypackage again.");

            string path = files[0];
            string[] parts = path.Split(new string[] { Application.dataPath }, StringSplitOptions.None);

            if (parts.Length != 2) throw new SelfLocatingException("Could not find the self-locator file, because the Editor data path cannot split the local file path in two. Something really weird is going on, please report this to DecentM!");
            string subPath = Path.GetDirectoryName(parts[1].Remove(0, 1));

            SelfLocation = $"Assets/{subPath}";

            return SelfLocation;
        }

        public const string SelfLocatorId = "decentm-self-locator";

        public static new SelfLocatorAsset CreateInstance(string input)
        {
            return ScriptableObject.CreateInstance<SelfLocatorAsset>();
        }
    }
}
