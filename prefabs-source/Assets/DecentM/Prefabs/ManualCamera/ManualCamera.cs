
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

[UdonBehaviourSyncMode(BehaviourSyncMode.Manual)]
public class ManualCamera : UdonSharpBehaviour
{
    private new Camera camera;

    private void Start()
    {
        this.camera = GetComponent<Camera>();
        this.camera.enabled = false;
    }

    public void Render()
    {
        this.camera.Render();
    }

    public int targetFps = 30;
    private float elapsed;

    private void LateUpdate()
    {
        this.elapsed += Time.unscaledDeltaTime;
        if (this.elapsed > 1 / this.targetFps)
        {
            this.elapsed = 0;
            this.camera.Render();
        }
    }
}
