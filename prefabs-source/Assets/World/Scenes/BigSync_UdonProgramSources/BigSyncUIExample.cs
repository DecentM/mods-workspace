using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;
using TMPro;
using DecentM.BigSync;
using DecentM.BigSync.Plugins;

public class BigSyncUIExample : BigSyncPlugin
{
    public TextMeshProUGUI slot;

    public void OnSyncButtonPressed()
    {
        this.system.SyncString(this.slot.text);
    }
}
