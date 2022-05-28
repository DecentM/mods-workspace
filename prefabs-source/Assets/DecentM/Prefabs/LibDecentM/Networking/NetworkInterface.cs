using UdonSharp;
using VRC.SDKBase;

namespace DecentM.Network
{
    public class NetworkInterface : UdonSharpBehaviour
    {
        public const int MAX_MESSAGE_SIZE = 512;

        public NetworkManager manager;
        public ByteBufferReader reader;
        public ByteBufferWriter writer;

        public bool IsInitComplete()
        {
            return (bool)manager.GetProgramVariable("isInitComplete");
        }

        public bool HasOtherConnections()
        {
            return manager.activeConnectionsCount > 1;
        }

        /// <summary>
        /// Adds a unet event listener.
        /// </summary>
        public void AddEventsListener(NetworkEventsListener listener)
        {
            manager.AddEventsListener(listener);
        }

        /// <summary>
        /// Removes a unet event listener.
        /// </summary>
        public void RemoveEventsListener(NetworkEventsListener listener)
        {
            manager.RemoveEventsListener(listener);
        }

        /// <summary>
        /// Returns max length of message for given options.
        /// </summary>
        /// <param name="sendTargetsCount">Target clients count, for <see cref="NetworkInterface.SendAll"/> and <see cref="NetworkInterface.SendMaster"/> is always 0.</param>
        /// <returns>Max length of message</returns>
        public int GetMaxDataLength(bool sequenced, int sendTargetsCount)
        {
            int len = MAX_MESSAGE_SIZE - 5; //header[byte] + length[ushort] + msg id[ushort]
            if (sequenced)
                len -= 1; //msg id[ushort] + sequence[byte]
            if (sendTargetsCount == 1)
                len -= 1; //connection index[byte]
            else if (sendTargetsCount > 1)
                len -= manager.connectionsMaskBytesCount;
            return len;
        }

        /// <summary>
        /// Cancels the sending of the message with the given id.
        /// This operation cannot affect the message if it has already been delivered to the recipients.
        /// This method must be called before the end of the message delivery (OnUNetSendComplete event), otherwise it may disrupt the sending of other messages.
        /// </summary>
        public void CancelMessageSend(int messageId)
        {
            manager.CancelMessageSend(messageId);
        }

        /// <summary>
        /// Sends message to other clients.
        /// </summary>
        /// <param name="data">Array of data bytes</param>
        /// <param name="dataLength">The length of data, must be less than or equals to <see cref="NetworkInterface.GetMaxDataLength"/></param>
        /// <returns>Message ID or -1 if the message was not added to the buffer</returns>
        public int SendAll(bool sequenced, byte[] data, int dataLength)
        {
            return manager.SendAll(sequenced, data, dataLength);
        }

        /// <summary>
        /// Sends message to master client only.
        /// </summary>
        /// <param name="data">Array of data bytes</param>
        /// <param name="dataLength">The length of data, must be less than or equals to <see cref="NetworkInterface.GetMaxDataLength"/></param>
        /// <returns>Message ID or -1 if the message was not added to the buffer</returns>
        public int SendMaster(bool sequenced, byte[] data, int dataLength)
        {
            return manager.SendMaster(sequenced, data, dataLength);
        }

        /// <summary>
        /// Sends message to target client only.
        /// </summary>
        /// <param name="data">Array of data bytes</param>
        /// <param name="dataLength">The length of data, must be less than or equals to <see cref="NetworkInterface.GetMaxDataLength"/></param>
        /// <param name="targetPlayerId">Target client <see cref="VRCPlayerApi.playerId"/></param>
        /// <returns>Message ID or -1 if the message was not added to the buffer</returns>
        public int SendTarget(bool sequenced, byte[] data, int dataLength, int targetPlayerId)
        {
            return manager.SendTarget(sequenced, data, dataLength, targetPlayerId);
        }

        /// <summary>
        /// Sends message to target clients only.
        /// </summary>
        /// <param name="data">Array of data bytes</param>
        /// <param name="dataLength">The length of data, must be less than or equals to <see cref="NetworkInterface.GetMaxDataLength"/></param>
        /// <param name="targetPlayerIds">Target clients <see cref="VRCPlayerApi.playerId"/></param>
        /// <returns>Message ID or -1 if the message was not added to the buffer</returns>
        public int SendTargets(bool sequenced, byte[] data, int dataLength, int[] targetPlayerIds)
        {
            return manager.SendTargets(sequenced, data, dataLength, targetPlayerIds);
        }
    }
}
