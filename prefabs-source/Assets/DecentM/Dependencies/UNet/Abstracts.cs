using UdonSharp;

namespace UNet
{
    public abstract class NetworkEventsListener : UdonSharpBehaviour
    {
        public NetworkInterface network;
        public ByteBufferReader reader;
        public ByteBufferWriter writer;

        protected virtual void _Start() { }

        private void Start()
        {
            this.network.AddEventsListener(this);
            this._Start();
        }

        public virtual void OnInit() { }

        public virtual void OnPrepareSend() { }

        public virtual void OnConnected(int playerId) { }

        public virtual void OnDisconnected(int playerId) { }

        public virtual void OnReceived(
            int sender,
            byte[] dataBuffer,
            int index,
            int length,
            int messageId
        ) { }

        public virtual void OnSendComplete(int messageId, bool succeed) { }

        protected string StringFromBuffer(int length, byte[] buffer)
        {
            return this.reader.ReadUTF8String(length, buffer, 0);
        }

        private byte[] BufferFromString(string input)
        {
            int length = this.writer.GetUTF8StringSize(input);
            byte[] buffer = new byte[length + 1];
            this.writer.WriteUTF8String(input, buffer, 0);
            return buffer;
        }

        protected int SendAll(bool sequenced, string message)
        {
            byte[] buffer = this.BufferFromString(message);
            int length = this.writer.GetUTF8StringSize(message);
            return this.network.SendAll(sequenced, buffer, length);
        }

        protected int SendTarget(bool sequenced, string message, int player)
        {
            byte[] buffer = this.BufferFromString(message);
            int length = this.writer.GetUTF8StringSize(message);
            return this.network.SendTarget(sequenced, buffer, length, player);
        }

        protected int SendTargets(bool sequenced, string message, int[] players)
        {
            byte[] buffer = this.BufferFromString(message);
            int length = this.writer.GetUTF8StringSize(message);
            return this.network.SendTargets(sequenced, buffer, length, players);
        }

        protected int SendMaster(bool sequenced, string message)
        {
            byte[] buffer = this.BufferFromString(message);
            int length = this.writer.GetUTF8StringSize(message);
            return this.network.SendMaster(sequenced, buffer, length);
        }
    }
}
