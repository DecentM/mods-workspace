
using UdonSharp;
using UnityEngine;
using VRC.Udon;

[UdonBehaviourSyncMode(BehaviourSyncMode.None)]
public class UdonBehaviourRecovery : UdonSharpBehaviour
{
    [Header("Settings")]
    [Tooltip("Check for crashed UdonBehaviours every N seconds")]
    public int pollSeconds = 10;

    private int secondsSinceLastCheck = 0;
    private int clock = 0;
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
    }

    private void FixedUpdate()
    {
        if (this.isChecking)
        {
            this.CheckAndFixIndex(this.checkingIndex);
            this.checkingIndex++;

            // We finished checking all components when the cursor is at the end of the component array
            if (this.checkingIndex >= this.childComponents.Length)
            {
                Debug.Log($"Checked {this.checkingIndex} components over {this.checkingIndex / this.fixedUpdateRate} seconds");

                this.checkingIndex = 0;
                this.clock = 0;
                this.secondsSinceLastCheck = 0;
                this.isChecking = false;
            }

            // We don't want to count time while checking, as we may have more than 50 components
            // to check. It would cause a race condition if checking was started before the previous one ended.
            return;
        }

        this.clock++;

        if (this.clock >= this.fixedUpdateRate)
        {
            this.clock = 0;
            this.secondsSinceLastCheck++;
        }

        if (this.secondsSinceLastCheck >= this.pollSeconds)
        {
            this.secondsSinceLastCheck = 0;
            this.isChecking = true;
        }
    }

    private void CheckAndFixIndex(int i)
    {
        Component component = this.childComponents[i];

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
