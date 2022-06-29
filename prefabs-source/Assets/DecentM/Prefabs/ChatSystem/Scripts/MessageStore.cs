using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;
using System;
using UnityEngine.UI;

using DecentM.Collections;

namespace DecentM.Chat
{
    public enum MessageStatus
    {
        Unknown,
        Created,
        Sending,
        Sent,
        FailedToSend,
        SentAcked,
        Received,
        ReceivedAcked,
    }

    [UdonBehaviourSyncMode(BehaviourSyncMode.None)]
    public class MessageStore : UdonSharpBehaviour
    {
        public ChatEvents events;
        public ChannelsStore channels;

        public int maxMessageCount = 150;

        // Map<string id, object[] message>
        public Map messages;

        /*
         * Messsage structure:
         * object[]
         * 0 - status (int)
         * 1 - text (string)
         * 2 - createdAt (DateTime)
         * 3 - updatedAt (DateTime | null)
         */

        private static int MessageObjectLength = 4;

        private const int MessageStatusIndex = 0;
        private const int MessageTextIndex = 1;
        private const int MessageCreatedAtIndex = 2;
        private const int MessageUpdatedAtIndex = 3;

        #region Message properties

        private MessageStatus GetMessageStatus(object[] message)
        {
            return (MessageStatus)message[MessageStatusIndex];
        }

        /* Message ID structure
         * string
         * <packetId>_<channelId>_<senderId>
         */

        private static char MessageIdSeparator = '_';

        private const int IdPacketIdIndex = 0;
        private const int IdChannelIdIndex = 1;
        private const int IdSenderIdIndex = 2;

        private int GetIdPart(string id, int partIndex)
        {
            string[] parts = id.Split(MessageIdSeparator);

            if (parts.Length != 4)
                return -1;

            string part = parts[partIndex];

            int result = -1;
            bool parsed = int.TryParse(part, out result);

            if (!parsed)
                return -1;

            return result;
        }

        private int GetPacketId(string id)
        {
            return this.GetIdPart(id, IdPacketIdIndex);
        }

        private int GetChannelId(string id)
        {
            return this.GetIdPart(id, IdChannelIdIndex);
        }

        private int GetSenderId(string id)
        {
            return this.GetIdPart(id, IdSenderIdIndex);
        }

        private string GetMessageText(object[] message)
        {
            return (string)message[MessageTextIndex];
        }

        private string GetMessageCreatedAt(object[] message)
        {
            return (string)message[MessageCreatedAtIndex];
        }

        private string GetMessageUpdatedAt(object[] message)
        {
            return (string)message[MessageUpdatedAtIndex];
        }

        private object[] SetMessageStatus(MessageStatus newStatus, object[] message)
        {
            object[] result = new object[MessageObjectLength];
            Array.Copy(message, result, Mathf.Min(message.Length, result.Length));

            result[MessageStatusIndex] = newStatus;
            return result;
        }

        private object[] SetMessageText(string newText, object[] message)
        {
            object[] result = new object[MessageObjectLength];
            Array.Copy(message, result, Mathf.Min(message.Length, result.Length));

            result[MessageTextIndex] = newText;
            return result;
        }

        private object[] SetMessageCreatedAt(DateTime newCreatedAt, object[] message)
        {
            object[] result = new object[MessageObjectLength];
            Array.Copy(message, result, Mathf.Min(message.Length, result.Length));

            result[MessageCreatedAtIndex] = newCreatedAt;
            return result;
        }

        private object[] SetMessageUpdatedAt(DateTime newUpdatedAt, object[] message)
        {
            object[] result = new object[MessageObjectLength];
            Array.Copy(message, result, Mathf.Min(message.Length, result.Length));

            result[MessageUpdatedAtIndex] = newUpdatedAt;
            return result;
        }

        #endregion

        #region Store Utilities

        public object[][] GetAllMessages()
        {
            return (object[][])this.messages.Values;
        }

        private void TrimMessages()
        {
            while (this.messages.Count > this.maxMessageCount)
            {
                string lastId = (string)this.messages.Keys[this.messages.Count - 1];
                this.messages.Remove(lastId);
                // TODO: Continue from here
            }
        }

        private int GetMessageIndexById(string id)
        {
            string[] ids = (string[])this.messages.Keys;

            // Travel in reverse because it's more likely people will access recent messages rather than old ones,
            // so we will probably return quicker.
            for (int i = ids.Length - 1; i >= 0; i--)
            {
                if (ids[i] == id)
                    return i;
            }

            // If the id was found we won't get here. So we return -1 to show that it wasn't found.
            return -1;
        }

        private int GetMessageIndexByPacket(int packetId)
        {
            string[] ids = (string[])this.messages.Keys;

            // Travel in reverse because it's more likely people will delete recent messages rather than old ones,
            // so we will probably return quicker.
            for (int i = ids.Length - 1; i >= 0; i--)
            {
                if (this.GetPacketId(ids[i]) == packetId)
                    return i;
            }

            // If the id was found we won't get here. So we return -1 to show that it wasn't found.
            return -1;
        }

