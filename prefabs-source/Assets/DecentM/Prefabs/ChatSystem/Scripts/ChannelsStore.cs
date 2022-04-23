using System;
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

namespace DecentM.Chat
{
    [UdonBehaviourSyncMode(BehaviourSyncMode.None)]
    public class ChannelsStore : UdonSharpBehaviour
    {
        public GameObject messageStoreTemplate;
        public Transform messageStoreRoot;

        private GameObject[] channelStore;
        public ChatEvents events;

        private void Start()
        {
            this.channelStore = new GameObject[0];
            this.messageStoreTemplate.SetActive(false);
        }

        private char[] MessageIdSeparator = { '_' };

        public string SerialiseMessageId(int channel, int messageIndex, int playerId)
        {
            return $"{channel}{MessageIdSeparator[0]}{playerId}{MessageIdSeparator[0]}{messageIndex}";
        }

        public int[] DeserialiseMessageId(string id)
        {
            int channel = -1;
            int playerId = -1;
            int index = -1;

            int[] result = new int[3];
            result[0] = -1; // channel
            result[1] = -1; // playerId
            result[2] = -1; // index

            if (id == null) return result;

            string[] parts = id.Split(MessageIdSeparator, 3);
            int.TryParse(parts[0], out channel);
            int.TryParse(parts[1], out playerId);
            int.TryParse(parts[2], out index);

            result[0] = channel;
            result[1] = playerId;
            result[2] = index;

            return result;
        }

        public bool MessageIdDataValid(int[] idData)
        {
            if (idData == null || idData.Length != 3) return false;

            int channel = idData[0];
            int playerId = idData[1];
            int index = idData[2];

            if (channel < 0) return false;
            if (playerId < 0) return false;
            if (index < 0) return false;

            return true;
        }

        public bool IsMessageSentByLocalPlayerByPacket(int packetId)
        {
            bool result = false;

            // Look through all the channels to see it the local player sent the message with this packet id
            // Per channel, this is quick because it uses a map
            foreach (GameObject channelObject in this.channelStore)
            {
                if (channelObject == null) continue;

                MessageStore messageStore = channelObject.GetComponent<MessageStore>();

                if (messageStore == null) continue;

                if (messageStore.IsMessageSentByLocalPlayerByPacket(packetId))
                {
                    result = true;
                    break;
                };
            }

            return result;
        }

        private GameObject CreateMessageStoreObject(string name)
        {
            GameObject channelObject = VRCInstantiate(this.messageStoreTemplate);

            channelObject.transform.SetParent(this.messageStoreRoot);
            channelObject.name = name;
            channelObject.transform.SetPositionAndRotation(this.messageStoreTemplate.transform.position, this.messageStoreTemplate.transform.rotation);
            channelObject.transform.localScale = this.messageStoreTemplate.transform.localScale;

            return channelObject;
        }

        private int GetStoreIndexById(int id)
        {
            for (int i = 0; i < this.channelStore.Length; i++)
            {
                GameObject storeObject = this.channelStore[i];

                if (storeObject == null) continue;

                MessageStore messageStore = storeObject.GetComponent<MessageStore>();

                if (messageStore == null) continue;

                if (messageStore.channel == id) return i;
            }

            return -1;
        }

        private bool ChannelExists(int id)
        {
            foreach (GameObject storeObject in this.channelStore)
            {
                if (storeObject == null) continue;

                MessageStore messageStore = storeObject.GetComponent<MessageStore>();

                if (messageStore.channel == id) return true;
            }

            return false;
        }

        private GameObject GetMessageStore(int channel)
        {
            int index = this.GetStoreIndexById(channel);

            return this.channelStore[index];
        }

        private MessageStore GetMessageStoreByMessageId(string id)
        {
            // Instead of searching through stores, we use the fact that the first component
            // of the message ID is the channel
            int[] idData = this.DeserialiseMessageId(id);

            if (!this.MessageIdDataValid(idData)) return null;

            int channel = idData[0];

            // If we get a change request for a channel that doesn't exist, we had an error before that we
            // need to ignore here in order to avoid crashing
            if (!this.ChannelExists(channel)) return null;

            GameObject channelObject = this.GetMessageStore(channel);

            if (channelObject == null) return null;

            return channelObject.GetComponent<MessageStore>();
        }

        private void AddChannel(int id)
        {
            bool initialised = this.channelStore != null;

            GameObject channelObject = this.CreateMessageStoreObject($"ch_{id}");
            MessageStore messageStore = channelObject.GetComponent<MessageStore>();

            messageStore.OnCreate(id);

            // Something's gone terribly wrong if messageStore doesn't exist, as it's supposed to be attached to the template
            if (messageStore == null) return;

            if (initialised)
            {
                GameObject[] tmp = new GameObject[this.channelStore.Length + 1];
                Array.Copy(this.channelStore, tmp, this.channelStore.Length);
                tmp[tmp.Length - 1] = channelObject;
                this.channelStore = tmp;
            }
            else
            {
                GameObject[] tmp = new GameObject[1];
                tmp[0] = channelObject ;
                this.channelStore = tmp;
            }

            // Show the channel in the world (if any)
            channelObject.SetActive(true);

            // Send a notification at the end when we're sure everything's already set up
            this.events.OnChannelAdded(messageStore);
        }

