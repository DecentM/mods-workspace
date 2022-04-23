
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;
using DecentM;

[UdonBehaviourSyncMode(BehaviourSyncMode.None)]
public class AutoMirror : UdonSharpBehaviour
{
    public LibDecentM lib;
    public ActivateObjectsVolume trigger;

    public int updatesPerSecond = 3;
    public int maxDistance = 5;
    public LayerMask raycastLayer;
    public Transform raycastWall;

    private LayerMask walkthroughLayer;
    private LayerMask pickupLayer;
    private LayerMask playerLayer;
    private LayerMask reflectionLayer;
    private LayerMask uiLayer;
    private LayerMask defaultLayer;

    public VRC_MirrorReflection[] mirrors;

    private void Start()
    {
        this.walkthroughLayer = LayerMask.NameToLayer("Walkthrough");
        this.pickupLayer = LayerMask.NameToLayer("Pickup");
        this.playerLayer = LayerMask.NameToLayer("Player");
        this.reflectionLayer = LayerMask.NameToLayer("MirrorReflection");
        this.uiLayer = LayerMask.NameToLayer("UI");
        this.defaultLayer = LayerMask.NameToLayer("Default");

        this.trigger.lib = this.lib;
        this.trigger.global = false;

        this.DisableAllMirrors();
        this.lib.performanceGovernor.Subscribe(this);
    }

    private int clock = 0;
    private void FixedUpdate()
    {
        this.clock++;

        // Update `updatesPerSecond` times per second
        if (this.clock > 1 / Time.fixedDeltaTime / this.updatesPerSecond)
        {
            this.clock = 0;
            this.CheckPlayer();
        }
    }

    private void CheckPlayer()
    {
        VRCPlayerApi.TrackingData tracking = Networking.LocalPlayer.GetTrackingData(VRCPlayerApi.TrackingDataType.Head);

        RaycastHit hit;
        Vector3 direction = this.raycastWall.position - tracking.position;

        bool rayHit = Physics.Raycast(tracking.position, direction, out hit, this.maxDistance + 1, this.raycastLayer);

        if (this.lib.debugging.isDebugging)
        {
            Debug.DrawRay(tracking.position, direction, rayHit ? Color.yellow : Color.red);
        };

        if (!rayHit) return;

        this.Process(hit.distance);
    }

    private void Process(float distance)
    {
        float normalisedDistance = distance / this.maxDistance;
        int mirrorIndex = Mathf.FloorToInt(Mathf.Clamp(normalisedDistance, 0, 1) * this.mirrors.Length);

        this.EnableMirror(mirrorIndex);
    }

    private void EnableMirror(int index)
    {
        if (index >= this.mirrors.Length) return;

        this.DisableAllMirrors();
        this.mirrors[index].gameObject.SetActive(true);
    }

    private void DisableAllMirrors()
    {
        foreach (VRC_MirrorReflection mirror in this.mirrors) {
            mirror.gameObject.SetActive(false);
        }
    }

    public void OnPerformanceHigh()
    {
        foreach (VRC_MirrorReflection mirror in this.mirrors)
        {
            mirror.m_ReflectLayers = LayerMask.GetMask("Player", "MirrorReflection", "Pickup", "Default", "Walkthrough", "UI", "Environment");
            // mirror.m_ReflectLayers = (int)this.walkthroughLayer | (int)this.pickupLayer | (int)this.playerLayer | (int)this.reflectionLayer | (int)this.uiLayer | (int)this.defaultLayer;
        }
    }

    public void OnPerformanceMedium()
    {
        foreach (VRC_MirrorReflection mirror in this.mirrors)
        {
            mirror.m_ReflectLayers = LayerMask.GetMask("Player", "MirrorReflection", "Pickup", "Default", "Environment");
            // mirror.m_ReflectLayers = (int)this.pickupLayer | (int)this.playerLayer | (int)this.reflectionLayer | (int)this.defaultLayer;
        }
    }

    public void OnPerformanceLow()
    {
        foreach (VRC_MirrorReflection mirror in this.mirrors)
        {
            mirror.m_ReflectLayers = LayerMask.GetMask("Player", "MirrorReflection");
            // mirror.m_ReflectLayers = (int)this.playerLayer | (int)this.reflectionLayer;
        }
    }
}
