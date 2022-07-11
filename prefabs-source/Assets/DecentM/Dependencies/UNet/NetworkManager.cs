using System;
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;

namespace UNet
{
    internal enum PacketTarget
    {
        All = 0,
        Master = 1 << 2,
        Single = 2 << 2,
        Multiple = 3 << 2,
    }

    internal enum PacketType
    {
        Normal = 0,
        Sequenced = 1,
        Ack = 2,
    }

    /// <summary>
    /// Manages all network activity
    /// </summary>
    [UdonBehaviourSyncMode(BehaviourSyncMode.Manual)]
    public class NetworkManager : UdonSharpBehaviour
    {
        private const byte MSG_TYPE_MASK = 3;
        private const byte MSG_TARGET_MASK = 3 << 2;
        private const int LENGTH_BYTES_COUNT = 2;

        [NonSerialized, HideInInspector]
        public int activeConnectionsCount = 0;

        [NonSerialized, HideInInspector]
        public ulong connectionsMask = 0ul;

        [NonSerialized, HideInInspector]
        public int connectionsMaskBytesCount;

        [UdonSynced]
        private int masterConnection = -1;

        private int localConnectionIndex;

        private int totalConnectionsCount;

        private Connection[] allConnections;
        private int[] connectionsOwners;

        private int masterId = -1;
        private Socket socket;

        private int eventListenersCount = 0;
        private NetworkEventsListener[] eventListeners;

        private bool hasLocal = false;
        private bool hasMaster = false;
        private bool isInitComplete = false;

        void Start()
        {
            var playersList = new VRCPlayerApi[VRCPlayerApi.GetPlayerCount()];
            foreach (var player in VRCPlayerApi.GetPlayers(playersList))
            {
                if (player.isMaster)
                {
                    masterId = player.playerId;
                    break;
                }
            }

            socket = gameObject.GetComponentInChildren<Socket>();
            allConnections = gameObject.GetComponentsInChildren<Connection>();
            totalConnectionsCount = allConnections.Length;
            connectionsMaskBytesCount = (totalConnectionsCount - 1) / 8 + 1;

            connectionsOwners = new int[totalConnectionsCount];
            for (var i = 0; i < totalConnectionsCount; i++)
            {
                connectionsOwners[i] = -1;
                allConnections[i].Init(i, this);
            }
        }

        public override void OnPlayerJoined(VRCPlayerApi player)
        {
            if (Networking.IsMaster)
            {
                int index = -1;
                if (player.isLocal)
                {
                    index = 0;
                    hasMaster = true;
                    masterConnection = index;
                    RequestSerialization();
                }
                else
                {
                    index = Array.IndexOf(connectionsOwners, -1);
                }
                if (index < 0)
                    Debug.LogError("UNet does not have an unoccupied connection for a new player");
                else
                {
                    OnOwnerReceived(index, player.playerId);
                    Networking.SetOwner(player, allConnections[index].gameObject);
                }
            }
        }

        public override void OnPlayerLeft(VRCPlayerApi player)
        {
            // the leaving player is null if the local player is leaving.
            // Don't need to bother with cleanup then, as the scene is being unloaded
            if (player == null)
                return;

            int id = player.playerId;
            int index = Array.IndexOf(connectionsOwners, id);
            if (index >= 0)
                OnConnectionRelease(index);

            if (id == masterId)
            {
                var playersList = new VRCPlayerApi[VRCPlayerApi.GetPlayerCount()];
                foreach (var playerInfo in VRCPlayerApi.GetPlayers(playersList))
                {
                    if (playerInfo.isMaster)
                    {
                        masterId = playerInfo.playerId;
                        if (playerInfo.isLocal)
                        {
                            masterConnection = localConnectionIndex;
                            RequestSerialization();
                        }
                        else
                        {
                            masterConnection = Array.IndexOf(connectionsOwners, masterId);
                        }
                        break;
                    }
                }
                if (isInitComplete)
                    socket.OnMasterLeave();
            }
        }

        public override void OnDeserialization()
        {
            if (masterConnection >= 0 && !isInitComplete)
            {
                hasMaster = true;
                OnOwnerReceived(masterConnection, masterId);
            }
        }

        public bool IsMasterConnection(int index)
        {
            return connectionsOwners[index] == masterId;
        }

