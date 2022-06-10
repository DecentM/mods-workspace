using UdonSharp;
using UnityEngine;
using VRC.Udon;
using VRC.SDKBase;
using DecentM;

[UdonBehaviourSyncMode(BehaviourSyncMode.None)]
public class UdonBehaviourRecovery : UdonSharpBehaviour
{
    [Header("Settings")]
    [Tooltip("Check for crashed UdonBehaviours every N seconds")]
    public int pollIntervalSeconds = 60;

    public UdonBehaviour template;
    private GameObject instance;

    private void Start()
    {
        if (this.template.gameObject == this.gameObject)
        {
            Debug.LogError(
                $"The monitored UdonBehaviour must be on a different game object from the UdonBehaviourRecovery script."
            );
            this.enabled = false;
            return;
        }

        if (this.template.gameObject.activeSelf)
        {
            Debug.LogError($"The template containing the UdonBehaviour must be disabled.");
            this.enabled = false;
            return;
        }

        this.CreateInstance();
    }

    private float elapsed = 0;

    private void FixedUpdate()
    {
        this.elapsed += Time.fixedUnscaledDeltaTime;
        if (this.elapsed <= this.pollIntervalSeconds)
            return;
        this.elapsed = 0;

        if (!this.Check())
            this.Fix();
    }

    private bool Check()
    {
        if (this.instance == null)
            return false;

        UdonBehaviour behaviour = this.instance.GetComponent<UdonBehaviour>();

        if (behaviour == null || !behaviour.gameObject.activeSelf || !behaviour.enabled)
            return false;

        return true;
    }

    private void DestroyInstance()
    {
        UdonBehaviour behaviour = this.instance.GetComponent<UdonBehaviour>();

        if (behaviour != null)
            behaviour.enabled = false;

        this.instance.SetActive(false);
        Destroy(this.instance.gameObject);
        this.instance = null;
    }

    private void CreateInstance()
    {
        if (this.instance != null)
            this.DestroyInstance();

        this.instance = Instantiate(this.template.gameObject);
        this.instance.name = $"{this.template.name}_instance";
        this.instance.SetActive(true);
        this.instance.transform.SetParent(this.gameObject.transform, true);
    }

    private void Fix()
    {
        if (this.instance != null)
            this.DestroyInstance();

        this.CreateInstance();
    }
}
