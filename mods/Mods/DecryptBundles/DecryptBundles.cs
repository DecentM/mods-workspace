using MelonLoader;

namespace DecentM.Mods.DecryptBundles
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
