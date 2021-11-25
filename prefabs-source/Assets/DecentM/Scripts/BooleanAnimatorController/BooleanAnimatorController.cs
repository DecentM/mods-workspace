
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using DecentM;

[UdonBehaviourSyncMode(BehaviourSyncMode.Manual)]
public class BooleanAnimatorController : UdonSharpBehaviour
{
    [Header("Settings")]
    [Tooltip("The Animator to control")]
    public Animator animator;
    [Tooltip("Which animation to target in the controller")]
    public int layerIndex = 0;
    [Tooltip("The name of the parameter to toggle")]
    public string parameterName = "";

    [Space]
    [Tooltip("The default state of the Animators. If checked they will start playing when this UdonBehaviour starts.")]
    public bool defaultState = false;
    [Tooltip("If true, the state will be synced to everyone")]
    public bool global = false;

    [Header("LibDecentM")]
    [Tooltip("The LibDecentM object")]
    public LibDecentM lib;
    [Tooltip("If checked, the list will function as a whitelist, otherwise it will function as a blacklist")]
    public bool isWhitelist = false;
    [Tooltip("If checked, only the instance master can use this trigger, and the player list will be ignored")]
    public bool masterOnly = false;
    [Tooltip("A list of players who can (or cannot) use this trigger")]
    public PlayerList playerList;

    void Start()
    {
        // Reset all targets to the default state
        this.SetActive(this.defaultState);
    }

    public override void Interact()
    {
        VRCPlayerApi player = Networking.LocalPlayer;
        bool isAllowed = this.lib.permissions.IsPlayerAllowed(player, this.masterOnly, this.isWhitelist, this.playerList);

        if (!isAllowed)
        {
            return;
        }

        if (this.global)
        {
            SendCustomNetworkEvent(VRC.Udon.Common.Interfaces.NetworkEventTarget.All, nameof(this.Toggle));
        }
        else
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

        bool isActive = this.animator.GetBool(this.parameterName);

        // Skip sending a network update if the current state is the default one, to save on network resources
        if (isActive == this.defaultState)
        {
            return;
        }

        if (isActive)
        {
            SendCustomNetworkEvent(VRC.Udon.Common.Interfaces.NetworkEventTarget.All, nameof(this.ToggleOn));
        }
        else
        {
            SendCustomNetworkEvent(VRC.Udon.Common.Interfaces.NetworkEventTarget.All, nameof(this.ToggleOff));
        }
    }

    public void Toggle()
    {
        // All targets should be the same at this point, so use the first one as the source of truth
        // This way we don't need to store an internal variable
        bool isActive = this.animator.GetBool(this.parameterName);

        if (isActive)
        {
            this.ToggleOff();
        }
        else
        {
            this.ToggleOn();
        }
    }

    public void ToggleOn()
    {
        this.SetActive(true);
    }

    public void ToggleOff()
    {
        this.SetActive(false);
    }

    private void SetActive(bool value)
    {
        this.animator.SetBool(this.parameterName, value);
    }
}
