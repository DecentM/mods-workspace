using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using DecentM.Mods.CustomComponents.StatsD;

public class DemoStatsDSender : MonoBehaviour
{
    public StatsDClient statsd;
    public float sendEverySeconds = 1;

    private float elapsed = 0;

    private void FixedUpdate()
    {
        this.elapsed += Time.fixedDeltaTime;
        if (this.elapsed < this.sendEverySeconds)
            return;
        this.elapsed = 0;

        this.statsd.Gauge("fps", 1 / Time.deltaTime);
    }
}
