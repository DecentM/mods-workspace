using HarmonyLib;

using UnityEngine;
using MelonLoader;

using ABI_RC.Core.Player;
using ABI_RC.Core.Networking;
using DarkRift;
using DarkRift.Client;

namespace DecentM.Mods.ComponentsUnleashed
{
    public static class NetworkManagerPatches
    {
        [HarmonyPatch(typeof(NetworkManager), "Awake")]
        [HarmonyPostfix]
        static void AwakePostfix()
        {
            NetworkedBehaviour.SetNetworkManager(NetworkManager.Instance);
        }
    }

    public class NetworkedBehaviour
    {
        private static NetworkManager network;

        public static void SetNetworkManager(NetworkManager networkManager)
        {
            network = networkManager;
        }

        public void Awake()
        {
            MelonLogger.Msg("[NetworkedBehaviour] Subscribing to network events...");

            network.GameNetwork.MessageReceived += this.HandleMessageReceived;
        }

        private void HandleMessageReceived(object sender, MessageReceivedEventArgs e)
        {
            MelonLogger.Msg("[NetworkedBehaviour] Some message was received");
        }

        public void SendTestConnectionMessage()
        {
            if (network.GameNetwork.ConnectionState != ConnectionState.Connected)
            {
                MelonLogger.Msg("[NetworkedBehaviour] Not connected to server, skipping message");
                return;
            }

            MelonLogger.Msg("[NetworkedBehaviour] Sending Test message");

            using (DarkRiftWriter writer = DarkRiftWriter.Create())
            {
                writer.Write("Test");

                // TODO: Figure out what the heck this tag argument is, for now just set it to something
                using (Message message = Message.Create(9992, writer))
                {
                    network.GameNetwork.SendMessage(message, SendMode.Unreliable);
                }
            }
        }
    }
}
