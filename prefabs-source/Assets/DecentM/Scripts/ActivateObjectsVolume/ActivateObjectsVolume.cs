
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;

public class ActivateObjectsVolume : UdonSharpBehaviour
{
    [Header("Settings")]
    [Tooltip("A list of GameObjects to toggle. They will always have the same state")]
    public GameObject[] targets;
    [Tooltip("If true, the trigger will enable/disable targets when other players enter/exit it")]
    public bool global = false;

    public override void OnPlayerTriggerEnter(VRCPlayerApi player)
    {
        VRCPlayerApi localPlayer = Networking.LocalPlayer;

        if (player != localPlayer && !this.global)
        {
            return;
        }

        for (int i = 0; i < this.targets.Length; i++)
        {
            this.targets[i].SetActive(true);
        }
    }

    public override void OnPlayerTriggerExit(VRCPlayerApi player)
    {
        VRCPlayerApi localPlayer = Networking.LocalPlayer;

        if (player != localPlayer && !this.global)
        {
            return;
        }

        for (int i = 0; i < this.targets.Length; i++)
        {
            this.targets[i].SetActive(false);
        }
    }
}
