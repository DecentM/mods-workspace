using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;
using System;
using UnityEngine.UI;

using DecentM.Collections;

namespace DecentM.Chat
{
    [UdonBehaviourSyncMode(BehaviourSyncMode.None)]
    public class messages : UdonSharpBehaviour
    {
        public ChatEvents events;
        public ChannelsStore channels;

        public int maxMessageCount = 150;

        public List messages;

        #region Message properties

        /*
         * Messsage structure:
         * object[]
         * 0 - id (string)
         * 1 - status (int)
         * 2 - text (string)
         * 3 - createdAt (DateTime)
         * 4 - updatedAt (DateTime | null)
         */

        private string GetMessageId(object[] message)
        {
            return (string)message[0];
        }

        private int GetMessageStatus(object[] message)
        {
            return (string)message[1];
        }

        /* Message ID structure
         * string
         * <packetId>_<channelId>_<senderId>_<increment>
         */

        private static string MessageIdSeparator = "_";

        private int GetIdPart(object[] message, int partIndex)
        {
            string id = this.GetMessageId(message);
            string[] parts = id.Split(MessageIdSeparator);

            if (parts.Length != 4)
                return -1;

            return parts[0];
        }

        private int GetMessagePacketId(object[] message)
        {
            return this.GetIdPart(message, 0);
        }

        private int GetMessageChannelId(object[] message)
        {
            return this.GetIdPart(message, 1);
        }

        private string GetMessageSenderId(object[] message)
        {
            return this.GetIdPart(message, 2);
        }

        private string GetMessageText(object[] message)
        {
            return (string)message[2];
        }

        private string GetMessageCreatedAt(object[] message)
        {
            return (string)message[3];
        }

        private string GetMessageUpdatedAt(object[] message)
        {
            return (string)message[4];
        }

        #endregion

        #region Message properties

        #endregion

        public object[][] GetAllMessages()
        {
            return this.messages.ToArray();
        }

        #region Store Utilities

        private void TrimMessages()
        {
            while (this.messages.Count > this.maxMessageCount)
            {
                this.RemoveMessageByIndex(this.messages.Count - 1);
            }
        }

        private int GetMessageIndexById(string id)
        {
            object[][] messages = this.messages.ToArray();

            // Travel in reverse because it's more likely people will delete recent messages rather than old ones,
            // so we will probably return quicker.
            for (int i = messages.Length - 1; i >= 0; i--)
            {
                if (this.messages[i] == id)
                    return i;
            }

            // If the id was found we won't get here. So we return -1 to show that it wasn't found.
            return -1;
        }

        private int GetMessageIndexByPacket(int packetId)
        {
            // Travel in reverse because it's more likely people will delete recent messages rather than old ones,
            // so we will probably return quicker.
            for (int i = initialPacketIdMap.Length - 1; i >= 0; i--)
            {
                if (this.initialPacketIdMap[i] == packetId)
                    return i;
            }

            // If the id was found we won't get here. So we return -1 to show that it wasn't found.
            return -1;
        }

        private GameObject GetMessageByIndex(int index)
        {
            // Check if the requested index is in bounds
            if (index < 0 || index >= this.messages.Count)
                return null;

            return this.messages[index];
        }

        private GameObject GetMessageById(string id)
        {
            int index = this.GetMessageIndexById(id);
            return this.GetMessageByIndex(index);
        }

        private GameObject GetMessageByPacket(int packetId)
        {
            int index = this.GetMessageIndexByPacket(packetId);
            return this.GetMessageByIndex(index);
        }

        private bool IsMessageSentByLocalPlayer(GameObject messageObject)
        {
            ChatMessage message = messageObject.GetComponent<ChatMessage>();

            if (message == null)
                return false;

            int senderId = (int)message.GetProgramVariable(nameof(message.senderId));

            return senderId == Networking.LocalPlayer.playerId;
        }

        public bool IsMessageSentByLocalPlayerByPacket(int packetId)
        {
            GameObject messageObject = this.GetMessageByPacket(packetId);

            if (messageObject == null)
                return false;

            return this.IsMessageSentByLocalPlayer(messageObject);
        }

        private GameObject CreateMessageObject(string name)
        {
            GameObject messageObject = VRCInstantiate(this.messageTemplate);

            messageObject.transform.SetParent(this.messageRoot);
            messageObject.name = name;
            messageObject.transform.SetPositionAndRotation(
                this.messageTemplate.transform.position,
                this.messageTemplate.transform.rotation
            );
            messageObject.transform.localScale = this.messageTemplate.transform.localScale;

            return messageObject;
        }

