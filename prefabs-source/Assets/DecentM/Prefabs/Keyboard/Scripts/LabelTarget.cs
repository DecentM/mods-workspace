using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

namespace DecentM.Keyboard
{
    [UdonBehaviourSyncMode(BehaviourSyncMode.None)]
    public class LabelTarget : UdonSharpBehaviour
    {
        // Empty class just so that no other game object can be dragged in as a label target
    }
}
