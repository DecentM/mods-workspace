
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;
using DecentM;

[UdonBehaviourSyncMode(BehaviourSyncMode.None)]
public class BadActor : UdonSharpBehaviour
{
    private GameObject nonExist;
    private Vector3 rotateBy = new Vector3(0, 2, 0);

    private void FixedUpdate()
    {
        this.gameObject.transform.Rotate(rotateBy);
    }

    public void DoCrash()
    {
        nonExist.SetActive(true);
    }
}
