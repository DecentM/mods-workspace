
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

namespace DecentM.VideoPlayer
{
    [UdonBehaviourSyncMode(BehaviourSyncMode.None)]
    public class ScreenHandler : UdonSharpBehaviour
    {
        public Renderer[] screens;
        private new Camera camera;

        private void Start()
        {
            this.camera = GetComponentInChildren<Camera>();
        }

        private bool isLocked = false;

        public bool RenderToRenderTexture(RenderTexture texture)
        {
            if (this.isLocked) return false;

            this.isLocked = true;
            this.camera.enabled = false;
            this.camera.forceIntoRenderTexture = true;
            this.camera.targetTexture = texture;
            this.camera.Render();
            return true;
        }

        private void OnPostRender()
        {
            this.camera.targetTexture = null;
            this.isLocked = false;
        }

        public float GetBrightness()
        {
            if (this.screens == null || this.screens.Length == 0) return 1f;

            Renderer screen = this.screens[0];
            Color color = screen.material.GetColor("_Color");

            return (color.r + color.g + color.b) / 3;
        }

        public void SetAspectRatio(float aspectRatio)
        {
            foreach (Renderer screen in screens)
            {
                screen.material.SetFloat("_TargetAspectRatio", aspectRatio);
            }
        }

        public void SetBrightness(float alpha)
        {
            foreach (Renderer screen in this.screens)
            {
                screen.material.SetColor("_EmissionColor", new Color(alpha, alpha, alpha));
            }
        }

        public void SetIsAVPro(bool isAVPro)
        {
            foreach (Renderer screen in this.screens)
            {
                screen.material.SetInt("_IsAVProInput", isAVPro ? 1 : 0);
            }
        }

        public void SetTexture(Texture texture)
        {
            foreach (Renderer screen in this.screens)
            {
                screen.material.SetTexture("_EmissionMap", texture);
            }
        }

        public void SetSize(Vector2 size)
        {
            foreach (Renderer screen in this.screens)
            {
                screen.transform.localScale = new Vector3(size.x, size.y, screen.transform.localScale.z);
            }
        }
    }
}
