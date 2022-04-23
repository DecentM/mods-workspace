
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using DecentM;

[UdonBehaviourSyncMode(BehaviourSyncMode.Manual)]
public class PlayAudioSources : UdonSharpBehaviour
{
    [Tooltip("A list of AudioSources to play")]
    public AudioSource[] targets;
    [Tooltip("The default state of the AudioSources. If checked they will start playing when this UdonBehaviour starts.")]
    public bool defaultState = false;

    [Header("Settings")]
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

    private void Start()
    {
        for (int i = 0; i < this.targets.Length; i++)
        {
            if (this.defaultState == true)
            {
                this.targets[i].Play();
            } else
            {
                this.targets[i].Stop();
            }
        }
    }

    public override void Interact()
    {
        VRCPlayerApi player = Networking.LocalPlayer;
        bool isAllowed = this.lib.permissions.IsPlayerAllowed(player, this.masterOnly, this.isWhitelist, this.playerList);

        if (!isAllowed)
        {
            return;
        }

        if (this.targets[0].isPlaying)
        {
            this.BroadcastStop();
        } else
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
            this.targets[i].Play();
        }
    }

    public void Stop()
    {
        for (int i = 0; i < this.targets.Length; i++)
        {
            this.targets[i].Stop();
        }
    }
}
