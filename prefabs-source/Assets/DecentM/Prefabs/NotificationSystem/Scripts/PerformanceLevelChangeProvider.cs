
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;
using UnityEngine.UI;

public class PerformanceLevelChangeProvider : UdonSharpBehaviour
{
    public NotificationSystem notifications;
    public Toggle toggle;

    public PerformanceGovernor performance;

    public Sprite iconHigh;
    public Sprite iconMed;
    public Sprite iconLow;

    private void Start()
    {
        this.performance.Subscribe(this);
    }

    public void OnPerformanceHigh()
    {
        if (!this.toggle.isOn) return;
        this.notifications.SendNotification(this.iconHigh, "Performance mode changed to High");
    }

    public void OnPerformanceMedium()
    {
        if (!this.toggle.isOn) return;
        this.notifications.SendNotification(this.iconMed, "Performance mode changed to Medium");
    }

    public void OnPerformanceLow()
    {
        if (!this.toggle.isOn) return;
        this.notifications.SendNotification(this.iconLow, "Performance mode changed to Low");
    }
}
