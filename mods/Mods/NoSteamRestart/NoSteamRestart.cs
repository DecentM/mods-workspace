using MelonLoader;

namespace DecentM.Mods.NoSteamRestart
{
    public class Mod : MelonMod
    {
        public override void OnApplicationStart()
        {
            LoggerInstance.Msg("Patching...");
            Patcher.Patch();
        }
    }
}
