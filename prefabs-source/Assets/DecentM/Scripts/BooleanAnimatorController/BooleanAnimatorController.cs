
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;

[UdonBehaviourSyncMode(BehaviourSyncMode.Manual)]
public class BooleanAnimatorController : UdonSharpBehaviour
{
    [Header("Settings")]
    [Tooltip("The Animator to control")]
    public Animator animator;
    [Tooltip("Which animation to target in the controller")]
    public int layerIndex = 0;
    [Tooltip("The name of the parameter to toggle")]
    public string parameterName = "";

    public void Toggle()
    {
        // All targets should be the same at this point, so use the first one as the source of truth
        // This way we don't need to store an internal variable
        bool isActive = this.animator.GetBool(this.parameterName);

        if (isActive)
        {
            this.ToggleOff();
        }
        else
        {
            this.ToggleOn();
        }
    }

    public void ToggleOn()
    {
        this.SetActive(true);
    }

    public void ToggleOff()
    {
        this.SetActive(false);
    }

    private void SetActive(bool value)
    {
        this.animator.SetBool(this.parameterName, value);
    }
}
