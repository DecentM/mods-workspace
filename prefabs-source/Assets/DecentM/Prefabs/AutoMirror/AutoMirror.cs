using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using DecentM.Pubsub;
using DecentM.Performance.Plugins;

namespace DecentM
{
    [UdonBehaviourSyncMode(BehaviourSyncMode.None)]
    public sealed class AutoMirror : PerformanceGovernorPlugin
    {
        public LibDecentM lib;

        public int updatesPerSecond = 3;
        public int maxDistance = 5;
        public LayerMask raycastLayer;
        public Transform raycastWall;

        public VRC_MirrorReflection[] mirrors;

        protected override void _Start()
        {
            this.DisableAllMirrors();
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
            VRCPlayerApi.TrackingData tracking = Networking.LocalPlayer.GetTrackingData(
                VRCPlayerApi.TrackingDataType.Head
            );

            RaycastHit hit;
            Vector3 direction = this.raycastWall.position - tracking.position;

            bool rayHit = Physics.Raycast(
                tracking.position,
                direction,
                out hit,
                this.maxDistance + 1,
                this.raycastLayer
            );

            if (this.lib.debugging.isDebugging)
            {
                Debug.DrawRay(tracking.position, direction, rayHit ? Color.yellow : Color.red);
            }
            ;

            if (!rayHit)
                return;

            this.Process(hit.distance);
        }

        private void Process(float distance)
        {
            float normalisedDistance = distance / this.maxDistance;
            int mirrorIndex = Mathf.FloorToInt(
                Mathf.Clamp(normalisedDistance, 0, 1) * this.mirrors.Length
            );

            this.EnableMirror(mirrorIndex);
        }

        private void EnableMirror(int index)
        {
            if (index >= this.mirrors.Length)
                return;

            this.DisableAllMirrors();
            this.mirrors[index].gameObject.SetActive(true);
        }

        private void DisableAllMirrors()
        {
            foreach (VRC_MirrorReflection mirror in this.mirrors)
            {
                mirror.gameObject.SetActive(false);
            }
        }

        private void SetMasks(params string[] layerNames)
        {
            foreach (VRC_MirrorReflection mirror in this.mirrors)
            {
                mirror.m_ReflectLayers = LayerMask.GetMask(layerNames);
            }
        }

        protected override void OnPerformanceModeChange(PerformanceGovernorMode mode, float fps)
        {
            switch (mode)
            {
                case PerformanceGovernorMode.Low:
                    this.SetMasks("Player", "MirrorReflection");
                    return;

                case PerformanceGovernorMode.Medium:
                    this.SetMasks("Player", "MirrorReflection", "Pickup", "Default", "Environment");
                    return;

                case PerformanceGovernorMode.High:
                    this.SetMasks(
                        "Player",
                        "MirrorReflection",
                        "Pickup",
                        "Default",
                        "Walkthrough",
                        "UI",
                        "Environment"
                    );
                    return;
            }
        }
    }
}
