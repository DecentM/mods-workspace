
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;
using DecentM;

[UdonBehaviourSyncMode(BehaviourSyncMode.None)]
public class BadActor : UdonSharpBehaviour
{
    public LibDecentM lib;
    private GameObject nonExist;

    private void Start()
    {
        this.lib.scheduling.OnEverySecond((UdonBehaviour)GetComponent(typeof(UdonBehaviour)));
    }

    public void OnSecondPassed()
    {
        Vector3 rotateBy = new Vector3(0, 18, 0);

        this.gameObject.transform.Rotate(rotateBy);
    }

    public void DoCrash()
    {
        nonExist.SetActive(true);
    }
}
