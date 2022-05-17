using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

using DecentM.VideoPlayer;
using DecentM.VideoPlayer.Plugins;

[UdonBehaviourSyncMode(BehaviourSyncMode.None)]
public class VideoScreenRealtimeEmission : UdonSharpBehaviour
{
    public ScreenAnalysisPlugin analysis;
    public VideoPlayerSystem system;

    public MeshRenderer meshRenderer;
    public string property = "_EmissionColor";

    private float elapsed = 0;

    Color[] history = new Color[8];

    int index = 0;

    private void FixedUpdate()
    {
        if (!this.system.IsPlaying() || this.meshRenderer == null)
            return;

        this.elapsed += Time.fixedUnscaledDeltaTime;
        if (this.elapsed <= 0.1f)
            return;
        this.elapsed = 0;

        if (index >= this.history.Length)
            index = 0;
        history[index] = this.analysis.GetAverage();
        index++;

        Color color = this.analysis.GetAverage(this.history) * 2;
        color.a = 1;

        this.meshRenderer.material.SetColor(this.property, color);
    }
}
