using System;
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;
using TMPro;
using UnityEngine.UI;

namespace DecentM.Chat
{
    [UdonBehaviourSyncMode(BehaviourSyncMode.None)]
    public class TypingIndicatorStore : UdonSharpBehaviour
    {
        public ChatEvents events;

        public int worldCapacity = 64;
        public TextMeshProUGUI indicatorSlot;
        public Image typingIndicatorImage;

        // Array of bools where the index is the playerId and the value is the typing indicator
        private bool[] store;

        private void Start()
        {
            this.store = new bool[this.worldCapacity];

            // Initialise everyone as not typing
            for (int i = 0; i < this.store.Length; i++)
            {
                this.store[i] = false;
            }
        }

        private int GetTypingCount()
        {
            if (this.store == null)
                return 0;

            int count = 0;

            for (int i = 0; i < this.store.Length; i++)
            {
                if (this.store[i])
                    count++;
            }

            return count;
        }

        private int[] GetTypingPlayerIds()
        {
            int[] result = new int[0];

            if (this.store == null)
                return result;

            for (int i = 0; i < this.store.Length; i++)
            {
                if (this.store[i])
                {
                    int[] tmp = new int[result.Length + 1];
                    Array.Copy(result, tmp, result.Length);
                    tmp[tmp.Length - 1] = i;
                    result = tmp;
                }
            }

            return result;
        }

        private VRCPlayerApi[] GetTypingPlayers()
        {
            int[] ids = this.GetTypingPlayerIds();
            VRCPlayerApi[] result = new VRCPlayerApi[0];

            for (int i = 0; i < ids.Length; i++)
            {
                VRCPlayerApi player = VRCPlayerApi.GetPlayerById(ids[i]);
                if (player == null || !player.IsValid())
                    continue;

                VRCPlayerApi[] tmp = new VRCPlayerApi[result.Length + 1];
                Array.Copy(result, tmp, result.Length);
                tmp[tmp.Length - 1] = player;
                result = tmp;
            }

            return result;
        }

        private void SetTypingByPlayerId(int playerId, bool isTyping)
        {
            if (this.store == null)
                return;

            if (isTyping)
                this.events.OnPlayerTypingStart(playerId);
            else
                this.events.OnPlayerTypingStop(playerId);

            this.store[playerId] = isTyping;
            this.RenderIndicator();
        }

        public void OnPlayerTypingChange(int playerId, bool isTyping)
        {
            if (playerId < 0)
                return;

            this.SetTypingByPlayerId(playerId, isTyping);
        }

        public int renderBatchedMessageLimit = 4;

        private void RenderIndicator()
        {
            int count = this.GetTypingCount();
            string message = "";

            if (count == 0)
            {
                this.typingIndicatorImage.enabled = false;
                this.indicatorSlot.text = message;
                return;
            }
            else
            {
                this.typingIndicatorImage.enabled = true;
            }

            if (count < this.renderBatchedMessageLimit)
            {
                VRCPlayerApi[] typingPlayers = this.GetTypingPlayers();

                for (int i = 0; i < typingPlayers.Length; i++)
                {
                    message += $"{typingPlayers[i].displayName}";

                    if (i == typingPlayers.Length - 1)
                        message += "";
                    else if (i == typingPlayers.Length - 2)
                        message += ", and ";
                    else
                        message += ", ";
                }
            }
            else
            {
                message = $"{count} people";
            }

            if (count == 1)
            {
                message += " is typing...";
            }
            else
            {
                message += " are typing...";
            }

            this.indicatorSlot.text = message;
        }
    }
}
