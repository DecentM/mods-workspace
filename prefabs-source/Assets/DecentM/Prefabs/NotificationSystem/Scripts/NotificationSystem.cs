using System;
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;
using TMPro;
using UnityEngine.UI;

[UdonBehaviourSyncMode(BehaviourSyncMode.None)]
public class NotificationSystem : UdonSharpBehaviour
{
    public GameObject[] overlayObjects;
    public Camera rendererCamera;

    private string[] queueTexts;
    private Sprite[] queueSprites;

    public Animator animator;
    public TextMeshProUGUI textSlot;
    public Image imageSlot;

    private float ticksPerSecond = 50;

    private void Start()
    {
        this.ticksPerSecond = 1 / Time.fixedDeltaTime;
        this.queueTexts = new string[0];
        this.queueSprites = new Sprite[0];
    }

    public void SendNotification(Sprite image, string text)
    {
        if (this.queueTexts == null)
            this.queueTexts = new string[0];

        string[] tempStr = new string[this.queueTexts.Length + 1];
        Array.Copy(this.queueTexts, 0, tempStr, 0, this.queueTexts.Length);
        tempStr[tempStr.Length - 1] = text;
        this.queueTexts = tempStr;

        Sprite[] tempSpr = new Sprite[this.queueSprites.Length + 1];
        Array.Copy(this.queueSprites, 0, tempSpr, 0, this.queueSprites.Length);
        tempSpr[tempSpr.Length - 1] = image;
        this.queueSprites = tempSpr;
    }

    private int clock = 0;

    private bool animationRunning = false;
    private int animationClock = 0;

    private void FixedUpdate()
    {
        this.AnimationTrackingFixedUpdate();
        this.NotificationCheckingFixedUpdate();
    }

    private void AnimationTrackingFixedUpdate()
    {
        if (this.animationRunning)
            this.animationClock++;

        // If at least 5 seconds passed since we started the animation, mark it as finished, so that the
        // next notification is displayed from the queue.
        if (this.animationClock > this.ticksPerSecond * 6)
        {
            this.animationRunning = false;
            this.rendererCamera.enabled = false;
            this.animationClock = 0;

            foreach (GameObject overlay in this.overlayObjects)
            {
                overlay.SetActive(false);
            }
        }
    }

    private void NotificationCheckingFixedUpdate()
    {
        this.clock++;

        // Only check notifications twice every second
        if (this.clock < this.ticksPerSecond / 2)
            return;

        this.clock = 0;
        this.CheckNotifications();
    }

    private void CheckNotifications()
    {
        if (this.queueTexts.Length == 0 || this.animationRunning)
            return;

        // If the queue length is not zero, it means we have at least one notification pending
        this.animationRunning = true;
        this.rendererCamera.enabled = true;

        foreach (GameObject overlay in this.overlayObjects)
        {
            overlay.SetActive(true);
        }

        // Take the first notification and assign it to the currently invisible items
        this.textSlot.text = this.queueTexts[0];
        this.imageSlot.sprite = this.queueSprites[0];

        // Make them visible
        this.animator.SetTrigger("ShowNotification");

        // If we only had a single notification, we can just clear the queue, no
        // need to shift stuff around.
        if (this.queueTexts.Length == 1)
        {
            this.queueTexts = new string[0];
            this.queueSprites = new Sprite[0];
            return;
        }

        // Remove the first element from the queue (that we just displayed) and move all other elements
        // one index down
        string[] tempStr = new string[this.queueTexts.Length - 1];
        Array.Copy(this.queueTexts, 1, tempStr, 0, tempStr.Length);
        this.queueTexts = tempStr;

        Sprite[] tempSpr = new Sprite[this.queueSprites.Length - 1];
        Array.Copy(this.queueSprites, 1, tempSpr, 0, tempSpr.Length);
        this.queueSprites = tempSpr;
    }
}
