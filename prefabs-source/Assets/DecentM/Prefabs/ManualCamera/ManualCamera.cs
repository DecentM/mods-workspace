using UnityEngine;

public class ManualCamera : MonoBehaviour
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
    private float elapsed = 0;

    private void LateUpdate()
    {
        if (this.targetFps <= 0)
            return;

        this.elapsed += Time.unscaledDeltaTime;

        if (this.elapsed > 1f / this.targetFps)
        {
            this.elapsed = 0;
            this.camera.Render();
        }
    }
}
