
using UdonSharp;
using UnityEngine;

namespace DecentM
{
    [UdonBehaviourSyncMode(BehaviourSyncMode.None)]
    public class PlayerList : UdonSharpBehaviour
    {
        [Header("Settings")]
        [Tooltip("A list of player names")]
        public string[] players;
    }
}
