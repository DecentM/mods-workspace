
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

[UdonBehaviourSyncMode(BehaviourSyncMode.None)]
public class PerformanceGovernorExampleRoot : UdonSharpBehaviour
{
    public GameObject laggy;
    public GameObject med;
    public GameObject good;

    public void OnPerformanceLow()
    {
        this.laggy.SetActive(false);
        this.med.SetActive(false);
        this.good.SetActive(true);
    }

    public void OnPerformanceMedium()
    {
        this.laggy.SetActive(false);
        this.med.SetActive(true);
        this.good.SetActive(true);
    }

    public void OnPerformanceHigh()
    {
        this.laggy.SetActive(true);
        this.med.SetActive(true);
        this.good.SetActive(true);
    }
}
