
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;

namespace DecentM
{
    [UdonBehaviourSyncMode(BehaviourSyncMode.None)]
    public class Permissions : UdonSharpBehaviour
    {
        [Header("Settings")]
        [Tooltip("A list of players who have the same permissions as the instance master")]
        public PlayerList operators;

        public bool IsMaster(VRCPlayerApi player)
        {
            if (player.isMaster)
            {
                return true;
            }

            bool result = false;

            for (int i = 0; i < this.operators.players.Length; i++)
            {
                if (player.displayName == this.operators.players[i])
                {
                    result = true;
                }
            }

            return result;
        }

        public bool IsPlayerAllowed(VRCPlayerApi player, bool masterOnly, bool isWhitelist, string[] players)
        {
            if (!player.IsValid())
            {
                return false;
            }

            if (masterOnly && !this.IsMaster(player))
            {
                return false;
            }

            bool isAllowed = !isWhitelist;

            if (isWhitelist)
            {
                for (int i = 0; i < players.Length; i++)
                {
                    string whitelistedPlayer = players[i];

                    if (player.displayName == whitelistedPlayer)
                    {
                        isAllowed = true;
                    }
                }
            }
            else
            {
                for (int i = 0; i < players.Length; i++)
                {
                    string whitelistedPlayer = players[i];

                    if (player.displayName == whitelistedPlayer)
                    {
                        isAllowed = false;
                    }
                }
            }

            return isAllowed;
        }
    }
}
