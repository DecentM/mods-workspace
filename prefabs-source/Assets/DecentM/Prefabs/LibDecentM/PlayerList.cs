using UdonSharp;
using UnityEngine;
using VRC.SDKBase;

namespace DecentM
{
    [UdonBehaviourSyncMode(BehaviourSyncMode.None)]
    public class PlayerList : UdonSharpBehaviour
    {
        [Header("Settings")]
        [Tooltip("A list of player names")]
        public string[] players;

        public bool CheckPlayer(VRCPlayerApi player)
        {
            if (player == null || !player.IsValid())
                return false;

            foreach (string playerName in this.players)
            {
                if (playerName == player.displayName)
                    return true;
            }

            return false;
        }
    }
}
