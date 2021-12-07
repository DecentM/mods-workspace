
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

[UdonBehaviourSyncMode(BehaviourSyncMode.Manual)]
public class PlayerAspectRatio : UdonSharpBehaviour
{
    public MeshFilter meshFilter;

    public int defaultMeshIndex;
    public int targetMeshIndex;
    public MeshRenderer videoScreenRenderer;
    public Mesh[] meshes;

    private void Start()
    {
        if (this.meshes == null || this.meshes.Length == 0)
        {
            Debug.LogError("[PlayerAspectRatio] Meshes are required to be set.");
            this.enabled = false;
            return;
        }

        this.ChangeToMeshIndex(this.defaultMeshIndex);
    }

    public void ChangeMesh(string name)
    {
        // Search for the required mesh by name
        for (int i = 0; i < this.meshes.Length; i++)
        {
            if (this.meshes[i] == null || this.meshes[i].name != name)
            {
                continue;
            }

            this.ChangeToMeshIndex(i);
        }
    }

    private void ChangeToMeshIndex(int index)
    {
        Mesh mesh = this.meshes[index];

        this.meshFilter.mesh = mesh;

        float[] ar = new float[2];
        string[] nameAr = mesh.name.Split('x');

        ar[0] = float.Parse(nameAr[0]);
        ar[1] = float.Parse(nameAr[1]);

        this.videoScreenRenderer.sharedMaterial.SetFloat("_TargetAspectRatio", ar[0] / ar[1]);
    }

    public void UpdateFromTargetMeshIndex()
    {
        this.ChangeToMeshIndex(this.targetMeshIndex);
    }
}
