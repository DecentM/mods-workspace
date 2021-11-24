
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

namespace DecentM
{
    [UdonBehaviourSyncMode(BehaviourSyncMode.None)]
    public class Scheduling : UdonSharpBehaviour
    {
        public void EveryNSeconds ()
        {
            Debug.Log("EveryNSeconds called");
        }
    }
}