        public void HandlePacket(int connection, byte[] dataBuffer, int dataBufferLength)
        {
            if (!isInitComplete || connectionsOwners[connection] < 0)
                return;

            int index = 0;
            while (index < dataBufferLength)
            {
                int header = dataBuffer[index];
                PacketType type = (PacketType)(header & MSG_TYPE_MASK);
                PacketTarget target = (PacketTarget)(header & MSG_TARGET_MASK);

                index++;

                bool isTarget = false;
                switch (target)
                {
                    case PacketTarget.All:
                        isTarget = true;
                        break;
                    case PacketTarget.Master:
                        isTarget = Networking.IsMaster;
                        break;
                    case PacketTarget.Single:
                        if (index >= dataBufferLength)
                            return;
                        isTarget = dataBuffer[index] == localConnectionIndex;
                        index++;
                        break;
                    case PacketTarget.Multiple:

                        {
                            if (index + connectionsMaskBytesCount > dataBufferLength)
                                return;
                            ulong mask = 0;
                            for (int i = 0; i < connectionsMaskBytesCount; i++)
                            {
                                mask |= (ulong)dataBuffer[index] << (i * 8);
                                index++;
                            }
                            ulong bit = 1ul << localConnectionIndex;
                            isTarget = (mask & bit) == bit;
                        }
                        break;
                }

                int sequence = 0;
                switch (type)
                {
                    case PacketType.Normal:

                    case PacketType.Sequenced:
                        if (
                            (index + LENGTH_BYTES_COUNT + (type == PacketType.Sequenced ? 3 : 2))
                            > dataBufferLength
                        )
                            return;
                        if (isTarget)
                        {
                            int id = (dataBuffer[index] << 8) | dataBuffer[index + 1];
                            index += 2;

                            if (type == PacketType.Sequenced)
                            {
                                sequence = dataBuffer[index];
                                index++;
                            }

                            int len = (dataBuffer[index] << 8) | dataBuffer[index + 1];
                            index += LENGTH_BYTES_COUNT;

                            if (index + len > dataBufferLength)
                                return;

                            if (type == PacketType.Sequenced)
                            {
                                socket.OnReceiveSequenced(
                                    connection,
                                    id,
                                    sequence,
                                    dataBuffer,
                                    index,
                                    len
                                );
                            }
                            else
                            {
                                socket.OnReceive(connection, id, dataBuffer, index, len);
                            }
                            index += len;
                        }
                        else
                        {
                            index += 2;
                            if (type == PacketType.Sequenced)
                                index++;
                            int len = (dataBuffer[index] << 8) | dataBuffer[index + 1];
                            index += LENGTH_BYTES_COUNT;
                            index += len;
                        }
                        break;

                    case PacketType.Ack:
                        if (index + 4 > dataBufferLength)
                            return;
                        if (isTarget)
                        {
                            int idStart = (dataBuffer[index] << 8) | dataBuffer[index + 1];
                            index += 2;
                            uint mask =
                                ((uint)dataBuffer[index] << 8) | (uint)dataBuffer[index + 1];
                            index += 2;

                            socket.OnReceivedAck(connection, idStart, mask);
                        }
                        else
                        {
                            index += 4;
                        }
                        break;
                }
            }
        }

        public int PrepareSendStream(int index)
        {
            if (!isInitComplete || connectionsOwners[index] < 0)
                return 0;

            for (var i = 0; i < eventListenersCount; i++)
            {
                NetworkEventsListener listener = eventListeners[i];
                listener.OnPrepareSend();

                /* if (eventListeners[i] is NetworkEventListener)
                    ((NetworkEventListener)eventListeners[i]).OnPrepareSend();
                else
                    eventListeners[i].SendCustomEvent("OnPrepareSend"); */
            }
            return socket.PrepareSendStream();
        }

        public void OnOwnerReceived(int index, int playerId)
        {
            var connection = allConnections[index];
            if (connectionsOwners[index] < 0)
            {
                connectionsOwners[index] = playerId;
                activeConnectionsCount++;

                if (playerId == Networking.LocalPlayer.playerId)
                {
                    localConnectionIndex = index;
                    socket.Init(connection, this, totalConnectionsCount);

                    hasLocal = true;
                }
                else
                {
                    connectionsMask |= 1ul << index;
                }

                if (isInitComplete)
                {
                    if (eventListeners != null)
                    {
                        for (var i = 0; i < eventListenersCount; i++)
                        {
                            NetworkEventsListener listener = eventListeners[i];
                            listener.OnConnected(playerId);
                            /* if (listener is NetworkEventListener)
                                ((NetworkEventListener)listener).OnConnected(playerId);
                            else
                            {
                                listener.SetProgramVariable("OnConnected_playerId", playerId);
                                listener.SendCustomEvent("OnConnected");
                            } */
                        }
                    }
                }
                else
                {
                    if (hasLocal && hasMaster)
                        Init();
                }
            }
        }

        public void OnDataReceived(
            Socket socket,
            int connectionIndex,
            byte[] dataBuffer,
            int index,
            int length,
            int messageId
        )
        {
            if (eventListeners != null)
            {
                int sender = connectionsOwners[connectionIndex];
                for (var i = 0; i < eventListenersCount; i++)
                {
                    NetworkEventsListener listener = eventListeners[i];
                    listener.OnReceived(sender, dataBuffer, index, length, messageId);

                    /* if (listener is NetworkEventListener)
                        ((NetworkEventListener)listener).OnReceived(
                            sender,
                            dataBuffer,
                            index,
                            length,
                            messageId
                        );
                    else
                    {
                        listener.SetProgramVariable("OnReceived_sender", sender);
                        listener.SetProgramVariable("OnReceived_dataBuffer", dataBuffer);
                        listener.SetProgramVariable("OnReceived_index", index);
                        listener.SetProgramVariable("OnReceived_length", length);
                        listener.SetProgramVariable("OnReceived_messageId", messageId);
                        listener.SendCustomEvent("OnReceived");
                    } */
                }
            }
        }

