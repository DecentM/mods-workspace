
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;
using TMPro;

namespace DecentM.Chat
{
    [UdonBehaviourSyncMode(BehaviourSyncMode.None)]
    public class ChatPresence : UdonSharpBehaviour
    {
        public TextMeshProUGUI usernameSlot;

        [HideInInspector]
        public int senderId = -1;

        [HideInInspector]
        public int OnReceive_senderId = -1;
        public void OnReceive()
        {
            if (OnReceive_senderId == -1) return;

            this.senderId = OnReceive_senderId;

            this.RenderMessage();
        }

        private void RenderMessage()
        {
            string displayName = "<unknown>";

            VRCPlayerApi player = VRCPlayerApi.GetPlayerById(this.senderId);
            if (player != null && player.IsValid()) displayName = player.displayName;

            this.usernameSlot.text = displayName;
        }
    }
}
