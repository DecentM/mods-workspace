
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

[UdonBehaviourSyncMode(BehaviourSyncMode.Manual)]
public class ManualCamera : UdonSharpBehaviour
{
    [Header("References")]
    [Tooltip("The Camera component that this UdonBehaviour controls")]
    public new Camera camera;

    void Start()
    {
        if (this.camera == null)
        {
            Debug.LogError("The Camera component has not been set, this ManualCamera will be disabled");
            return;
        }

        // Disable the camera so it doesn't auto-render
        this.camera.enabled = false;
    }

    public void Render()
    {
        this.camera.Render();
    }
}