        private int GetLastIndexForPlayer(int playerId)
        {
            if (this.messages == null)
                this.messages = new GameObject[0];

            // Go backwards, so that the largest index comes first
            for (int i = this.messages.Count - 1; i >= 0; i--)
            {
                GameObject messageObject = this.messages[i];
                ChatMessage message = messageObject.GetComponent<ChatMessage>();

                // If message is null, some Udon issue occurred that removed the Message component. It's normally there and Udon code can't
                // even remove it so we just deal with the error by ignoring this Message instance.
                if (message == null)
                    continue;

                string fullMessageId = (string)message.GetProgramVariable("id");

                if (fullMessageId == null || fullMessageId == "")
                    continue;

                int[] idData = this.channels.DeserialiseMessageId(fullMessageId);

                if (!this.channels.MessageIdDataValid(idData))
                    continue;

                // Since we're going backwards, the most recent messages will come first. If we find the most recent message by the player,
                // we found the largest one.
                if (idData[1] == playerId)
                    return idData[2];
            }

            // If we haven't returned by now, we haven't found any message
            return -1;
        }

        private int GetNextIndexForPlayer(int playerId)
        {
            // The last ID is last message ID they sent, so we return the next one to get assigned
            return this.GetLastIndexForPlayer(playerId) + 1;
        }

        public string GenerateNextIdForPlayer(int playerId)
        {
            // The id of a message is: "<icremental number, unique per player>_<player name>"
            return this.channels.SerialiseMessageId(
                this.channel,
                this.GetNextIndexForPlayer(playerId),
                playerId
            );
        }

        #endregion

        #region Adding/Removing messages

        private void AddMessage(int initialPacketId, string id, string messageText, int senderId)
        {
            GameObject messageObject = this.CreateMessageObject(id);
            ChatMessage message = messageObject.GetComponent<ChatMessage>();

            message.SetProgramVariable(nameof(message.OnReceive_packetId), initialPacketId);
            message.SetProgramVariable(nameof(message.OnReceive_id), id);
            message.SetProgramVariable(nameof(message.OnReceive_channel), this.channel);
            message.SetProgramVariable(nameof(message.OnReceive_senderId), senderId);
            message.SetProgramVariable(nameof(message.OnReceive_message), messageText);
            message.SendCustomEvent(nameof(message.OnReceive));

            GameObject[] tmp = new GameObject[this.messages.Count + 1];
            Array.Copy(this.messages, tmp, this.messages.Count);
            tmp[tmp.Length - 1] = messageObject;
            this.messages = tmp;

            // Update the ID map to make it easy to reference messages by their ID instead of their index in the store
            if (this.indexIdMap == null)
                this.indexIdMap = new string[0];
            string[] indexTmp = new string[this.indexIdMap.Length + 1];
            Array.Copy(this.indexIdMap, indexTmp, this.indexIdMap.Length);
            indexTmp[indexTmp.Length - 1] = id;
            this.indexIdMap = indexTmp;

            // Update the packet map so that messages can be queried by packet id
            if (this.initialPacketIdMap == null)
                this.initialPacketIdMap = new int[0];
            int[] pktTmp = new int[this.initialPacketIdMap.Length + 1];
            Array.Copy(this.initialPacketIdMap, pktTmp, this.initialPacketIdMap.Length);
            pktTmp[pktTmp.Length - 1] = initialPacketId;
            this.initialPacketIdMap = pktTmp;

            // Make sure we're not going over the message limit
            this.TrimMessages();

            // Activate the message object so it's visible
            messageObject.SetActive(true);

            // Send an event to ChatEvents about this new message
            this.events.OnMessageAdded(message);
        }

        private void RemoveMessageByIndex(int index)
        {
            // The id was somehow not found, ignore the request
            if (index == -1)
                return;

            GameObject messageObject = this.GetMessageByIndex(index);

            // The store doesn't have a GameObject at the index, or the index was out of bounds
            if (messageObject == null)
                return;

            ChatMessage message = messageObject.GetComponent<ChatMessage>();

            if (message == null)
                return;

            // At this point we're committed to deleting the message, so we send the event before it's
            // actually deleted so its ID can still be read
            this.events.OnMessageDeleted(message);

            // Clean up the actual object from the scene
            messageObject.SetActive(false);
            Destroy(messageObject);

            // If there are no messages, it means this function was called in error, we just ignore the request
            if (this.messages.Count == 0)
            {
                return;
            }

            // If there's only one message, don't bother with shifting stuff around, just set data to
            // an empty array.
            if (this.messages.Count == 1)
            {
                this.messages = new GameObject[0];
                this.indexIdMap = new string[0];
                this.initialPacketIdMap = new int[0];
                return;
            }

            // Remove the item at the index from the message store and shrink it by one
            GameObject[] tmp = new GameObject[this.messages.Count - 1];
            // Copy all items except the index
            Array.Copy(this.messages, tmp, index);
            Array.Copy(
                this.messages,
                index + 1,
                tmp,
                index,
                this.messages.Count - 1 - index
            );
            this.messages = tmp;

            // Update the ID map the same way
            string[] indexTmp = new string[this.indexIdMap.Length - 1];
            Array.Copy(this.indexIdMap, indexTmp, index);
            Array.Copy(
                this.indexIdMap,
                index + 1,
                indexTmp,
                index,
                this.indexIdMap.Length - 1 - index
            );
            this.indexIdMap = indexTmp;

            // Update the packet map the same way
            int[] pktTmp = new int[this.initialPacketIdMap.Length - 1];
            Array.Copy(this.initialPacketIdMap, pktTmp, index);
            Array.Copy(
                this.initialPacketIdMap,
                index + 1,
                pktTmp,
                index,
                this.initialPacketIdMap.Length - 1 - index
            );
            this.initialPacketIdMap = pktTmp;
        }

