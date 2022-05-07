
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;
using UnityEngine.UI;
using TMPro;

[UdonBehaviourSyncMode(BehaviourSyncMode.Manual)]
public class ToggleVariableSync : UdonSharpBehaviour
{
    public UdonBehaviour behaviour;
    public Text textMesh;
    public string variableName;

    private Toggle toggle;

    private void Start()
    {
        this.toggle = GetComponent<Toggle>();
        this.textMesh.text = this.variableName;
    }

    public void OnUpdate()
    {
        if (this.toggle.isOn)
        {
            SendCustomNetworkEvent(VRC.Udon.Common.Interfaces.NetworkEventTarget.All, nameof(this.ToggleOn));
        } else
        {
            SendCustomNetworkEvent(VRC.Udon.Common.Interfaces.NetworkEventTarget.All, nameof(this.ToggleOff));
        }

        this.DoToggle(this.toggle.isOn);
    }

    private void DoToggle(bool value)
    {
        this.behaviour.SetProgramVariable(this.variableName, value);
        this.textMesh.text = this.variableName;
    }

    public void ToggleOn()
    {
        // Prevent infinite loops by doing a sanity check here
        if (this.toggle.isOn)
        {
            return;
        }

        this.DoToggle(true);
        this.toggle.SetIsOnWithoutNotify(true);
    }

    public void ToggleOff()
    {
        // Prevent infinite loops by doing a sanity check here
        if (!this.toggle.isOn)
        {
            return;
        }

        this.DoToggle(false);
        this.toggle.SetIsOnWithoutNotify(false);
    }

    // Handle late joiners by firing ToggleOn/ToggleOff from the master
    public override void OnPlayerJoined(VRCPlayerApi player)
    {
        // This isn't a permission check, only one player (the master) must be running this function
        if (!Networking.LocalPlayer.isMaster)
        {
            return;
        }

        if (this.toggle.isOn)
        {
            SendCustomNetworkEvent(VRC.Udon.Common.Interfaces.NetworkEventTarget.All, nameof(this.ToggleOn));
        }
        else
        {
            SendCustomNetworkEvent(VRC.Udon.Common.Interfaces.NetworkEventTarget.All, nameof(this.ToggleOff));
        }
    }
}
