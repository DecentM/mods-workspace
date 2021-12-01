
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;
using DecentM;

[UdonBehaviourSyncMode(BehaviourSyncMode.Manual)]
public class CustomEventVolume : UdonSharpBehaviour
{
    [Header("Settings")]
    [Tooltip("A list of GameObjects to send events to. They will receive the events at the same time.")]
    public Component[] targets;
    [Tooltip("If true, the trigger will enable/disable targets when other players enter/exit it")]
    public bool global = false;
    [Tooltip("The event to send when the player enters the trigger")]
    public string enterEventName = "";
    [Tooltip("The event to send when the player exits the trigger")]
    public string exitEventName = "";

    [Header("LibDecentM")]
    [Tooltip("The LibDecentM object")]
    public LibDecentM lib;
    [Tooltip("If checked, the list will function as a whitelist, otherwise it will function as a blacklist")]
    public bool isWhitelist = false;
    [Tooltip("If checked, only the instance master can use this trigger, and the player list will be ignored")]
    public bool masterOnly = false;
    [Tooltip("A list of players who can (or cannot) use this trigger")]
    public PlayerList playerList;

    private void Start()
    {
        MeshRenderer mesh = this.GetComponent<MeshRenderer>();

        this.lib.debugging.ApplyToMeshRenderer(mesh);
    }

    public override void OnPlayerTriggerEnter(VRCPlayerApi player)
    {
        if (player == null || !player.IsValid())
        {
            return;
        }

        VRCPlayerApi localPlayer = Networking.LocalPlayer;

        if (player != localPlayer && !this.global)
        {
            return;
        }

        bool isAllowed = this.lib.permissions.IsPlayerAllowed(player, this.masterOnly, this.isWhitelist, this.playerList);

        if (!isAllowed)
        {
            return;
        }

        for (int i = 0; i < this.targets.Length; i++)
        {
            // Skip over non-UdonBehaviours
            if (this.targets[i].GetType() != typeof(UdonBehaviour))
            {
                continue;
            }

            UdonBehaviour behaviour = (UdonBehaviour) this.targets[i];

            behaviour.SendCustomEvent(this.enterEventName);
        }
    }

    public override void OnPlayerTriggerExit(VRCPlayerApi player)
    {
        if (player == null || !player.IsValid())
        {
            return;
        }

        VRCPlayerApi localPlayer = Networking.LocalPlayer;

        if (player != localPlayer && !this.global)
        {
            return;
        }

        bool isAllowed = this.lib.permissions.IsPlayerAllowed(player, this.masterOnly, this.isWhitelist, this.playerList);

        if (!isAllowed)
        {
            return;
        }

        for (int i = 0; i < this.targets.Length; i++)
        {
            // Skip over non-UdonBehaviours
            if (this.targets[i].GetType() != typeof(UdonBehaviour))
            {
                continue;
            }

            UdonBehaviour behaviour = (UdonBehaviour) this.targets[i];

            behaviour.SendCustomEvent(this.exitEventName);
        }
    }
}
