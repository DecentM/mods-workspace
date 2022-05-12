using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using UnityEngine.UI;
using VRC.Udon;
using DecentM;

[UdonBehaviourSyncMode(BehaviourSyncMode.None)]
public class AvatarEnforcer : UdonSharpBehaviour
{
    [Header("LibDecentM")]
    [Tooltip("The LibDecentM object")]
    public LibDecentM lib;

    // public float wantedJawPos = 1.2121266f;
    public Text debugText;

    // public HumanBodyBones targetBone1 = HumanBodyBones.Hips;
    public HumanBodyBones targetBone = HumanBodyBones.Head;

    // public HumanBodyBones targetBoneRoot = HumanBodyBones.Neck;

    void Start()
    {
        this.debugText.text = "Starting...";

        this.lib.scheduling.OnEverySecond((UdonBehaviour)GetComponent(typeof(UdonBehaviour)));
    }

    public void OnSecondPassed()
    {
        // Quaternion boneRot1 = Networking.LocalPlayer.GetBoneRotation(this.targetBone1);
        // Quaternion boneRot2 = Networking.LocalPlayer.GetBoneRotation(this.targetBone2);

        // Vector3 bonePos1 = Networking.LocalPlayer.GetBonePosition(this.targetBone1);
        // Vector3 bonePos = Networking.LocalPlayer.GetBonePosition(this.targetBone);
        // Vector3 rootPos = Networking.LocalPlayer.GetBonePosition(this.targetBoneRoot);

        VRCPlayerApi.TrackingData td = Networking.LocalPlayer.GetTrackingData(
            VRCPlayerApi.TrackingDataType.Head
        );

        // if (bonePos1.y == 0)
        // {
        // this.debugText.text = $"{this.targetBone1} was not found";
        // return;
        // }

        // if (bonePos2.y == 0)
        // {
        // this.debugText.text = $"{this.targetBone2} was not found";
        // return;
        // }

        // float diff = Mathf.Round((bonePos2.y - bonePos1.y) * 10000);
        // float rotdiff = Mathf.Round((boneRot1.w - boneRot2.w) * 1000);

        this.debugText.text = $"{this.targetBone} {td.position.normalized}";
    }
}
