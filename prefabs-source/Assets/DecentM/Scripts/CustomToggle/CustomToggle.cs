
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

[UdonBehaviourSyncMode(BehaviourSyncMode.Manual)]
public class CustomToggle : UdonSharpBehaviour
{
    [Tooltip("The default state of the toggle")]
    public bool defaultState = false;
    [Tooltip("If true, the state will be synced to everyone")]
    public bool global = false;
    [Space]
    [Tooltip("The UdonBehaviour to send events to")]
    public UdonBehaviour behaviour;
    [Tooltip("The name of the event we send when toggling off")]
    public string offEvent = "ToggleOff";
    [Tooltip("The name of the event we send when toggling on")]
    public string onEvent = "ToggleOn";

    private bool state;

    void Start()
    {
        // Reset to the default state
        this.state = this.defaultState;
    }

    // Handle late joiners by firing ToggleOn/ToggleOff from the master
    public override void OnPlayerJoined(VRCPlayerApi player)
    {
        // This isn't a permission check, only one player (the master) must be running this function
        if (!Networking.LocalPlayer.isMaster || !this.global)
        {
            return;
        }

        // Skip sending a network update if the current state is the default one, to save on network resources
        if (this.state == this.defaultState)
        {
            return;
        }

        if (this.state == true)
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
        // Invert the state
        if (this.state == true)
        {
            this.ToggleOff();
        }
        else
        {
            this.ToggleOn();
        }

        if (this.global)
        {
            this.SyncState();
        }
    }

    public void ToggleOn()
    {
        this.state = true;

        this.behaviour.SendCustomEvent(this.onEvent);
    }

    public void ToggleOff()
    {
        this.state = false;

        this.behaviour.SendCustomEvent(this.offEvent);
    }

    private void SyncState()
    {
        SendCustomNetworkEvent(VRC.Udon.Common.Interfaces.NetworkEventTarget.All, this.state ? nameof(this.ToggleOn) : nameof(this.ToggleOff));
    }
}