        private void RemoveMessage(string id)
        {
            int index = this.GetMessageIndexById(id);

            this.RemoveMessageByIndex(index);
        }

        private void RemoveAllPlayerMessagesByPlayerId(int playerId)
        {
            string[] idsToRemove = new string[0];

            for (int i = 0; i < this.messages.Count; i++)
            {
                GameObject messageObject = this.GetMessageByIndex(i);

                if (messageObject == null)
                    continue;

                ChatMessage message = messageObject.GetComponent<ChatMessage>();

                if (message == null)
                    continue;

                int senderId = (int)message.GetProgramVariable(nameof(message.senderId));

                if (senderId != playerId)
                    continue;

                string[] idsTmp = new string[idsToRemove.Length + 1];
                Array.Copy(idsToRemove, idsTmp, idsToRemove.Length);
                idsTmp[idsTmp.Length - 1] = (string)message.GetProgramVariable(nameof(message.id));
                idsToRemove = idsTmp;
            }

            for (int i = 0; i < idsToRemove.Length; i++)
            {
                this.RemoveMessage(idsToRemove[i]);
            }
        }

        public bool PurgeByPlayerId(int playerId)
        {
            // Ignore incorrectly sent events
            if (playerId < 0)
                return false;

            this.RemoveAllPlayerMessagesByPlayerId(playerId);

            return true;
        }

        public bool AddMessageWithId(int packetId, string id, string message, int senderId)
        {
            if (packetId == -1 || id == "" || message == "" || senderId < 0)
                return false;

            this.AddMessage(packetId, id, message, senderId);

            return true;
        }

        public bool AddMessageWithoutId(int packetId, string message, int senderId)
        {
            if (packetId == -1 || message == "" || senderId < 0)
                return false;

            string id = this.GenerateNextIdForPlayer(senderId);

            return this.AddMessageWithId(packetId, id, message, senderId);
        }

        public bool RemoveMessageById(string id)
        {
            if (id == "")
                return false;

            this.RemoveMessage(id);

            return true;
        }

        #endregion

        #region Update message status

        private void ChangeMessageStatus(GameObject messageObject, int status)
        {
            ChatMessage message = messageObject.GetComponent<ChatMessage>();

            if (message == null)
                return;

            message.SetProgramVariable(nameof(message.OnStatusChange_status), status);
            message.SendCustomEvent(nameof(message.OnStatusChange));

            this.events.OnMessageChanged(message);
        }

        private void ChangeMessageStatusById(string id, int status)
        {
            GameObject messageObject = this.GetMessageById(id);

            if (messageObject == null)
                return;

            this.ChangeMessageStatus(messageObject, status);
        }

        private void ChangeMessageStatusByPacket(int packetId, int status)
        {
            GameObject messageObject = this.GetMessageByPacket(packetId);

            if (messageObject == null)
                return;

            this.ChangeMessageStatus(messageObject, status);
        }

        public bool OnMessageStatusChangeById(string id, int status)
        {
            // Ignore improperly called events
            if (id == "" || status == -1)
                return false;

            this.ChangeMessageStatusById(id, status);

            return true;
        }

        public void OnMessageStatusChangeByPacket(int packetId, int status)
        {
            // Ignore improperly called events
            if (packetId < 0 || status < 0)
                return;

            this.ChangeMessageStatusByPacket(packetId, status);
        }

        #endregion

        #region Update message colour

        private void ChangeMessageColour(GameObject messageObject, Color colour)
        {
            ChatMessage message = messageObject.GetComponent<ChatMessage>();

            if (message == null)
                return;

            message.SetProgramVariable(nameof(message.OnColourChange_colour), colour);
            message.SendCustomEvent(nameof(message.OnColourChange));
        }

        private void ChangeMessageColourById(string id, Color colour)
        {
            GameObject messageObject = this.GetMessageById(id);

            if (messageObject == null)
                return;

            this.ChangeMessageColour(messageObject, colour);
        }

        public bool OnMessageColourChangeById(string id, Color colour)
        {
            // Ignore improperly called events
            if (id == "" || colour == null)
                return false;

            this.ChangeMessageColourById(id, colour);

            return true;
        }

        #endregion
    }
}
