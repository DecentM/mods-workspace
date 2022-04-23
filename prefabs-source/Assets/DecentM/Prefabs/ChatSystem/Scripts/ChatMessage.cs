using System;
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;
using UnityEngine.UI;
using TMPro;

namespace DecentM.Chat
{
    [UdonBehaviourSyncMode(BehaviourSyncMode.None)]
    public class ChatMessage : UdonSharpBehaviour
    {
        public Image iconSlot;
        public RawImage ppSlot;
        public Sprite sendingIcon;
        public Sprite sentIcon;
        public Sprite failedToSendIcon;
        public Sprite ackdIcon;
        public Sprite receviedIcon;
        public Sprite receivedAckdIcon;

        [Space]
        public TextMeshProUGUI usernameSlot;
        public TextMeshProUGUI messageSlot;
        public TextMeshProUGUI noteSlot;

        [Space]
        public ProfilePictureStore profilePictureStore;

        public const int MessageStatusErr = -1;
        public const int MessageStatusSending = 0;
        public const int MessageStatusSent = 1;
        public const int MessageStatusFailed = 2;
        public const int MessageStatusAckd = 3;
        public const int MessageStatusReceived = 4;
        public const int MessageStatusReceivedAckd = 5;

        /**
         * -1: Not set, erroneous call
         * 0: Sending
         * 1: Sent
         * 2: Failed to send
         * 3: Sent message acknowledged at least once
         * 4: Received
         * 5: Ack sent for received message
         */
        [HideInInspector]
        public int OnStatusChange_status = MessageStatusErr;
        public void OnStatusChange()
        {
            // Ignore improperly sent event
            if (OnStatusChange_status == -1) return;

            this.status = OnStatusChange_status;

            switch (OnStatusChange_status)
            {
                case MessageStatusSending:
                    this.iconSlot.sprite = this.sendingIcon;
                    break;
                case MessageStatusSent:
                    this.iconSlot.sprite = this.sentIcon;
                    break;
                case MessageStatusFailed:
                    this.iconSlot.sprite = this.failedToSendIcon;
                    break;
                case MessageStatusAckd:
                    this.iconSlot.sprite = this.ackdIcon;
                    break;
                case MessageStatusReceived:
                    this.iconSlot.sprite = this.receviedIcon;
                    break;
                case MessageStatusReceivedAckd:
                    this.iconSlot.sprite = this.receivedAckdIcon;
                    break;
                default:
                    break;
            }

            this.RenderMessage();

            this.OnStatusChange_status = -1;
        }

        [HideInInspector]
        public int status = -1;
        [HideInInspector]
        public int packetId = -1;
        [HideInInspector]
        public string id = "";
        [HideInInspector]
        public int channel = -1;
        [HideInInspector]
        public int senderId = -1;
        [HideInInspector]
        public string message = "";
        [HideInInspector]
[VRC.Udon.Serialization.OdinSerializer.OdinSerialize] /* UdonSharp auto-upgrade: serialization */ 
        public DateTime timestamp;

        [HideInInspector]
        public int OnReceive_packetId;
        [HideInInspector]
        public string OnReceive_id;
        [HideInInspector]
        public int OnReceive_channel;
        [HideInInspector]
        public int OnReceive_senderId;
        [HideInInspector]
        public string OnReceive_message;
        public void OnReceive()
        {
            this.packetId = this.OnReceive_packetId;
            this.id = OnReceive_id;
            this.channel = OnReceive_channel;
            this.senderId = OnReceive_senderId;
            this.message = OnReceive_message;
            this.timestamp = Networking.GetNetworkDateTime();

            this.profilePictureStore.TakePicture(this.senderId);
            this.RenderMessage();
        }

        public Color colour;

        [HideInInspector]
        public Color OnColourChange_colour;
        public void OnColourChange()
        {
            // Ignore improperly sent event
            if (OnColourChange_colour == null) return;

            this.colour = OnColourChange_colour;

            this.RenderMessage();
        }

        private const int Minute = 60;
        private const int Hour = 60 * 60;

        private string ToRelativeTime(DateTime dateTime)
        {
            var ts = new TimeSpan(DateTime.UtcNow.Ticks - dateTime.Ticks);
            double delta = Math.Abs(ts.TotalSeconds);

            if (delta < Minute) return ts.Seconds <= 1 ? "just now" : $"less than a minute ago";
            if (delta < Minute * 2) return "a minute ago";
            if (delta < 50 * Minute) return $"{ts.Minutes} minutes ago";
            if (delta < 75 * Minute) return "about an hour ago";
            if (delta < 24 * Hour) return $"{ts.Hours} hours ago";

            return "long ago";
        }

        private float ticksPerSecond = 50;

        private void Start()
        {
            this.ticksPerSecond = 1 / Time.fixedDeltaTime;
        }

        private int clock = 0;
        private void FixedUpdate()
        {
            this.clock++;

            // Re-render every 10 seconds to update the timestamp
            if (this.clock <= this.ticksPerSecond * 10) return;

            this.clock = 0;
            this.RenderMessage();
        }

        public Color failedColour = Color.red;

        private void RenderMessage()
        {
            string displayName = "<unknown>";
            VRCPlayerApi player = VRCPlayerApi.GetPlayerById(this.senderId);
            if (player != null && player.IsValid()) displayName = player.displayName;

            this.usernameSlot.text = displayName;
            this.messageSlot.text = this.message;
            this.messageSlot.color = this.colour;

            if (this.status == 2)
            {
                this.noteSlot.text = "failed to send";
                this.messageSlot.color = this.failedColour;
            }
            else
            {
                this.noteSlot.text = this.ToRelativeTime(this.timestamp);
            }
            
            RenderTexture rt = this.profilePictureStore.GetPlayerPicture(this.senderId);
            this.ppSlot.texture = rt;
        }
    }
}
