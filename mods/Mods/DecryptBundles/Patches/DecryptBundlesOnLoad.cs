using System.IO;
using System.Threading;

using HarmonyLib;
using MelonLoader;
using UnityEngine;

using ABI_RC.Core.IO;

namespace DecentM.Mods.DecryptBundles.Patches
{
    class DecryptBundlesOnLoad
    {
        [HarmonyPatch(typeof(CVRObjectLoader), "InitiateLoadIntoWorld")]
        [HarmonyPrefix]
        static bool LoadPrefix(DownloadJob.ObjectType t, string objectId, byte[] b = null)
        {
            string fileName = "";

            switch (t)
            {
                case DownloadJob.ObjectType.ScriptBundle:
                    fileName += "scriptbundle";
                    break;

                case DownloadJob.ObjectType.Avatar:
                    fileName += "avatar";
                    break;

                case DownloadJob.ObjectType.Prop:
                    fileName += "prop";
                    break;

                case DownloadJob.ObjectType.Mission:
                    fileName += "mission";
                    break;

                case DownloadJob.ObjectType.World:
                    fileName += "world";
                    break;

                default:
                    fileName += "unknown";
                    break;
            }

            fileName += $"_{objectId}";

            path = $"{Application.dataPath}/Decrypted/{fileName}";
            
            if (b == null)
                data = new byte[0];
            else
                data = b;

            MelonLogger.Msg($"Saving decrypted bundle to {path}");

            Thread thread = new Thread(SaveData);
            thread.Start();

            return true;
        }

        private static string path = string.Empty;
        private static byte[] data = new byte[0];

        private static void SaveData()
        {
            Directory.CreateDirectory(Path.GetDirectoryName(path));
            File.WriteAllBytes(path, data);
        }
    }
}