        private void RemoveChannelByIndex(int index)
        {
            if (this.channelStore == null) return;

            GameObject channelObject = this.channelStore[index];

            if (channelObject == null) return;

            MessageStore messageStore = channelObject.GetComponent<MessageStore>();

            if (messageStore == null) return;

            // Send a notification about the deletion
            this.events.OnChannelDeleted(messageStore);

            // Remove it from the world
            channelObject.SetActive(false);
            Destroy(channelObject);

            // Don't bother dynamically deleting it if there's only one
            if (this.channelStore.Length == 1)
            {
                this.channelStore = new GameObject[0];
                return;
            }

            GameObject[] tmp = new GameObject[this.channelStore.Length - 1];
            Array.Copy(this.channelStore, tmp, index);
            Array.Copy(this.channelStore, index + 1, tmp, index, this.channelStore.Length - 1 - index);
            this.channelStore = tmp;
        }

        private void RemoveChannel(int id)
        {
            int index = this.GetStoreIndexById(id);

            if (index == -1) return;

            this.RemoveChannelByIndex(index);
        }

        #region Public API

        public ChatMessage[] GetAllMessages()
        {
            ChatMessage[] messages = new ChatMessage[0];

            foreach (GameObject channelObject in this.channelStore)
            {
                MessageStore messageStore = channelObject.GetComponent<MessageStore>();

                if (messageStore == null) continue;

                ChatMessage[] storeMessages = messageStore.GetAllMessages();

                ChatMessage[] tmp = new ChatMessage[messages.Length + storeMessages.Length];
                Array.Copy(messages, tmp, messages.Length);
                Array.Copy(storeMessages, 0, tmp, messages.Length, storeMessages.Length);
                messages = tmp;
            }

            return messages;
        }

        public void AddMessage(int packetId, int channel, int senderId, string message)
        {
            // Create the channel if it doesn't exist yet
            if (!this.ChannelExists(channel)) this.AddChannel(channel);

            GameObject channelObject = this.GetMessageStore(channel);

            if (channelObject == null) return;

            MessageStore messageStore = channelObject.GetComponent<MessageStore>();

            if (messageStore == null) return;

            messageStore.AddMessageWithoutId(packetId, message, senderId);
        }

        public void AddMessageWithId(int packetId, string id, int channel, int senderId, string message)
        {
            // Create the channel if it doesn't exist yet
            if (!this.ChannelExists(channel)) this.AddChannel(channel);

            GameObject channelObject = this.GetMessageStore(channel);

            if (channelObject == null) return;

            MessageStore messageStore = channelObject.GetComponent<MessageStore>();

            if (messageStore == null) return;

            messageStore.AddMessageWithId(packetId, id, message, senderId);
        }

        public string GenerateNextIdForPlayer(int channel, int playerId)
        {
            // Create the channel if it doesn't exist yet
            if (!this.ChannelExists(channel)) this.AddChannel(channel);

            GameObject channelObject = this.GetMessageStore(channel);

            if (channelObject == null) return "";

            MessageStore messageStore = channelObject.GetComponent<MessageStore>();

            if (messageStore == null) return "";

            return messageStore.GenerateNextIdForPlayer(playerId);
        }

        public void PurgeMessagesByPlayerId(int playerId)
        {
            foreach (GameObject storeObject in this.channelStore)
            {
                if (storeObject == null) continue;

                MessageStore messageStore = storeObject.GetComponent<MessageStore>();

                if (messageStore == null) return;

                messageStore.PurgeByPlayerId(playerId);
            }
        }

        public void ChangeMessageStatusByPacket(int packetId, int status)
        {
            foreach (GameObject storeObject in this.channelStore)
            {
                if (storeObject == null) continue;

                MessageStore messageStore = storeObject.GetComponent<MessageStore>();

                if (messageStore == null) return;

                // MessageStore wil ignore this request if there's no message in it with this packet ID
                messageStore.OnMessageStatusChangeByPacket(packetId, status);
            }
        }

        public void ChangeMessageStatusById(string id, int status)
        {
            MessageStore messageStore = this.GetMessageStoreByMessageId(id);

            if (messageStore == null) return;

            messageStore.OnMessageStatusChangeById(id, status);
        }

        public void ChangeMessageColourById(string id, Color colour)
        {
            MessageStore messageStore = this.GetMessageStoreByMessageId(id);

            if (messageStore == null) return;

            messageStore.OnMessageColourChangeById(id, colour);
        }

        #endregion
    }
}
