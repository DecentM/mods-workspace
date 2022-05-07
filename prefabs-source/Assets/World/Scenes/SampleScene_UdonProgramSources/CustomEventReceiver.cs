
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

[UdonBehaviourSyncMode(BehaviourSyncMode.Manual)]
public class CustomEventReceiver : UdonSharpBehaviour
{
    public void OnMyCustomEvent()
    {
        Vector3 rotateBy = new Vector3(0, 18, 0);

        this.gameObject.transform.Rotate(rotateBy);
    }
}
