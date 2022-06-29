using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

namespace DecentM.Chat.Connectors
{
    public interface IChatConnector
    {
        public ChatSystem system;
        public void OnPlayerTyping();
        public void OnPlayerPresent();
        public void OnPlayerAway();
    }

    [UdonBehaviourSyncMode(BehaviourSyncMode.None)]
    public class ConnectorGeneric : UdonSharpBehaviour, IChatConnector
    {
        public ChatSystem system;

        public void OnPlayerTyping()
        {
            this.system.OnPlayerTyping();
        }

        public void OnPlayerPresent()
        {
            this.system.OnPresenceChange(true);
        }

        public void OnPlayerAway()
        {
            this.system.OnPresenceChange(false);
        }
    }
}
