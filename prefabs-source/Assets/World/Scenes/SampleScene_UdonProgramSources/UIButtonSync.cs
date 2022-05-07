
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

[UdonBehaviourSyncMode(BehaviourSyncMode.Manual)]
public class UIButtonSync : UdonSharpBehaviour
{
    public UdonBehaviour behaviour;
    public string variableName;
    public string variableValue;
    public string eventName;

    public void OnClick()
    {
        if (this.variableName != null && this.variableValue != null)
        {
            this.behaviour.SetProgramVariable(this.variableName, this.variableValue);
        }

        if (this.eventName != null)
        {
            this.behaviour.SendCustomEvent(this.eventName);
        }
    }
}