        private object[] GetMessageById(string id)
        {
            return (object[])this.messages.Get(id);
        }

        private object[] GetMessageByPacket(int packetId)
        {
            string[] ids = (string[])this.messages.Keys;

            for (int i = 0; i < ids.Length; i++)
            {
                if (this.GetPacketId(ids[i]) == packetId)
                    return (object[])this.messages.Get(ids[i]);
            }

            return null;
        }

        private string GetIdByPacket(int packetId)
        {
            string[] ids = (string[])this.messages.Keys;

            for (int i = 0; i < ids.Length; i++)
            {
                if (this.GetPacketId(ids[i]) == packetId)
                    return ids[i];
            }

            return null;
        }

        private bool IsMessageSentByLocalPlayer(string id)
        {
            if (string.IsNullOrEmpty(id))
                return false;

            int senderId = this.GetSenderId(id);

            return senderId == Networking.LocalPlayer.playerId;
        }

        public bool IsMessageSentByLocalPlayerByPacket(int packetId)
        {
            string id = this.GetIdByPacket(packetId);

            if (id == null)
                return false;

            return this.IsMessageSentByLocalPlayer(id);
        }

        private object[] CreateMessageObject()
        {
            string id = string.Empty;
            MessageStatus status = MessageStatus.Created;
            string text = string.Empty;
            DateTime createdAt = DateTime.Now;

            return new object[] { id, status, text, createdAt, null, };
        }

        public string SerialiseMessageId(int packetId, int channelId, int senderId)
        {
            return string.Join(
                MessageIdSeparator.ToString(),
                new string[] { packetId.ToString(), channelId.ToString(), senderId.ToString() }
            );
        }

        public int[] DeserialiseMessageId(string id)
        {
            int packetId = -1;
            int channelId = -1;
            int senderId = -1;

            int[] result = new int[4];
            result[0] = -1; // packetId
            result[1] = -1; // channelId
            result[2] = -1; // senderId

            if (id == null)
                return result;

            string[] parts = id.Split(new char[] { MessageIdSeparator }, 4);
            int.TryParse(parts[0], out packetId);
            int.TryParse(parts[1], out channelId);
            int.TryParse(parts[2], out senderId);

            result[0] = packetId;
            result[1] = channelId;
            result[2] = senderId;

            return result;
        }

        public string GenerateNextIdForPlayer(int packetId, int channelId, int senderId)
        {
            return this.SerialiseMessageId(packetId, channelId, senderId);
        }

        #endregion

        #region Adding/Removing messages

        public void AddMessageWithId(int packetId, string id, string messageText, int senderId)
        {
            object[] message = this.CreateMessageObject();

            message = this.SetMessageStatus(MessageStatus.Created, message);
            message = this.SetMessageText(messageText, message);
            message = this.SetMessageCreatedAt(DateTime.Now, message);

            this.messages.Add(id, message);

            // Make sure we're not going over the message limit
            this.TrimMessages();

            // Send an event to ChatEvents about this new message
            this.events.OnMessageAdded(id, message);
        }

        private void RemoveMessageById(string id)
        {
            // The id was somehow not found, ignore the request
            if (string.IsNullOrEmpty(id))
                return;

            object[] message = (object[])this.messages.Get(id);

            if (message == null)
                return;

            // At this point we're committed to deleting the message, so we send the event before it's
            // actually deleted so its ID can still be read
            this.events.OnMessageDeleted(id, message);

            // If there are no messages, it means this function was called in error, we just ignore the request
            if (this.messages.Count == 0)
                return;

            this.messages.Remove(message);
        }

        public List idsToRemove;

        private void RemoveAllPlayerMessagesByPlayerId(int playerId)
        {
            this.idsToRemove.Clear();

            for (int i = 0; i < this.messages.Count; i++)
            {
                string id = (string)this.messages.Keys[i];

                if (string.IsNullOrEmpty(id))
                    continue;

                int senderId = this.GetSenderId(id);

                if (senderId != playerId)
                    continue;

                this.idsToRemove.Add(id);
            }

            for (int i = 0; i < idsToRemove.Count; i++)
            {
                this.RemoveMessageById((string)idsToRemove.ElementAt(i));
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

        public void AddMessageWithoutId(int packetId, string message, int senderId)
        {
            if (packetId == -1 || message == "" || senderId < 0)
                return;

            // TODO: Channels
            string id = this.GenerateNextIdForPlayer(packetId, 0, senderId);

            this.AddMessageWithId(packetId, id, message, senderId);
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
            object[] messageObject = this.GetMessageById(id);

            if (messageObject == null)
                return;

            this.SetMessageStatus(messageObject, status);
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
