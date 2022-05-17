using System;
using JetBrains.Annotations;
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;
using DecentM.Network;
using DecentM;
using TMPro;

namespace DecentM.BigSync
{
    [UdonBehaviourSyncMode(BehaviourSyncMode.Manual)]
    public class BigSyncSystem : NetworkEventsListener
    {
        public LibDecentM lib;
        public BigSyncEvents events;

        [UdonSynced, FieldChangeCallback(nameof(hashAndLength))]
        private string _hashAndLength;

        public string hashAndLength
        {
            set
            {
                if (Networking.GetOwner(this.gameObject) == Networking.LocalPlayer)
                    return;

                // The owner changed hashes, which means we need to clear the existing value and resync it
                // from the owner from the start

                if (value == this._hashAndLength)
                    return;

                this._hashAndLength = value;
                this.HandleHashChange();
            }
            get => _hashAndLength;
        }

        private string[] syncSource = new string[0];

        private int ownerId
        {
            get
            {
                VRCPlayerApi player = Networking.GetOwner(this.gameObject);
                if (player == null || !player.IsValid())
                    return -1;

                return player.playerId;
            }
        }

        #region Universal stuff

        protected override void _Start()
        {
            this.events.OnDebugLog("_Start()");
        }

        // "SR 1" for example, where 1 is the index we're requesting to sync
        private const string SyncRequestCommand = "SR";

        private string CreateSyncRequestCommand(int index)
        {
            return $"{SyncRequestCommand} {index}";
        }

        // "S 1 asdsadasd" for example, where 1 is the index we're syncing, and asdsadasd is the string contents of the index
        private const string SyncCommand = "S";

        private string CreateSyncCommand(int index, string value)
        {
            return $"{SyncCommand} {index} {value}";
        }

        private void SyncIndexToPlayer(int playerId, int index)
        {
            if (index >= this.syncSource.Length || index < 0)
                return;

            this.SendTarget(false, this.CreateSyncCommand(index, this.syncSource[index]), playerId);
        }

        public override void OnReceived(
            int sender,
            byte[] dataBuffer,
            int index,
            int length,
            int messageId
        )
        {
            string message = this.StringFromBuffer(length, dataBuffer);
            string[] parts = message.Split(new char[] { ' ' }, 3);

            this.events.OnDebugLog($"got a network message, separates into {parts.Length} parts");
            this.events.OnDebugLog($"message: {message}");

            switch (parts[0])
            {
                case SyncCommand:
                {
                    // Only non-owners should receive this command
                    if (this.ownerId == Networking.LocalPlayer.playerId)
                        return;

                    if (parts.Length != 3)
                        return;

                    int syncIndex;
                    if (!int.TryParse(parts[1], out syncIndex))
                        return;

                    string syncValue = parts[2];
                    this.HandleSyncCommand(syncIndex, sender, syncValue);
                    break;
                }

                case SyncRequestCommand:
                {
                    // Only owners should receive this command
                    if (this.ownerId != Networking.LocalPlayer.playerId)
                        return;

                    if (parts.Length != 2)
                        return;

                    int syncRequestIndex;
                    if (!int.TryParse(parts[1], out syncRequestIndex))
                        return;

                    this.HandleSyncRequestCommand(syncRequestIndex, sender);
                    break;
                }
            }
        }

        #endregion

        #region Owner stuff

        [PublicAPI]
        public bool SyncString(string input)
        {
            // Only the owner can sync strings
            if (Networking.GetOwner(this.gameObject) != Networking.LocalPlayer)
            {
                this.events.OnDebugLog("Not syncing because we're not owners");
                return false;
            }

            string source = input;
            float maxLength = this.network.GetMaxDataLength(false, VRCPlayerApi.GetPlayerCount());

            int digits = Mathf.CeilToInt(source.Length / maxLength).ToString().Length;
            float chunkSize = maxLength - SyncCommand.Length - 1 - digits - 1;
            this.syncSource = new string[Mathf.CeilToInt(source.Length / chunkSize)];

            string line = string.Empty;
            int index = 0;

            while (source.Length > 0)
            {
                line += source[0];
                source = source.Remove(0, 1);

                // write the line if the size limit is reached of we're out of input
                if (line.Length >= chunkSize || source.Length == 0)
                {
                    this.syncSource[index] = line;
                    index++;
                    line = string.Empty;
                }
            }

            this._hashAndLength = $"{this.lib.hash.SHA256_UTF8(input)} {this.syncSource.Length}";
            this.events.OnDebugLog($"Syncing: {this._hashAndLength}");
            this.RequestSerialization();

            return true;
        }

        private void HandleSyncRequestCommand(int requestingPlayer, int requestedIndex)
        {
            this.events.OnDebugLog($"player {requestingPlayer} is requesting {requestedIndex}");

            if (requestedIndex == 0)
                this.events.OnSyncBegin(
                    BigSyncDirection.Receive,
                    requestingPlayer,
                    this.syncSource.Length
                );
            else if (requestedIndex == this.syncSource.Length)
                this.events.OnSyncEnd(
                    BigSyncDirection.Receive,
                    requestingPlayer,
                    BigSyncStatus.Successful
                );
            else
                this.events.OnSyncProgress(
                    BigSyncDirection.Receive,
                    requestingPlayer,
                    (float)requestedIndex / this.syncSource.Length
                );

            this.SyncIndexToPlayer(requestingPlayer, requestedIndex);
        }

        #endregion

        #region Non-owner stuff

        private void RequestIndexFromOwner(int index)
        {
            this.events.OnDebugLog($"requesting {index} from owner");
            this.SendTarget(false, this.CreateSyncRequestCommand(index), this.ownerId);
        }

        private void HandleHashChange()
        {
            this.events.OnDebugLog($"hash changed");

            string[] parts = this.hashAndLength.Split(' ');
            if (parts.Length != 2)
                return;

            int arrayLength;
            if (!int.TryParse(parts[1], out arrayLength))
                return;

            this.syncSource = new string[arrayLength];
            this.RequestIndexFromOwner(0);
        }

        private void HandleSyncCommand(int index, int playerId, string value)
        {
            this.events.OnDebugLog($"got sync command from {playerId}, index {index}");

            if (index >= this.syncSource.Length)
                return;

            if (string.IsNullOrEmpty(value))
                return;

            this.syncSource[index] = value;

            string[] hashParts = this.hashAndLength.Split(' ');
            string syncedSoFar = string.Join("", this.syncSource);
            string currentHash = this.lib.hash.SHA256_UTF8(syncedSoFar);

            if (currentHash != hashParts[0])
            {
                this.RequestIndexFromOwner(index + 1);
            }

            if (index == 0)
                this.events.OnSyncBegin(BigSyncDirection.Receive, playerId, this.syncSource.Length);
            else if (index == this.syncSource.Length)
                this.events.OnSyncEnd(BigSyncDirection.Receive, playerId, BigSyncStatus.Successful);
            else
                this.events.OnSyncProgress(
                    BigSyncDirection.Receive,
                    playerId,
                    (float)index / this.syncSource.Length
                );
        }

        #endregion
    }
}
