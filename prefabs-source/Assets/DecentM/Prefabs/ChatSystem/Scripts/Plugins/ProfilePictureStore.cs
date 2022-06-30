using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

namespace DecentM.Chat
{
    [UdonBehaviourSyncMode(BehaviourSyncMode.None)]
    public class ProfilePictureStore : UdonSharpBehaviour
    {
        public RenderTexture[] store;
        public GameObject cameraTemplate;
        public ChatEvents events;

        private GameObject cameraObject;

        private void Start()
        {
            // If it's not set up, we generate an empty store, which will always return no profile picture for any request,
            // as render textures cannot be generated at runtime with Udon
            if (this.store == null)
                this.store = new RenderTexture[0];

            this.cameraTemplate.SetActive(false);

            this.cameraObject = Instantiate(cameraTemplate);
            this.cameraObject.transform.SetParent(this.gameObject.transform);
            this.cameraObject.name = $"{name}_LocalCamera";
            this.cameraObject.transform.SetPositionAndRotation(
                this.cameraTemplate.transform.position,
                this.cameraTemplate.transform.rotation
            );
            this.cameraObject.transform.localScale = this.cameraTemplate.transform.localScale;

            this.cameraObject.SetActive(true);
            Networking.SetOwner(Networking.LocalPlayer, this.cameraObject);
        }

        public RenderTexture GetPlayerPicture(int playerId)
        {
            if (playerId < 0 || playerId > this.store.Length - 1)
                return null;

            return this.store[playerId];
        }

        public void TakePicture(int playerId)
        {
            VRCPlayerApi player = VRCPlayerApi.GetPlayerById(playerId);

            if (player == null || !player.IsValid())
                return;

            VRCPlayerApi.TrackingData head = player.GetTrackingData(
                VRCPlayerApi.TrackingDataType.Head
            );
            this.cameraObject.transform.SetPositionAndRotation(
                head.position + (head.rotation * Vector3.forward * 2),
                head.rotation
            );
            this.cameraObject.transform.LookAt(head.position);

            Camera camera = this.cameraObject.GetComponent<Camera>();
            if (camera == null)
                return;

            RenderTexture rt = this.GetPlayerPicture(playerId);
            if (rt == null)
                return;

            camera.targetTexture = rt;
            camera.Render();

            this.events.OnProfilePictureChange(playerId);
        }
    }
}
