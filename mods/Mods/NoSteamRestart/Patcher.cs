using DecentM.Mods.NoSteamRestart.Patches;

namespace DecentM.Mods.NoSteamRestart
{
    internal class Patcher
    {
        public static void Patch()
        {
            HarmonyLib.Harmony.CreateAndPatchAll(typeof(NoRestartSteamAPI));
        }
    }
}
