
using UdonSharp;
using UnityEngine;

[UdonBehaviourSyncMode(BehaviourSyncMode.Continuous)]
public class PlayerAspectRatio : UdonSharpBehaviour
{
    [Header("Settings")]
    [Tooltip("Sets the base scale of the video screen")]
    public float scale = 2f;

    [Header("References")]
    [Tooltip("The video screen from USharpVideo")]
    public MeshRenderer videoScreenRenderer;
    [Tooltip("A one by one mesh to use. This mesh will be resized with the required aspect ratio.")]
    public Mesh oneByOneMesh;

    [Header("Variables designed to be set by a script")]
    public string stringArg = "";

    [UdonSynced, FieldChangeCallback(nameof(width))]
    public float _width = 1.6f;
    public float width
    {
        get => this._width;
        set
        {
            this._width = value;
            this.Resize(value, this._height);
        }
    }

    [UdonSynced, FieldChangeCallback(nameof(height))]
    public float _height = 0.9f;
    public float height
    {
        get => this._height;
        set
        {
            this._height = value;
            this.Resize(this._width, value);
        }
    }

    private void Start()
    {
        if (this.videoScreenRenderer == null)
        {
            Debug.LogError("[PlayerAspectRatio] VideoScreenRenderer is required to be set.");
            this.enabled = false;
            return;
        }

        if (this.videoScreenRenderer.gameObject.isStatic) {
            Debug.LogError($"[PlayerAspectRatio] The linked video screen is static! Resizing it will not work, therefore the aspect ratio cannot be changed. Exiting...");
            this.enabled = false;
            return;
        };

        MeshFilter meshFilter = this.videoScreenRenderer.GetComponent<MeshFilter>();
        meshFilter.mesh = this.oneByOneMesh;

        this.Resize(this.width, this.height);
    }

    public void UpdateFromStringArg()
    {
        this.UpdateFromString(this.stringArg);
        this.stringArg = "";
    }

    // Reads the stringArg variable, and parses it into a width and height float
    // for example, 16:9 would yield [16, 9]
    private void UpdateFromString(string input)
    {
        string[] ars = input.Split(':');

        if (ars.Length != 2 || ars[0] == null || ars[0] == "" || ars[1] == null || ars[1] == "")
        {
            Debug.Log($"Invalid input given to UpdateFromString. Input was: {input}");
            for (int i = 0; i < ars.Length; i++)
            {
                Debug.Log($"ars[{i}] == \"{ars[i].ToString()}\"");
            }
            return;
        }

        this.width = float.Parse(ars[0]);
        this.height = float.Parse(ars[1]);
    }

    private void Resize(float w, float h)
    {
        // We're only adjusting the width of the screen, so the user can set the height by setting the scale variable.
        float finalW = w / h;

        this.videoScreenRenderer.transform.localScale = new Vector3(finalW * this.scale, 1 * this.scale, 1);
        // "_TargetAspectRatio" is the name of the AR parameter in the USharpVideo Standard Video Emission shader
        this.videoScreenRenderer.sharedMaterial.SetFloat("_TargetAspectRatio", w / h);
    }
}
