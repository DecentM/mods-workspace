using System;
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;
using TMPro;
using UnityEngine.UI;

namespace DecentM.Keyboard
{
    [UdonBehaviourSyncMode(BehaviourSyncMode.None)]
    public class KeyboardKey : UdonSharpBehaviour
    {
        public string primarySymbol = "";
        public string secondarySymbol = "";
        public string tertiarySymbol = "";

        public KeyLayout layoutTemplate;
        private GameObject layoutInstance;
        private LabelTarget labelTarget;
        private Animator animator;

        private Material[] keyMaterials;
        private Image keyImage;
        private TextMeshProUGUI[] keyLabels;
        private Light[] keyLights;

        public bool hideSymbol = false;
        public bool isToggle = false;
        public Texture emissionMap;

        [HideInInspector]
        public KeyboardLayout keyboardLayout;

        [HideInInspector]
        public int symbolIndex = -1;

        public KeyboardEvents events;

        private void Start()
        {
            MeshRenderer keyMesh = this.GetComponentInChildren<MeshRenderer>();
            SkinnedMeshRenderer keySkinnedMesh = this.GetComponentInChildren<SkinnedMeshRenderer>();

            if (keyMesh != null)
            {
                this.keyMaterials = keyMesh.materials;
            } else if (keySkinnedMesh != null)
            {
                this.keyMaterials = keySkinnedMesh.materials;
            }

            if (this.keyMaterials != null)
            {
                foreach (Material mat in this.keyMaterials)
                {
                    mat.EnableKeyword("_EMISSION");
                    mat.SetTexture("_EmissionMap", this.emissionMap);
                }
            }

            this.InstantiateLayout();

            this.animator = this.GetComponent<Animator>();
            this.keyLights = this.GetComponentsInChildren<Light>();
            this.keyImage = this.GetComponentInChildren<Image>();

            // Turn the backlight off by default
            this.ChangeBacklight(Color.black);
        }

        public int symbolCount
        {
            get
            {
                int result = 0;

                if (this.primarySymbol != "") result++;
                if (this.secondarySymbol != "") result++;
                if (this.tertiarySymbol != "") result++;

                return result;
            }
        }

        private void InstantiateLayout()
        {
            this.labelTarget = this.GetComponentInChildren<LabelTarget>();

            if (this.hideSymbol || this.layoutTemplate == null) return;

            this.layoutInstance = VRCInstantiate(this.layoutTemplate.gameObject);
            this.layoutInstance.name = $"KeyLayout_{this.name}_{this.primarySymbol}";
            this.layoutInstance.transform.SetParent(this.labelTarget.transform, false);
            this.layoutInstance.transform.localPosition = Vector3.zero;
            this.layoutInstance.transform.localRotation = Quaternion.identity;

            KeyLayout layout = this.layoutInstance.GetComponent<KeyLayout>();
            this.keyLabels = this.layoutInstance.GetComponentsInChildren<TextMeshProUGUI>();

            layout.SetSymbols(this.primarySymbol, this.secondarySymbol, this.tertiarySymbol);
        }

        public float minBrightness = 0.2f;

        public void ChangeBacklight(Color backlight)
        {
            float r = Mathf.Max(backlight.r, this.minBrightness);
            float g = Mathf.Max(backlight.g, this.minBrightness);
            float b = Mathf.Max(backlight.b, this.minBrightness);
            float a = Mathf.Max(backlight.a, this.minBrightness);

            Color finalColour = new Color(r, g, b, a);

            if (this.keyLights != null)
            {
                foreach (Light light in this.keyLights)
                {
                    light.intensity = finalColour.a;
                    light.color = finalColour;
                    light.enabled = backlight != Color.black;
                }
            }

            if (this.keyMaterials != null)
            {
                foreach (Material mat in this.keyMaterials)
                {
                    mat.SetColor("_EmissionColor", finalColour);
                }
            }

            if (this.keyImage != null)
            {
                this.keyImage.color = finalColour;
            }

            if (this.keyLabels != null)
            {
                foreach (TextMeshProUGUI label in this.keyLabels)
                {
                    label.color = finalColour;
                }
            }
        }

        private Color tempColour;
        private bool tempChangePending = false;

        public void ChangeBacklightFromTemp()
        {
            if (this.tempColour == null || !this.tempChangePending) return;
            this.ChangeBacklight(this.tempColour);
            this.tempChangePending = false;
        }

        public bool ChangeBacklightAfterSeconds(Color backlight, float seconds)
        {
            if (this.tempChangePending) return false;
            this.tempChangePending = true;
            this.tempColour = backlight;
            this.SendCustomEventDelayedSeconds(nameof(ChangeBacklightFromTemp), seconds);
            return true;
        }

        private GameObject currentCollider;

        public void OnTriggerEnter(Collider other)
        {
            Drumstick drumstick = other.gameObject.GetComponent<Drumstick>();

            // If we're currently colliding with something, we don't react to other
            // objects until that one leaves the trigger. We also only care about colliding
            // with a Drumstick.
            if (drumstick == null || this.currentCollider != null) return;
            this.currentCollider = other.gameObject;

            drumstick.PlayHapticFeedback();
            this.animator.SetBool("KeyPress", true);
            this.keyboardLayout.HandleKeyPress(this);
            this.events.OnKeyPressDown(this);
        }

        public void OnTriggerExit(Collider other)
        {
            if (this.currentCollider != other.gameObject) return;

            this.currentCollider = null;
            this.events.OnKeyPressUp(this);
            this.animator.SetBool("KeyPress", false);
        }
    }
}
