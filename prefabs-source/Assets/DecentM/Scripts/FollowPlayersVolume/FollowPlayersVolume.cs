
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;

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
    [Tooltip("The link object will be spawned under the player's feet, and then offset by this vector.")]
    public Vector3 linkOffset = new Vector3(0, 0, 0);
    
    private VRCPlayerApi[] followingPlayers;
    private GameObject[] followingLinks;

    private GameObject SpawnLinkClone ()
    {
        GameObject playerLink = VRCInstantiate(this.link);
        playerLink.SetActive(true);
        playerLink.transform.SetPositionAndRotation(this.link.transform.position, this.link.transform.rotation);

        return playerLink;
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
        // This has the side effect of not pushing players when the array length has reached
        // the limit from this.trackLimit, since that's used for the length of the array
        for (int i = 0; i < this.followingPlayers.Length; i++)
        {
            if (this.followingPlayers[i] == null)
            {
                this.followingPlayers[i] = player;
                Debug.Log($"Pushed {player.displayName} on index {i}");

                GameObject playerLink = this.SpawnLinkClone();
                this.followingLinks[i] = playerLink;
                break;
            }
        }
    }

    private void UnfollowPlayer (VRCPlayerApi player)
    {
        Debug.Log($"Unfollowing {player.displayName}");

        int i;

        // Remove the player from the array by setting its index to null
        for (i = 0; i < this.followingPlayers.Length; i++)
        {
            if (this.followingPlayers[i] == player)
            {
                this.followingPlayers[i] = null;
                Debug.Log($"Removed {player.displayName} from index {i}");

                Destroy(this.followingLinks[i]);
                this.followingLinks[i] = null;
                // We don't break here, to remove all occurrences of the player in case we have a bug
                // where a player is duplicated within the array
            }
        }
    }

    private void Start()
    {
        // Initialise the array of currently followed players to the limit we're configured with
        this.followingPlayers = new VRCPlayerApi[this.trackLimit];
        this.followingLinks = new GameObject[this.trackLimit];

        // The link object is a template that we'll be instantiating in code later, so we disable the template
        // so users don't see it.
        if (this.link.activeSelf)
        {
            this.link.SetActive(false);
        }
    }

    private void FixedUpdate()
    {
        for (int i = 0; i < this.followingPlayers.Length; i++)
        {
            // This shouldn't happen, but just in case the consistency between links and players is broken,
            // we bail from updating the broken link here. Players can fix this by leaving and re-entering the trigger.
            if (this.followingLinks[i] == null)
            {
                return;
            }

            GameObject link = this.followingLinks[i];
            VRCPlayerApi player = this.followingPlayers[i];

            // If the player leaves the world while being tracked, we untrack them here
            if (!player.IsValid())
            {
                this.UnfollowPlayer(player);
                return;
            }

            // Move the link to the player
            link.transform.SetPositionAndRotation(player.GetPosition() + this.linkOffset, player.GetRotation());
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
