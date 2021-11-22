
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;
using System.Collections.Generic;
using System;

[UdonBehaviourSyncMode(BehaviourSyncMode.Manual)]
public class FollowPlayersVolume : UdonSharpBehaviour
{
    [Header("References")]
    [Tooltip("The game object to follow players with")]
    public GameObject link;

    [Header("Settings")]
    [Tooltip("How many people to track at once - If exceeded, players who step on later will not be tracked until someone else leaves the volume")]
    public int trackLimit = 5;
    [Tooltip("If false, it will only track the local player, regardless of the track limit setting")]
    public bool global = true;
    
    private VRCPlayerApi[] followingPlayers;

    private void Start()
    {
        // Initialise the array of currently followed players to the limit we're configured with
        this.followingPlayers = new VRCPlayerApi[this.trackLimit];

        // The link object is a template that we'll be instantiating in code later, so we disable the template
        // so users don't see it.
        if (this.link.activeSelf)
        {
            this.link.SetActive(false);
        }
    }

    private bool IsFollowingPlayer (VRCPlayerApi player)
    {
        // Search for the player
        bool result = false;

        for (int i = 0; i < this.followingPlayers.Length; i++)
        {
            if (this.followingPlayers[i] == player)
            {
                result = true;
            }
        }

        return result;
    }

    private void FollowPlayer (VRCPlayerApi player)
    {
        Debug.Log($"Following {player.displayName}");
        
        // Insert the player into the first empty index
        for (int i = 0; i < this.followingPlayers.Length; i++)
        {
            if (this.followingPlayers[i] == null)
            {
                this.followingPlayers[i] = player;
                Debug.Log($"Pushed {player.displayName} on index {i}");
                break;
            }
        }
    }

    private void UnfollowPlayer (VRCPlayerApi player)
    {
        Debug.Log($"Unfollowing {player.displayName}");

        // Remove the player from the array by setting its index to null
        for (int i = 0; i < this.followingPlayers.Length; i++)
        {
            if (this.followingPlayers[i] == player)
            {
                this.followingPlayers[i] = null;
                Debug.Log($"Removed {player.displayName} from index {i}");
                // We don't break here, to remove all occurrences of the player in case we have a bug
                // where a player is duplicated within the array
            }
        }
    }

    public override void OnPlayerTriggerEnter (VRCPlayerApi player)
    {
        // We don't do anything if we're already following someone or the player isn't valid
        if (!player.IsValid() || this.IsFollowingPlayer(player))
        {
            return;
        }

        this.FollowPlayer(player);
    }

    public override void OnPlayerTriggerExit(VRCPlayerApi player)
    {
        // No reason to unfollow a player if we're not already following them
        if (!player.IsValid() || !this.IsFollowingPlayer(player))
        {
            return;
        }

        this.UnfollowPlayer(player);
    }
}
