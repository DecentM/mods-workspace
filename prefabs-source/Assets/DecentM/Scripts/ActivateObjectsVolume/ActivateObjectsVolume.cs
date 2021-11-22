
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using System;

public class ActivateObjectsVolume : UdonSharpBehaviour
{
    [Header("Settings")]
    [Tooltip("A list of GameObjects to toggle. They will always have the same state")]
    public GameObject[] targets;
    [Tooltip("If true, the trigger will enable/disable targets when other players enter/exit it")]
    public bool global = false;
    [Tooltip("If checked, the list will function as a whitelist, otherwise it will function as a blacklist")]
    public bool isWhitelist = false;
    [Tooltip("A list of player names who can (or cannot) use this trigger")]
    public string[] players = new string[0];

    private bool IsPlayerAllowed(VRCPlayerApi player)
    {
        bool isAllowed = !this.isWhitelist;

        if (this.isWhitelist)
        {
            for (int i = 0; i < this.players.Length; i++)
            {
                string whitelistedPlayer = this.players[i];

                if (player.displayName == whitelistedPlayer)
                {
                    isAllowed = true;
                }
            }
        }
        else
        {
            for (int i = 0; i < this.players.Length; i++)
            {
                string whitelistedPlayer = this.players[i];

                if (player.displayName == whitelistedPlayer)
                {
                    isAllowed = false;
                }
            }
        }

        return isAllowed;
    }

    public override void OnPlayerTriggerEnter(VRCPlayerApi player)
    {
        if (!player.IsValid())
        {
            return;
        }

        VRCPlayerApi localPlayer = Networking.LocalPlayer;

        if (player != localPlayer && !this.global)
        {
            return;
        }

        bool isAllowed = this.IsPlayerAllowed(player);

        if (!isAllowed)
        {
            return;
        }

        for (int i = 0; i < this.targets.Length; i++)
        {
            this.targets[i].SetActive(true);
        }
    }

    public override void OnPlayerTriggerExit(VRCPlayerApi player)
    {
        if (!player.IsValid())
        {
            return;
        }

        VRCPlayerApi localPlayer = Networking.LocalPlayer;

        if (player != localPlayer && !this.global)
        {
            return;
        }

        bool isAllowed = this.IsPlayerAllowed(player);

        if (!isAllowed)
        {
            return;
        }

        for (int i = 0; i < this.targets.Length; i++)
        {
            this.targets[i].SetActive(false);
        }
    }
}