        public void OnSendComplete(Socket socket, int messageId, bool succeed)
        {
            if (eventListeners != null)
            {
                for (var i = 0; i < eventListenersCount; i++)
                {
                    NetworkEventsListener listener = eventListeners[i];
                    listener.OnSendComplete(messageId, succeed);

                    /* if (listener is NetworkEventListener)
                        ((NetworkEventListener)listener).OnSendComplete(messageId, succeed);
                    else
                    {
                        listener.SetProgramVariable("OnSendComplete_messageId", messageId);
                        listener.SetProgramVariable("OnSendComplete_succeed", succeed);
                        listener.SendCustomEvent("OnSendComplete");
                    } */
                }
            }
        }

        public void AddEventsListener(NetworkEventsListener listener)
        {
            if (eventListeners == null)
            {
                eventListeners = new NetworkEventsListener[1];
            }
            else if (eventListenersCount >= eventListeners.Length)
            {
                var tmp = new NetworkEventsListener[eventListenersCount * 2];
                eventListeners.CopyTo(tmp, 0);
                eventListeners = tmp;
            }

            eventListeners[eventListenersCount] = listener;
            eventListenersCount++;
        }

        public void RemoveEventsListener(NetworkEventsListener listener)
        {
            int index = Array.IndexOf(eventListeners, listener);
            if (index >= 0)
            {
                eventListenersCount--;
                Array.Copy(
                    eventListeners,
                    index + 1,
                    eventListeners,
                    index,
                    eventListenersCount - index
                );
                eventListeners[eventListenersCount] = null;
            }
        }

        public void CancelMessageSend(int messageId)
        {
            socket.CancelSend(messageId);
        }

        public int SendAll(bool sequenced, byte[] data, int dataLength)
        {
            // if (activeConnectionsCount < 2)
            //    return -1;
            return socket.SendAll(sequenced, data, dataLength);
        }

        public int SendMaster(bool sequenced, byte[] data, int dataLength)
        {
            if (activeConnectionsCount < 2 || Networking.IsMaster)
                return -1;
            return socket.SendMaster(sequenced, data, dataLength);
        }

        public int SendTarget(bool sequenced, byte[] data, int dataLength, int targetPlayerId)
        {
            if (activeConnectionsCount < 2)
                return -1;
            int index = Array.IndexOf(connectionsOwners, targetPlayerId);
            if (index < 0)
                return -1;
            return socket.SendTarget(sequenced, data, dataLength, index);
        }

        public int SendTargets(bool sequenced, byte[] data, int dataLength, int[] targetPlayerIds)
        {
            if (activeConnectionsCount < 2)
                return -1;
            if (targetPlayerIds.Length < 1)
                return -1;
            if (targetPlayerIds.Length == 1)
            {
                return socket.SendTarget(sequenced, data, dataLength, targetPlayerIds[0]);
            }
            uint targetsMask = 0;
            foreach (var playerId in targetPlayerIds)
            {
                int index = Array.IndexOf(connectionsOwners, playerId);
                if (index < 0)
                    return -1;
                targetsMask = 1u << index;
            }
            return socket.SendTargets(sequenced, data, dataLength, targetsMask);
        }

        private void Init()
        {
            isInitComplete = true;
            if (eventListeners != null && eventListenersCount > 0)
            {
                for (var i = 0; i < eventListenersCount; i++)
                {
                    NetworkEventsListener listener = eventListeners[i];
                    listener.OnInit();
                    /* if (listener is NetworkEventListener)
                        ((NetworkEventListener)listener).OnInit();
                    else
                    {
                        listener.SendCustomEvent("OnInit");
                    } */
                }

                int localId = Networking.LocalPlayer.playerId;
                for (var i = 0; i < totalConnectionsCount; i++)
                {
                    int owner = connectionsOwners[i];
                    if (owner >= 0 && owner != localId)
                    {
                        for (var j = 0; j < eventListenersCount; j++)
                        {
                            NetworkEventsListener listener = eventListeners[j];
                            listener.OnConnected(owner);
                            /* if (listener is NetworkEventListener)
                                ((NetworkEventListener)listener).OnConnected(owner);
                            else
                            {
                                listener.SetProgramVariable("OnConnected_playerId", owner);
                                listener.SendCustomEvent("OnConnected");
                            } */
                        }
                    }
                }
            }
        }

        private void OnConnectionRelease(int index)
        {
            int owner = connectionsOwners[index];
            connectionsOwners[index] = -1;
            connectionsMask &= ~(1ul << index);
            socket.OnConnectionRelease(index);

            activeConnectionsCount--;
            if (eventListeners != null)
            {
                for (var i = 0; i < eventListenersCount; i++)
                {
                    NetworkEventsListener listener = eventListeners[i];
                    listener.OnDisconnected(owner);

                    /* if (listener is NetworkEventListener)
                        ((NetworkEventListener)listener).OnDisconnected(owner);
                    else
                    {
                        listener.SetProgramVariable("OnDisconnected_playerId", owner);
                        listener.SendCustomEvent("OnDisconnected");
                    } */
                }
            }
        }
    }
}
