using System;
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

namespace DecentM.Chat
{
    [UdonBehaviourSyncMode(BehaviourSyncMode.None)]
    public class PresenceStore : UdonSharpBehaviour
    {
        public ChatEvents events;
        public GameObject presenceTemplate;
        public Transform presenceRoot;

        private GameObject[] store;

        public int worldCapacity = 64;

        private void Start()
        {
            this.presenceTemplate.SetActive(false);
            this.store = new GameObject[this.worldCapacity];
        }

        #region Utilities

        private GameObject CreatePresenceObject(string name)
        {
            GameObject presenceObject = VRCInstantiate(this.presenceTemplate);

            presenceObject.transform.SetParent(this.presenceRoot);
            presenceObject.name = name;
            presenceObject.transform.SetPositionAndRotation(
                this.presenceTemplate.transform.position,
                this.presenceTemplate.transform.rotation
            );
            presenceObject.transform.localScale = this.presenceTemplate.transform.localScale;

            return presenceObject;
        }

        private GameObject GetPresenceByPlayer(int playerId)
        {
            // Check if the requested index is in bounds
            if (playerId < 0 || playerId >= this.store.Length)
                return null;

            return this.store[playerId];
        }

        #endregion

        #region Adding/Removing presences

        private void AddPresence(int senderId)
        {
            GameObject existing = this.GetPresenceByPlayer(senderId);

            if (existing != null)
                this.RemovePresenceByPlayer(senderId);

            GameObject presenceObject = this.CreatePresenceObject($"Presence_{senderId}");
            ChatPresence presence = presenceObject.GetComponent<ChatPresence>();

            presence.SetProgramVariable(nameof(presence.OnReceive_senderId), senderId);
            presence.SendCustomEvent(nameof(presence.OnReceive));

            presenceObject.SetActive(true);

            this.store[senderId] = presenceObject;
        }

        private void RemovePresenceByPlayer(int playerId)
        {
            // If there are no messages, it means this function was called in error, we just ignore the request
            if (this.store.Length == 0)
                return;

            GameObject presenceObject = this.GetPresenceByPlayer(playerId);

            // The store doesn't have a GameObject at the index, or the index was out of bounds
            if (presenceObject == null)
                return;

            // If there's only one message, don't bother with shifting stuff around, just set it to
            // an empty array.
            if (this.store.Length == 1)
            {
                this.store = new GameObject[0];
                return;
            }

            this.store[playerId] = null;

            // Clean up the actual object from the scene
            presenceObject.SetActive(false);
            Destroy(presenceObject);
        }

        public override void OnPlayerLeft(VRCPlayerApi player)
        {
            if (player == null || !player.IsValid())
                return;

            this.RemovePresenceByPlayer(player.playerId);
        }

        #endregion

        #region API

        public void OnAddPresence(int playerId)
        {
            if (playerId < 0)
                return;

            this.AddPresence(playerId);
            this.events.OnPlayerPresent(playerId);
        }

        public void OnRemovePresence(int playerId)
        {
            if (playerId < 0)
                return;

            this.RemovePresenceByPlayer(playerId);
            this.events.OnPlayerAway(playerId);
        }

        #endregion
    }
}
