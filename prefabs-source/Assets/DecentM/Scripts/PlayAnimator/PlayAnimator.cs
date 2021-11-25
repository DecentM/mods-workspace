
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;
using DecentM;

[UdonBehaviourSyncMode(BehaviourSyncMode.Manual)]
public class PlayAnimator : UdonSharpBehaviour
{
    [Header("Settings")]
    [Tooltip("A list of Animators to control")]
    public Animator[] targets;
    [Tooltip("Which animation to target in the controller")]
    public int layerIndex = 0;
    [Tooltip("The default state of the Animators. If checked they will start playing when this UdonBehaviour starts.")]
    public bool defaultState = false;
    [Tooltip("If true, the state will be synced to everyone")]
    public bool global = false;
    [Tooltip("Float between 0 and 1 - We will offset the animation by this much to count for network latency\nDoes nothing when `global` is unchecked")]
    public float latencyOffset = 0.01f;
    [Tooltip("If checked, the animation will be toggled on/off.\nIf unchecked, the animation will be played each time the button is pressed.")]
    public bool isToggle = false;

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
        for (int i = 0; i < this.targets.Length; i++)
        {
            Animator target = this.targets[i];

            if (this.defaultState == true)
            {
                this.PlayTarget(target);
            }
            else
            {
                this.StopTarget(target);
            }
        }
    }

    private void PlayTarget(Animator animator)
    {
        animator.enabled = true;
        int hash = animator.GetCurrentAnimatorStateInfo(this.layerIndex).fullPathHash;
        animator.Play(hash, this.layerIndex, this.latencyOffset);
    }

    private void StopTarget(Animator animator)
    {
        animator.enabled = false;
    }

    public override void Interact()
    {
        VRCPlayerApi player = Networking.LocalPlayer;
        bool isAllowed = this.lib.permissions.IsPlayerAllowed(player, this.masterOnly, this.isWhitelist, this.playerList);

        if (!isAllowed || this.targets[0] == null)
        {
            return;
        }

        if (this.targets[0].enabled && this.isToggle)
        {
            this.BroadcastStop();
        }
        else
        {
            this.BroadcastPlay();
        }
    }

    private void BroadcastPlay()
    {
        if (this.global)
        {
            SendCustomNetworkEvent(VRC.Udon.Common.Interfaces.NetworkEventTarget.All, nameof(this.Play));
        }
        else
        {
            this.Play();
        }
    }

    private void BroadcastStop()
    {
        if (this.global)
        {
            SendCustomNetworkEvent(VRC.Udon.Common.Interfaces.NetworkEventTarget.All, nameof(this.Stop));
        }
        else
        {
            this.Stop();
        }
    }

    public void Play()
    {
        for (int i = 0; i < this.targets.Length; i++)
        {
            this.PlayTarget(targets[i]);
        }
    }

    public void Stop()
    {
        for (int i = 0; i < this.targets.Length; i++)
        {
            this.StopTarget(targets[i]);
        }
    }
}
