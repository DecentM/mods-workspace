
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;
using DecentM;

[UdonBehaviourSyncMode(BehaviourSyncMode.Manual)]
public class RespawnObjects : UdonSharpBehaviour
{
    [Tooltip("A list of GameObjects to respawn. They will always have the same state")]
    public GameObject[] targets;
    [Tooltip("An object that represents each respawn location. Set it to an empty game object if you don't want to visibly mark the respawn location.")]
    public GameObject respawnMarker;

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

    // Auto-managed arrays to store respawn markers for each target
    private GameObject[] markers;

    void Start()
    {
        if (!Networking.LocalPlayer.isMaster)
        {
            return;
        }

        this.markers = new GameObject[this.targets.Length];

        // Clone the respawn marker to each object's starting location
        for (int i = 0; i < this.targets.Length; i++)
        {
            GameObject marker = VRCInstantiate(this.respawnMarker);
            GameObject target = this.targets[i];

            marker.transform.SetPositionAndRotation(target.transform.position, target.transform.rotation);
            this.markers[i] = marker;
        }
    }

    public override void Interact()
    {
        VRCPlayerApi player = Networking.LocalPlayer;
        bool isAllowed = this.permissions.IsPlayerAllowed(player, this.masterOnly, this.isWhitelist, this.playerList ? this.playerList.players : new string[0]);

        if (!isAllowed)
        {
            return;
        }

        if (this.global)
        {
            SendCustomNetworkEvent(VRC.Udon.Common.Interfaces.NetworkEventTarget.All, "Respawn");
        }
        else
        {
            this.Respawn();
        }
    }

    public void Respawn()
    {
        for (int i = 0; i < this.markers.Length; i++)
        {
            GameObject marker = this.markers[i];
            GameObject target = this.targets[i];

            target.transform.SetPositionAndRotation(marker.transform.position, marker.transform.rotation);
        }
    }
}
