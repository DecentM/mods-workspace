
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

[UdonBehaviourSyncMode(BehaviourSyncMode.None)]
public class BadActorTrigger : UdonSharpBehaviour
{
    public BadActor badActor;

    public override void Interact()
    {
        if (this.badActor == null)
        {
            return;
        }

        badActor.SendCustomEvent(nameof(this.badActor.DoCrash));
    }
}
