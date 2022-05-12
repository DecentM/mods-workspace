using UdonSharp;
using UnityEngine;
using VRC.Udon;
using DecentM;

[UdonBehaviourSyncMode(BehaviourSyncMode.None)]
public class UdonBehaviourRecovery : UdonSharpBehaviour
{
    [Header("Settings")]
    [Tooltip("Check for crashed UdonBehaviours every N seconds")]
    public int pollIntervalSeconds = 60;

    [Header("LibDecentM")]
    [Tooltip("The LibDecentM object")]
    public LibDecentM lib;

    private int secondsSinceLastCheck = 0;
    private float fixedUpdateRate;

    private Component[] childComponents;
    private bool isChecking = false;
    private int checkingIndex = 0;

    private void Start()
    {
        // The Unity editor logs an error if this is set on the initialiser, so we set it on Start
        this.fixedUpdateRate = 1 / Time.fixedDeltaTime;

        // UdonSharp fails to compile if it sees UdonBehaviour[], so we just use Component[]
        // and cast to UdonBehaviour individually later
        this.childComponents = GetComponentsInChildren<Component>();

        // Subscribe to the broadcast that happens every second from the scheduler
        this.lib.scheduling.OnEverySecond((UdonBehaviour)GetComponent(typeof(UdonBehaviour)));
    }

    public void OnSecondPassed()
    {
        // We don't want to receive events when we're in the middle of checking
        if (this.isChecking)
        {
            return;
        }

        this.secondsSinceLastCheck++;

        if (this.secondsSinceLastCheck >= this.pollIntervalSeconds)
        {
            this.secondsSinceLastCheck = 0;
            this.isChecking = true;
        }
    }

    private void FixedUpdate()
    {
        // We don't do anything unless we're in the middle of checking
        if (this.isChecking)
        {
            this.CheckAndFixAll();
        }
    }

    private void CheckAndFixAll()
    {
        this.CheckAndFixIndex(this.checkingIndex);
        this.checkingIndex++;

        // We finished checking all components when the cursor is at the end of the component array
        if (this.checkingIndex >= this.childComponents.Length)
        {
            this.checkingIndex = 0;
            this.secondsSinceLastCheck = 0;
            this.isChecking = false;
        }
    }

    private void CheckAndFixIndex(int i)
    {
        Component component = this.childComponents[i];

        // Prevent crashing due to weird invalid components
        if (component == null)
        {
            return;
        }

        // Skip over non-UdonBehaviours
        if (component.GetType() != typeof(UdonBehaviour))
        {
            return;
        }

        // Cast to UdonBehaviour to get access to .enabled
        UdonBehaviour behaviour = (UdonBehaviour)component;

        if (!behaviour.enabled)
        {
            behaviour.enabled = true;
        }
    }
}
