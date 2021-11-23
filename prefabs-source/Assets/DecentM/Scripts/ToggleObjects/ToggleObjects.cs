
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;
using DecentM;

[UdonBehaviourSyncMode(BehaviourSyncMode.Manual)]
public class ToggleObjects : UdonSharpBehaviour
{
    [Header("References")]
    [Tooltip("The Permissions object from LibDecentM")]
    public Permissions permissions;

    [Header("Settings")]
    [Tooltip("If true, the trigger will enable/disable targets when other players enter/exit it")]
    public bool global = false;
    [Tooltip("If checked, the list will function as a whitelist, otherwise it will function as a blacklist")]
    public bool isWhitelist = false;
    [Tooltip("If checked, only the instance master can use this trigger, and the player list will be ignored")]
    public bool masterOnly = false;
    [Tooltip("A list of players who can (or cannot) use this trigger")]
    public PlayerList playerList;
    [Space]
    [Tooltip("The initial state of all of the targets - This will be applied when this UdonBehaviour starts")]
    public bool defaultState = false;
    [Space]
    [Tooltip("A list of GameObjects to toggle. They will always have the same state")]
    public GameObject[] targets;

    void Start()
    {
        // Reset all targets to the default state
        this.SetActiveAll(this.defaultState);
    }

    public override void Interact ()
    {
        VRCPlayerApi player = Networking.LocalPlayer;
        bool isAllowed = this.permissions.IsPlayerAllowed(player, this.masterOnly, this.isWhitelist, this.playerList.players);

        if (!isAllowed)
        {
            return;
        }

        if (this.global)
        {
            SendCustomNetworkEvent(VRC.Udon.Common.Interfaces.NetworkEventTarget.All, "Toggle");
        } else
        {
            this.Toggle();
        }
    }

    // Handle late joiners by firing ToggleOn/ToggleOff from the master
    public override void OnPlayerJoined(VRCPlayerApi player)
    {
        // This isn't a permission check, only one player (the master) must be running this function
        if (!Networking.LocalPlayer.isMaster || !this.global)
        {
            return;
        }

        bool isActive = this.targets[0].activeSelf;

        if (isActive)
        {
            SendCustomNetworkEvent(VRC.Udon.Common.Interfaces.NetworkEventTarget.All, "ToggleOn");
        } else
        {
            SendCustomNetworkEvent(VRC.Udon.Common.Interfaces.NetworkEventTarget.All, "ToggleOff");
        }
    }

    public void Toggle ()
    {
        // All targets should be the same at this point, so use the first one as the source of truth
        // This way we don't need to store an internal variable
        bool isActive = this.targets[0].gameObject.activeSelf;

        if (isActive)
        {
            this.ToggleOn();
        } else
        {
            this.ToggleOff();
        }
    }

    public void ToggleOn()
    {
        this.SetActiveAll(true);
    }

    public void ToggleOff()
    {
        this.SetActiveAll(false);
    }

    private void SetActiveAll(bool value)
    {
        for (int i = 0; i < this.targets.Length; i++)
        {
            this.targets[i].SetActive(value);
        }
    }
}
