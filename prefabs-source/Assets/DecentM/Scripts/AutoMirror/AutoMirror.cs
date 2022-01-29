
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
    public TimedCustomEvent manualCameraEvents;

    public Camera lowCamera;
    public int updatesPerSecond = 3;
    public int raycastMaxDistance = 5;
    public LayerMask raycastLayer;
    public Transform raycastWall;

    public GameObject[] mirrors;

    private void Start()
    {
        if (this.mirrors.Length != 5)
        {
            Debug.LogError($"[AutoMirror] I need exactly 5 mirrors set. You set {this.mirrors.Length}.");
            this.enabled = false;
            return;
        }

        this.trigger.lib = this.lib;
        this.trigger.global = false;

        this.manualCameraEvents.lib = this.lib;

        this.DisableAllMirrors();
    }

    private int clock = 0;
    private void FixedUpdate()
    {
        this.clock++;

        // Update twice per second
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

        bool rayHit = Physics.Raycast(tracking.position, direction, out hit, this.raycastMaxDistance, this.raycastLayer);

        if (this.lib.debugging.isDebugging)
        {
            Debug.DrawRay(tracking.position, direction, rayHit ? Color.yellow : Color.red);
        };

        if (!rayHit) return;

        this.Process(hit.distance);
    }

    private void Process(float distance)
    {
        if (distance < 1)
        {
            this.EnableMirror(0);
            return;
        }

        if (distance < 2)
        {
            this.EnableMirror(1);
            return;
        }

        if (distance < 3)
        {
            this.EnableMirror(2);
            return;
        }

        if (distance < 4)
        {
            this.EnableMirror(3);
            return;
        }

        if (distance < 5)
        {
            this.EnableMirror(4);
            this.ToggleCamera(true);
            return;
        }
    }

    private void ToggleCamera(bool state)
    {
        this.lowCamera.gameObject.SetActive(state);
    }

    private void EnableMirror(int index)
    {
        if (this.mirrors[index] == null) return;

        this.DisableAllMirrors();
        this.mirrors[index].gameObject.SetActive(true);
    }

    private void DisableAllMirrors()
    {
        this.ToggleCamera(false);

        foreach (GameObject mirror in this.mirrors) {
            mirror.gameObject.SetActive(false);
        }
    }
}
