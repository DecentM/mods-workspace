using DecentM.Mods.DecryptBundles.Patches;

namespace DecentM.Mods.DecryptBundles
{
    internal class Patcher
    {
        public static void Patch()
        {
            HarmonyLib.Harmony.CreateAndPatchAll(typeof(DecryptBundlesOnLoad));
        }
    }
}
