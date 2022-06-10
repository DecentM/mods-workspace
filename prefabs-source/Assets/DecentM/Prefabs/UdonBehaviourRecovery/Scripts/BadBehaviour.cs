using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;
using TMPro;

namespace DecentM.UdonBehaviourRecovery
{
    [UdonBehaviourSyncMode(BehaviourSyncMode.Continuous)]
    public class BadBehaviour : UdonSharpBehaviour
    {
        public TextMeshProUGUI tmp;

        [UdonSynced, FieldChangeCallback(nameof(continuousSync))]
        private float _continuousSync;

        public float continuousSync
        {
            get { return this._continuousSync; }
            set
            {
                this._continuousSync = value;
                this.UpdateContSync();
            }
        }

        public GameObject rotatedObject;

        private void UpdateContSync()
        {
            this.tmp.text = $"{this.continuousSync}";
        }

        private void FixedUpdate()
        {
            if (Networking.GetOwner(this.gameObject) != Networking.LocalPlayer)
                return;

            this.continuousSync = Time.time;

            this.rotatedObject.transform.rotation = Quaternion.Euler(
                Mathf.Sin(this.continuousSync) * 90,
                0,
                0
            );
        }

        private GameObject nonexist;

        public override void Interact()
        {
            Debug.Log($"[BadBehaviour] Crashing on purpose...");
            this.nonexist.SetActive(false);
        }
    }
}
