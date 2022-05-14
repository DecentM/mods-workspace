using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

using DecentM.Network;

namespace DecentM.Chat
{
    [UdonBehaviourSyncMode(BehaviourSyncMode.None)]
    public class ChatSystem : NetworkEventListener
    {
        public LibDecentM lib;

        [Space]
        public ChannelsStore channelsStore;
        public TypingIndicatorStore typingIndicatorStore;
        public PresenceStore presenceStore;

        protected override void _Start()
        {
            this.TypingStateStart();
        }

        private void FixedUpdate()
        {
            this.TypingStateFixedUpdate();
        }

        #region Connector Entrypoints

        /**
         * These functions define the interfgace we expose to connectors.
         * This means connectors must implement this API in oreder to interface with us.
         * If a connector cannot or does not implement an event, the rest of the events must
         * still function as normal. For example, if a connector doesn't implement the
         * typing events, it just means we won't show typing indicators.
         * This can be used to delegate features to separate connectors.
         */

        /**
         * Purpose: The basic functionality of sending text messages
         */
        public void OnSendMessage(int channel, string message)
        {
            if (channel < 0 || message == "")
                return;

            this.HandleOutgoingMessage(channel, message);
        }

        /**
         * Purpose: To show typing indicators when users start/stop typing
         * Note: Only send this event when the user starts typing, do not send it when they stop.
         * The system will detect if this event isn't received for a while.
         */
        public void OnPlayerTyping()
        {
            this.HandleLetterEntry("");
        }

        public void OnPresenceChange(bool state)
        {
            this.HandlePresenceChange(state);
        }

        #endregion

        public Color debugColour;

        private void DebugLog(string message)
        {
            if (!this.lib.debugging.isDebugging)
                return;

            this.channelsStore.AddMessage(0, 0, Networking.LocalPlayer.playerId, message);
        }

        private const string SendMessageCommand = "SM";
        private const string SendLateMessageCommand = "SLM";
        private const string AckMessageCommand = "ACK";
        private const string TypingStartCommand = "TSTR";
        private const string TypingStopCommand = "TSTO";
        private const string PresenceOnCommand = "PON";
        private const string PresenceOffCommand = "POF";

        public const int MessageStatusErr = -1;
        public const int MessageStatusSending = 0;
        public const int MessageStatusSent = 1;
        public const int MessageStatusFailed = 2;
        public const int MessageStatusAckd = 3;
        public const int MessageStatusReceived = 4;
        public const int MessageStatusReceivedAckd = 5;

        public override void OnReceived(
            int sender,
            byte[] dataBuffer,
            int index,
            int length,
            int messageId
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
                // SendMessageCommand - int id, int channel, string message
                case SendMessageCommand:
                {
                    string id = "";
                    int channel = -1;
                    string message = "";

                    string[] args = arguments.Split(null, 3);
                    id = args[0];
                    bool channelParsed = int.TryParse(args[1], out channel);
                    message = args[2];

                    if (!channelParsed)
                        break;

                    this.HandleIncomingMessage(messageId, id, channel, sender, message);
                    break;
                }
                // SendLateMessageCommand - int id, int channel, int senderId, string message
                case SendLateMessageCommand:
                {
                    string id = "";
                    int channel = -1;
                    int senderId = -1;
                    string message = "";

                    string[] args = arguments.Split(null, 4);
                    id = args[0];
                    bool channelParsed = int.TryParse(args[1], out channel);
                    bool senderIdParsed = int.TryParse(args[2], out senderId);
                    message = args[3];

                    if (!channelParsed || !senderIdParsed)
                        break;

                    this.HandleIncomingMessage(messageId, id, channel, senderId, message);
                    break;
                }
                // AckMessageCommand: int id
                case AckMessageCommand:
                {
                    string id = arguments;

                    this.HandleMessageAck(id);
                    break;
                }

                case TypingStartCommand:
                {
                    this.HandleTypingStartReceived(sender);
                    break;
                }
                case TypingStopCommand:
                {
                    this.HandleTypingStopReceived(sender);
                    break;
                }
                case PresenceOffCommand:
                {
                    this.HandlePresenceOffReceived(sender);
                    break;
                }
                case PresenceOnCommand:
                {
                    this.HandlePresenceOnReceived(sender);
                    break;
                }
                default:
                    // Ignore messages we don't recognise
                    break;
            }
        }

        #region Presence Handling

        private void HandlePresenceChange(bool newState)
        {
            if (newState)
                this.HandlePresenceOnReceived(Networking.LocalPlayer.playerId);
            else
                this.HandlePresenceOffReceived(Networking.LocalPlayer.playerId);

            this.SendAll(false, newState ? PresenceOnCommand : PresenceOffCommand);
        }

        private void HandlePresenceOffReceived(int playerId)
        {
            this.presenceStore.OnRemovePresence(playerId);
        }

        private void HandlePresenceOnReceived(int playerId)
        {
            this.presenceStore.OnAddPresence(playerId);
        }

        #endregion

        #region Typing Indicator Handling

        private bool typingState = false;
        private int typingTimeoutClock = 0;
        public int typingIndicatorTimeoutSeconds = 10;

        private float typingIndicatorTimeoutTicks;

