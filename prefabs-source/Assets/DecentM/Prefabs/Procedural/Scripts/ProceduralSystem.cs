using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

using DecentM.Tools;

namespace DecentM.Procedural
{
    [UdonBehaviourSyncMode(BehaviourSyncMode.Manual)]
    public class ProceduralSystem : UdonSharpBehaviour
    {
        public RandomInstance random;

        [UdonSynced, FieldChangeCallback(nameof(seed))]
        private int _seed;

        public int seed
        {
            get => _seed;
            set
            {
                this._seed = value;
                this.HandleSeedChange();
            }
        }

        private void Start()
        {
            if (Networking.GetOwner(this.gameObject) != Networking.LocalPlayer)
                return;

            this.seed = 6;
            this.RequestSerialization();
        }

        private void HandleSeedChange()
        {
            this.random.SetSeed(this.seed);
        }
    }
}
