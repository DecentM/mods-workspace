
using UdonSharp;
using UnityEngine;

namespace DecentM
{
    public class Debugging : UdonSharpBehaviour
    {
        [Header("Settings")]
        [Tooltip("If checked, linked objects will turn debugging on")]
        public bool isDebugging = false;

        public void ApplyToMeshRenderer(MeshRenderer mesh)
        {
            if (mesh == null)
            {
                return;
            }

            if (this.isDebugging)
            {
                mesh.enabled = true;
            }
            else
            {
                mesh.enabled = false;
            }
        }
    }
}
