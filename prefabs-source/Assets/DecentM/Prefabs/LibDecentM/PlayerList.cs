using UnityEngine;

namespace DecentM.Prefabs.PlayerList
{
    public class PlayerList : MonoBehaviour
    {
        [Header("Settings")]
        [Tooltip("A list of player names")]
        public string[] players;

        /* TODO: Restore this once we have LocalPlayer

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
        */
    }
}
