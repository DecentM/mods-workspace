using System;
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;
using JetBrains.Annotations;

using UNet;

namespace DecentM.Chat
{
    [UdonBehaviourSyncMode(BehaviourSyncMode.None)]
    public class ChatSystem : NetworkEventsListener
    {
        public LibDecentM lib;

        [Space]
        public MessageStore messages;

        public Color debugColour;

        private void DebugLog(string message)
        {
            if (!this.lib.debugging.isDebugging)
                return;

            int packetId = Mathf.FloorToInt(DateTimeOffset.Now.ToUnixTimeMilliseconds());
            string messageId = this.messages.SerialiseMessageId(packetId, 0, 0);
            this.messages.AddMessage(messageId, message);
        }

        private const string SendMessageCommand = "SM";
        private const string AckMessageCommand = "ACK";

        public override void OnReceived(
            int sender,
            byte[] dataBuffer,
            int index,
            int length,
            int packetId
        )
        {
            // Ignore messages coming from ourselves
            if (sender == Networking.LocalPlayer.playerId)
                return;

            string value = this.reader.ReadUTF8String(length, dataBuffer, index);
            string command = value.Split(null, 2)[0];
            string arguments = value.Split(null, 2)[1];

            this.DebugLog($"OnReceived: {value}");

            // Network messages are strings, where the command and its arguments are separated by a space
            switch (command)
            {
                // SendMessageCommand - int channel, string message
                case SendMessageCommand:
                {
                    int channel = -1;
                    string message = "";

                    string[] args = arguments.Split(null, 2);
                    bool channelParsed = int.TryParse(args[0], out channel);
                    message = args[1];

                    if (!channelParsed)
                        break;

                    this.HandleIncomingMessage(packetId, channel, sender, message);
                    break;
                }
                // AckMessageCommand: int id
                case AckMessageCommand:
                {
                    string id = arguments;

                    this.HandleMessageAck(id);
                    break;
                }
                default:
                    // Ignore messages we don't recognise
                    break;
            }
        }

        private void HandleIncomingMessage(
            int packetId,
            int channelId,
            int senderId,
            string message
        )
        {
            // Add it locally
            string id = this.messages.SerialiseMessageId(packetId, channelId, senderId);
            this.messages.AddMessage(id, message);

            // We don't care about the ack status, as the message will stay on "Sent, but not ack'd" status
            // for the sender if it doesn't receive a single ack
            // (plus, there was a bug with the first ack overriding further ones in the store because I don't want to store
            // more ID maps 8l)
            // this.SendCommandTarget(AckMessageCommand, senderId, id);

            // Send ack
            // Returns -1 if it failed, but we don't care since not receiving an ack will keep the message in
            // "Sent, but not ackd" status for the sender
            int ackPacketId = this.SendTarget(false, AckMessageCommand, senderId);

            MessageStatus status =
                ackPacketId == -1 ? MessageStatus.Received : MessageStatus.ReceivedAcked;

            // Set the message's status to Received, ackd or not, depending on the outcome of the send
            this.messages.ChangeMessageStatus(id, status);
        }

        private void HandleMessageAck(string id)
        {
            this.DebugLog($"Received ack for message {id}");
            this.messages.ChangeMessageStatus(id, MessageStatus.SentAcked);
        }

        private void HandleOutgoingMessage(int channel, string message)
        {
            this.DebugLog($"{SendMessageCommand} {channel} {message}");
            int packetId = this.SendAll(false, $"{SendMessageCommand} {channel} {message}");

            string id = this.messages.SerialiseMessageId(
                packetId,
                channel,
                Networking.LocalPlayer.playerId
            );

            // Add the message locally
            // TODO: Check what the consequences of multiples of the same packetId are! It will probably just use the newest message
            // with that packet ID, which is probably fine, as this usually only happens for messages that failed to send.
            this.messages.AddMessage(id, message);

            // The packet id will be -1 if it failed to send
            MessageStatus status =
                packetId == -1 ? MessageStatus.FailedToSend : MessageStatus.Sending;

            this.messages.ChangeMessageStatus(id, status);
        }

        public void OnSendMessage(string text, int channelId = 0)
        {
            this.HandleOutgoingMessage(channelId, text);
        }

        public override void OnSendComplete(int packetId, bool succeed)
        {
            this.DebugLog($"SendComplete {packetId} {succeed}");

            string messageId = this.messages.GetMessageId(packetId);
            int senderId = this.messages.GetSenderId(messageId);

            // Check if the message that belongs to this packet was sent by us
            bool sentByLocalPlayer = senderId == Networking.LocalPlayer.playerId;

            if (!sentByLocalPlayer)
                return;

            MessageStatus status = succeed ? MessageStatus.Sent : MessageStatus.FailedToSend;
            this.messages.ChangeMessageStatus(messageId, status);
        }

        public override void OnPlayerLeft(VRCPlayerApi player)
        {
            if (
                player == null
                || Networking.LocalPlayer == null
                || player.playerId == Networking.LocalPlayer.playerId
            )
                return;

            this.messages.PurgeByPlayerId(player.playerId);
        }

        // TODO: replay chat history for joining players?
    }
}
