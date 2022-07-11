using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;
using System;
using UnityEngine.UI;
using JetBrains.Annotations;

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

        private const int MessageObjectLength = 4;

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

        private const char MessageIdSeparator = '_';

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

        [PublicAPI]
        public int GetPacketId(string id)
        {
            return this.GetIdPart(id, IdPacketIdIndex);
        }

        [PublicAPI]
        public int GetChannelId(string id)
        {
            return this.GetIdPart(id, IdChannelIdIndex);
        }

        [PublicAPI]
        public int GetSenderId(string id)
        {
            return this.GetIdPart(id, IdSenderIdIndex);
        }

        private MessageStatus GetMessageObjectStatus(object[] message)
        {
            return (MessageStatus)message[MessageStatusIndex];
        }

        private string GetMessageObjectText(object[] message)
        {
            return (string)message[MessageTextIndex];
        }

        private DateTime GetMessageObjectCreatedAt(object[] message)
        {
            return (DateTime)message[MessageCreatedAtIndex];
        }

        private DateTime GetMessageObjectUpdatedAt(object[] message)
        {
            return (DateTime)message[MessageUpdatedAtIndex];
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

        [PublicAPI]
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

        [PublicAPI]
        public string GetMessageId(int packetId)
        {
            string[] ids = (string[])this.messages.Keys;

            for (int i = 0; i < ids.Length; i++)
            {
                if (this.GetPacketId(ids[i]) == packetId)
                    return ids[i];
            }

            return null;
        }

        [PublicAPI]
        public object[] GetMessage(string messageId)
        {
            return (object[])this.messages.Get(messageId);
        }

        [PublicAPI]
        public object[] GetMessage(int packetId)
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

        private object[] CreateMessageObject()
        {
            string id = string.Empty;
            MessageStatus status = MessageStatus.Created;
            string text = string.Empty;
            DateTime createdAt = DateTime.Now;

            return new object[] { id, status, text, createdAt, null, };
        }

        [PublicAPI]
        public string SerialiseMessageId(int packetId, int channelId, int senderId)
        {
            return string.Join(
                MessageIdSeparator.ToString(),
                new string[] { packetId.ToString(), channelId.ToString(), senderId.ToString() }
            );
        }

        [PublicAPI]
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

            string[] parts = id.Split(new char[] { MessageIdSeparator }, 3);
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

        [PublicAPI]
        public void AddMessage(string messageId, string messageText)
        {
            object[] message = this.CreateMessageObject();

            message = this.SetMessageStatus(MessageStatus.Created, message);
            message = this.SetMessageText(messageText, message);
            message = this.SetMessageCreatedAt(DateTime.Now, message);

            this.messages.Add(messageId, message);

            // Make sure we're not going over the message limit
            this.TrimMessages();

            // Send an event to ChatEvents about this new message
            this.events.OnMessageAdded(messageId, message);
        }

        [PublicAPI]
        public void RemoveMessage(string messageId)
        {
            // The id was somehow not found, ignore the request
            if (string.IsNullOrEmpty(messageId))
                return;

            object[] message = (object[])this.messages.Get(messageId);

            if (message == null)
                return;

            // At this point we're committed to deleting the message, so we send the event before it's
            // actually deleted so its ID can still be read
            this.events.OnMessageDeleted(messageId, message);

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
                this.RemoveMessage((string)idsToRemove.ElementAt(i));
            }
        }

        [PublicAPI]
        public bool PurgeByPlayerId(int playerId)
        {
            // Ignore incorrectly sent events
            if (playerId < 0)
                return false;

            this.RemoveAllPlayerMessagesByPlayerId(playerId);

            return true;
        }

        [PublicAPI]
        public void AddMessage(int packetId, string message, int senderId)
        {
            if (packetId == -1 || message == "" || senderId < 0)
                return;

            // TODO: Channels
            string id = this.GenerateNextIdForPlayer(packetId, 0, senderId);

            this.AddMessage(id, message);
        }

        #endregion

        #region Message Status

        [PublicAPI]
        public MessageStatus GetMessageStatus(string messageId)
        {
            object[] message = this.GetMessage(messageId);

            if (message == null)
                return MessageStatus.Unknown;

            return this.GetMessageObjectStatus(message);
        }

        [PublicAPI]
        public void ChangeMessageStatus(string messageId, MessageStatus status)
        {
            object[] message = this.GetMessage(messageId);

            if (message == null)
                return;

            object[] newMessage = this.SetMessageStatus(status, message);

            this.messages.Set(messageId, newMessage);
        }

        [PublicAPI]
        public void ChangeMessageStatus(int packetId, MessageStatus status)
        {
            string id = this.GetIdByPacket(packetId);

            this.ChangeMessageStatus(id, status);
        }

        #endregion

        #region Message Text

        [PublicAPI]
        public string GetMessageText(string messageId)
        {
            object[] message = this.GetMessage(messageId);

            if (message == null)
                return string.Empty;

            return this.GetMessageObjectText(message);
        }

        [PublicAPI]
        public void ChangeMessageText(string messageId, string text)
        {
            object[] message = this.GetMessage(messageId);

            if (message == null)
                return;

            object[] newMessage = this.SetMessageText(text, message);

            this.messages.Set(messageId, newMessage);
        }

        [PublicAPI]
        public void ChangeMessageText(int packetId, string text)
        {
            string id = this.GetIdByPacket(packetId);

            this.ChangeMessageText(id, text);
        }

        #endregion

        #region Message CreatedAt

        [PublicAPI]
        public DateTime GetMessageCreatedAt(string messageId)
        {
            object[] message = this.GetMessage(messageId);

            if (message == null)
                return DateTime.MinValue;

            return this.GetMessageObjectCreatedAt(message);
        }

        [PublicAPI]
        public void ChangeMessageCreatedAt(string messageId, DateTime createdAt)
        {
            object[] message = this.GetMessage(messageId);

            if (message == null)
                return;

            object[] newMessage = this.SetMessageCreatedAt(createdAt, message);

            this.messages.Set(messageId, newMessage);
        }

        [PublicAPI]
        public void ChangeMessageCreatedAt(int packetId, DateTime createdAt)
        {
            string id = this.GetIdByPacket(packetId);

            this.ChangeMessageCreatedAt(id, createdAt);
        }

        #endregion

        #region Message UpdatedAt

        [PublicAPI]
        public DateTime GetMessageUpdatedAt(string messageId)
        {
            object[] message = this.GetMessage(messageId);

            if (message == null)
                return DateTime.MinValue;

            return this.GetMessageObjectUpdatedAt(message);
        }

        [PublicAPI]
        public void ChangeMessageUpdatedAt(string messageId, DateTime updatedAt)
        {
            object[] message = this.GetMessage(messageId);

            if (message == null)
                return;

            object[] newMessage = this.SetMessageCreatedAt(updatedAt, message);

            this.messages.Set(messageId, newMessage);
        }

        [PublicAPI]
        public void ChangeMessageUpdatedAt(int packetId, DateTime updatedAt)
        {
            string id = this.GetIdByPacket(packetId);

            this.ChangeMessageUpdatedAt(id, updatedAt);
        }

        #endregion
    }
}
