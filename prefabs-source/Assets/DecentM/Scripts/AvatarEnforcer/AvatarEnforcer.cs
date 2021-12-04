
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using UnityEngine.UI;
using VRC.Udon;

[UdonBehaviourSyncMode(BehaviourSyncMode.None)]
public class AvatarEnforcer : UdonSharpBehaviour
{
    public float wantedJawPos = 1.2121266f;
    public Text debugText;

    void Start()
    {
        Vector3 jawPos = Networking.LocalPlayer.GetBonePosition(HumanBodyBones.Hips);

        bool flag = jawPos.y == this.wantedJawPos;

        if (flag == true)
        {
            this.debugText.text = $"Avatar accepted, bone position was {jawPos}, expected {this.wantedJawPos}";
        } else {
            this.debugText.text = $"Avatar denied, bone position was {jawPos}, expected {this.wantedJawPos}";
        }

        if (jawPos.y == 0)
        {
            Debug.Log("We're probably in the editor");
        }
    }
}
