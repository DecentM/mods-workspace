
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

[UdonBehaviourSyncMode(BehaviourSyncMode.Manual)]
public class InteractDistributor : UdonSharpBehaviour
{
    public Component[] targets;
    public string[] eventNames;

    private void Start()
    {
        if (this.targets == null || this.eventNames == null)
        {
            Debug.LogError("[InteractDistributor] Targets and Event Names are both required.");
            UdonBehaviour self = (UdonBehaviour) GetComponent(typeof(UdonBehaviour));
            self.enabled = false;
            return;
        }

        if (this.targets.Length != this.eventNames.Length)
        {
            Debug.LogError("[InteractDistributor] Targets and Event Names must have the same number of items.");
            UdonBehaviour self = (UdonBehaviour) GetComponent(typeof(UdonBehaviour));
            self.enabled = false;
            return;
        }
    }

    public override void Interact()
    {
        for (int i = 0; i < this.targets.Length; i++)
        {
            if (this.targets[i].GetType() != typeof(UdonBehaviour))
            {
                continue;
            }

            UdonBehaviour behaviour = (UdonBehaviour) targets[i];

            behaviour.SendCustomEvent(this.eventNames[i]);
        }
    }
}