        private void TypingStateStart()
        {
            this.typingIndicatorTimeoutTicks =
                1 / Time.fixedDeltaTime * this.typingIndicatorTimeoutSeconds;
        }

        private void TypingStateFixedUpdate()
        {
            if (typingState == false)
                return;

            typingTimeoutClock++;

            if (typingTimeoutClock < this.typingIndicatorTimeoutTicks)
                return;

            this.typingState = false;
            this.typingTimeoutClock = 0;
            this.HandleTypingStateChange();
        }

        private void HandleLetterEntry(string letter)
        {
            if (this.typingState)
                return;

            this.typingTimeoutClock = 0;
            this.typingState = true;
            this.HandleTypingStateChange();
        }

        private void HandleTypingStateChange()
        {
            if (this.typingState)
                this.HandleTypingStartReceived(Networking.LocalPlayer.playerId);
            else
                this.HandleTypingStopReceived(Networking.LocalPlayer.playerId);

            this.SendAll(false, this.typingState ? TypingStartCommand : TypingStopCommand);
        }

        private void HandleTypingStartReceived(int playerId)
        {
            this.typingIndicatorStore.OnPlayerTypingChange(playerId, true);
        }

        private void HandleTypingStopReceived(int playerId)
        {
            this.typingIndicatorStore.OnPlayerTypingChange(playerId, false);
        }

        #endregion

        private string GetPlayerNameById(int id)
        {
            string name = "";

            VRCPlayerApi player = VRCPlayerApi.GetPlayerById(id);

            if (player != null && player.IsValid())
                name = player.displayName;

            return name;
        }

        private void HandleIncomingMessage(
            int packetId,
            string id,
            int channel,
            int senderId,
            string message
        )
        {
            // Add it locally
            this.channelsStore.AddMessageWithId(packetId, id, channel, senderId, message);
            this.channelsStore.ChangeMessageStatusById(id, MessageStatusReceived);

            // We don't care about the ack status, as the message will stay on "Sent, but not ack'd" status
            // for the sender if it doesn't receive a single ack
            // (plus, there was a bug with the first ack overriding further ones in the store because I don't want to store
            // more ID maps 8l)
            // this.SendCommandTarget(AckMessageCommand, senderId, id);

            // Send ack
            // Returns -1 if it failed, but we don't care since not receiving an ack will keep the message in
            // "Sent, but not ackd" status for the sender
            int ackPacketId = this.SendTarget(false, AckMessageCommand, senderId);
            int status = ackPacketId == -1 ? MessageStatusReceived : MessageStatusReceivedAckd;

            // Set the message's status to Received, ackd or not, depending on the outcome of the send
            this.channelsStore.ChangeMessageStatusById(id, status);
        }

        private void HandleMessageAck(string id)
        {
            this.DebugLog($"Received ack for message {id}");
            this.channelsStore.ChangeMessageStatusById(id, MessageStatusAckd);
        }

        private void HandleOutgoingMessage(int channel, string message)
        {
            // Set typing to stopped to prevent the indicator from appearing after the message has been sent
            this.typingState = false;
            this.typingTimeoutClock = 0;
            this.HandleTypingStateChange();

            string id = this.channelsStore.GenerateNextIdForPlayer(
                channel,
                Networking.LocalPlayer.playerId
            );

            this.DebugLog($"{SendMessageCommand} {id} {channel} {message}");
            int packetId = this.SendAll(false, $"{SendMessageCommand} {id} {channel} {message}");

            // Add the message locally
            // TODO: Check what the consequences of multiples of the same packetId are! It will probably just use the newest message
            // with that packet ID, which is probably fine, as this usually only happens for messages that failed to send.
            this.channelsStore.AddMessageWithId(
                packetId == -1 ? 0 : packetId,
                id,
                channel,
                Networking.LocalPlayer.playerId,
                message
            );

            // The packet id will be -1 if it failed to send
            int status = packetId == -1 ? MessageStatusFailed : MessageStatusSending;
            this.channelsStore.ChangeMessageStatusById(id, status);
        }

        public override void OnSendComplete(int messageId, bool succeed)
        {
            this.DebugLog($"SendComplete {messageId} {succeed}");

            // Check if the message that belongs to this packet was sent by us
            bool sentByLocalPlayer = this.channelsStore.IsMessageSentByLocalPlayerByPacket(
                messageId
            );

            if (!sentByLocalPlayer)
                return;

            int status = succeed ? MessageStatusSent : MessageStatusFailed;
            this.channelsStore.ChangeMessageStatusByPacket(messageId, status);
        }

        public override void OnPlayerLeft(VRCPlayerApi player)
        {
            if (
                player == null
                || Networking.LocalPlayer == null
                || player.playerId == Networking.LocalPlayer.playerId
            )
                return;

            this.channelsStore.PurgeMessagesByPlayerId(player.playerId);
        }

        public override void OnPlayerJoined(VRCPlayerApi player)
        {
            if (!Networking.LocalPlayer.isMaster || player == null || !player.IsValid())
                return;

            ChatMessage[] messages = this.channelsStore.GetAllMessages();

            foreach (ChatMessage message in messages)
            {
                this.SendTarget(
                    true,
                    $"{SendLateMessageCommand} {message.id} {message.channel} {message.senderId} {message.message}",
                    player.playerId
                );
            }
        }
    }
}
