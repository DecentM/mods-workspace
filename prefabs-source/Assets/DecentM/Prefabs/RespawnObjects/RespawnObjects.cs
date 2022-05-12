using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using DecentM;

[UdonBehaviourSyncMode(BehaviourSyncMode.Manual)]
public class RespawnObjects : UdonSharpBehaviour
{
    [Tooltip("A list of GameObjects to respawn. They will always have the same state")]
    public GameObject[] targets;

    [Tooltip(
        "An object that represents each respawn location. Set it to an empty game object if you don't want to visibly mark the respawn location."
    )]
    public GameObject respawnMarker;

    [Header("Settings")]
    [Tooltip("If true, the trigger will enable/disable targets when other players enter/exit it")]
    public bool global = false;

    [Tooltip(
        "How long to wait until the markers reset after the master respawns the objects. Set it larger if the spawn locations drift after the instance master leaves."
    )]
    public float respawnMarkerSetDelay = 1f;

    [Header("LibDecentM")]
    [Tooltip("The LibDecentM object")]
    public LibDecentM lib;

    [Tooltip(
        "If checked, the list will function as a whitelist, otherwise it will function as a blacklist"
    )]
    public bool isWhitelist = false;

    [Tooltip(
        "If checked, only the instance master can use this trigger, and the player list will be ignored"
    )]
    public bool masterOnly = false;

    [Tooltip("A list of players who can (or cannot) use this trigger")]
    public PlayerList playerList;

    // Auto-managed arrays to store respawn markers for each target
    private GameObject[] markers;

    void Start()
    {
        this.markers = new GameObject[this.targets.Length];
        this.respawnMarker.SetActive(false);

        this.MarkTargets();
    }

    private void MarkTargets()
    {
        // Clone the respawn marker to each object's starting location
        for (int i = 0; i < this.targets.Length; i++)
        {
            GameObject marker = VRCInstantiate(this.respawnMarker);
            GameObject target = this.targets[i];

            marker.SetActive(true);
            marker.transform.SetPositionAndRotation(
                target.transform.position,
                target.transform.rotation
            );
            this.markers[i] = marker;
        }
    }

    public void MasterRespawnTargets()
    {
        // The master needs to be the one to actually respawn objects, as they are the one with the correct markers.
        if (!Networking.LocalPlayer.isMaster)
        {
            return;
        }

        this.ClaimOwnershipTargets();
        this.DoRespawn();
    }

    public override void Interact()
    {
        VRCPlayerApi player = Networking.LocalPlayer;
        bool isAllowed = this.lib.permissions.IsPlayerAllowed(
            player,
            this.masterOnly,
            this.isWhitelist,
            this.playerList
        );

        if (!isAllowed)
        {
            return;
        }

        if (this.global)
        {
            SendCustomNetworkEvent(
                VRC.Udon.Common.Interfaces.NetworkEventTarget.All,
                nameof(this.MasterRespawnTargets)
            );
        }
        else
        {
            this.DoRespawn();
        }
    }

    private void ClaimOwnershipTargets()
    {
        for (int i = 0; i < this.targets.Length; i++)
        {
            GameObject target = this.targets[i];

            Networking.SetOwner(Networking.LocalPlayer, target);
        }
    }

    private void DoRespawn()
    {
        for (int i = 0; i < this.markers.Length; i++)
        {
            GameObject marker = this.markers[i];
            GameObject target = this.targets[i];

            target.transform.SetPositionAndRotation(
                marker.transform.position,
                marker.transform.rotation
            );
        }
    }
}
