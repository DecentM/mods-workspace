
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;
using DecentM;

[UdonBehaviourSyncMode(BehaviourSyncMode.Manual)]
public class TargetCycler : UdonSharpBehaviour
{
    [Header("References")]
    [Tooltip("This object will be teleported to each target")]
    public GameObject link;
    [Tooltip("The targets that the link object will teleport between")]
    public GameObject[] targets;

    [Header("Settings")]
    [Tooltip("If checked, the link location will be synced on every loop")]
    public bool global;
    [Tooltip("Defines how often we switch between the targets, in seconds")]
    public int interval = 4;

    [Header("LibDecentM")]
    [Tooltip("The LibDecentM object")]
    public LibDecentM lib;

    private int secondsPassed = 0;

    private int _chosenIndex = 0;
    private int chosenIndex
    {
        get => _chosenIndex;
        set
        {
            this._chosenIndex = value;
            this.UpdatePosition();
        }
    }

    private void UpdatePosition()
    {
        GameObject target = this.targets[this.chosenIndex];

        this.link.transform.SetPositionAndRotation(target.transform.position, target.transform.rotation);
    }

    private void Start()
    {
        this.lib.scheduling.OnEverySecond((UdonBehaviour) GetComponent(typeof(UdonBehaviour)));
    }

    public void OnSecondPassed()
    {
        this.secondsPassed++;

        // The interval is reached when the seconds passed is cleanly divisible by the interval
        if (this.secondsPassed % this.interval == 0)
        {
            this.Advance();
            this.secondsPassed = 0;
        }
    }

    private void Advance()
    {
        // Nudge the chosen index forward by one.
        if (this.chosenIndex + 1 >= this.targets.Length)
        {
            this.chosenIndex = 0;
            this.BroadcastReset();
        }
        else
        {
            this.chosenIndex++;
        }
    }

    private void BroadcastReset()
    {
        // If we're global, the master alone calls everyone's reset function.
        // If we're local, everyone calls their own reset function.
        if (this.global && Networking.LocalPlayer.isMaster)
        {
            SendCustomNetworkEvent(VRC.Udon.Common.Interfaces.NetworkEventTarget.All, nameof(this.Reset));
        } else if (!this.global)
        {
            this.Reset();
        }
    }

    public void Reset()
    {
        this.chosenIndex = 0;
    }
}
