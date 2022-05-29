using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using DecentM;

[UdonBehaviourSyncMode(BehaviourSyncMode.Manual)]
public class RespawnObjects : UdonSharpBehaviour
{
    [Tooltip("A list of GameObjects to respawn. They will always have the same state")]
    public Transform[] targets;

    [UdonSynced]
    private Vector3[] respawnLocations;

    void Start()
    {
        if (Networking.GetOwner(this.gameObject) != Networking.LocalPlayer)
            return;

        this.SaveRespawnLocations();
    }

    private void SaveRespawnLocations()
    {
        this.respawnLocations = new Vector3[this.targets.Length];

        for (int i = 0; i < this.targets.Length; i++)
        {
            this.respawnLocations[i] = this.targets[i].transform.position;
        }

        this.RequestSerialization();
    }

    public override void Interact()
    {
        VRCPlayerApi player = Networking.LocalPlayer;

        this.ClaimTargetsOwnership();
        this.DoRespawn();
    }

    private void ClaimTargetsOwnership()
    {
        for (int i = 0; i < this.targets.Length; i++)
        {
            Transform target = this.targets[i];

            Networking.SetOwner(Networking.LocalPlayer, target.gameObject);
        }
    }

    private void DoRespawn()
    {
        for (int i = 0; i < this.targets.Length; i++)
        {
            Transform target = this.targets[i];
            Vector3 position = this.respawnLocations[i];

            target.SetPositionAndRotation(position, Quaternion.identity);
        }
    }
}
