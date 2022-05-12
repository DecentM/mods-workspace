using System;
using UdonSharp;
using UnityEngine;
using VRC.SDK3.Components;
using VRC.SDK3.Components.Video;
using VRC.SDKBase;
using VRC.Udon;
using TMPro;
using UnityEngine.UI;
using DecentM.UI;

namespace DecentM.VideoPlayer.Plugins
{
    enum SubtitleSlot
    {
        Default,
        Japanese,
        Korean,
        ChineseSimplified,
        ChineseHongKong,
        ChineseTaiwan,
        Arabic,
    }

    public sealed class UIPlugin : VideoPlayerPlugin
    {
        [Space]
        public LayerMask raycastLayerMask;
        public float raycastIntervalSeconds = 0.5f;
        public float raycastMaxDistance = 2f;
        public Animator animator;
        public GameObject raycastTarget;

        [Space]
        public Slider progress;

        [Space]
        public Button playButton;
        public Button pauseButton;
        public Button stopButton;

        [Space]
        public TextMeshProUGUI status;
        public VRCUrlInputField urlInput;
        public Button enterButton;

        [Space]
        public Image brightnessImage;
        public Slider brightnessSlider;
        public Image volumeImage;
        public Slider volumeSlider;

        [Space]
        public Sprite volumeMutedIcon;
        public Sprite volumeLowIcon;
        public Sprite volumeMediumIcon;
        public Sprite volumeHighIcon;
        public Sprite brightnessLowIcon;
        public Sprite brightnessMediumIcon;
        public Sprite brightnessHighIcon;

        [Space]
        public TextMeshProUGUI info;

        [Space]
        public TextMeshProUGUI titleSlot;
        public TextMeshProUGUI uploaderSlot;
        public TextMeshProUGUI descriptionSlot;
        public TextMeshProUGUI viewCountSlot;
        public TextMeshProUGUI likeCountSlot;

        [Space]
        public UI.Dropdown subtitlesDropdown;
        public Button subtitlesButton;
        public Image subtitlesButtonImage;
        public Image subtitlesButtonIcon;
        public TextMeshProUGUI subtitlesButtonLabel;
        public Sprite subtitlesAvailable;
        public Sprite subtitlesUnavailable;
        public Button subtitlesToggleButton;
        public Image subtitlesToggleButtonImage;
        public Image subtitlesToggleButtonIcon;

        [Space]
        public TextMeshProUGUI subtitleSlot;

        [Space]
        public Button ownershipButton;
        public TextMeshProUGUI ownershipLabel;
        public Sprite lockIcon;
        public Sprite unlockIcon;
        public Sprite ownershipTransferIcon;

        [Space]
        public VRCUrl emptyUrl;

        private string HumanReadableTimestamp(float timestamp)
        {
            TimeSpan t = TimeSpan.FromSeconds(timestamp);

            if (t.Hours > 0)
                return string.Format("{0:D2}:{1:D2}:{2:D2}", t.Hours, t.Minutes, t.Seconds);

            return string.Format("{0:D2}:{1:D2}", t.Minutes, t.Seconds);
        }

        private string GetProgressIndicator(float timestamp, float duration)
        {
            if (float.IsInfinity(timestamp))
                return "Live";
            if (float.IsInfinity(duration))
                return $"{this.HumanReadableTimestamp(timestamp)} (Live)";

            return $"{this.HumanReadableTimestamp(timestamp)} / {this.HumanReadableTimestamp(duration)}";
        }

        #region Focus handling

        private float elapsed = 0;

        private RaycastHit hitInfo;

        public Quaternion desktopRaycastTurn = Quaternion.identity;
        public Quaternion vrRaycastTurn = Quaternion.identity;

        private bool CheckDesktopHit()
        {
            if (Networking.LocalPlayer.IsUserInVR())
                return false;

            VRCPlayerApi.TrackingData head = Networking.LocalPlayer.GetTrackingData(
                VRCPlayerApi.TrackingDataType.Head
            );

            return Physics.Raycast(
                head.position,
                head.rotation * desktopRaycastTurn * Vector3.forward,
                out hitInfo,
                this.raycastMaxDistance,
                this.raycastLayerMask
            );
        }

        private bool CheckVRHit()
        {
            if (!Networking.LocalPlayer.IsUserInVR())
                return false;

            VRCPlayerApi.TrackingData rightHand = Networking.LocalPlayer.GetTrackingData(
                VRCPlayerApi.TrackingDataType.RightHand
            );
            VRCPlayerApi.TrackingData leftHand = Networking.LocalPlayer.GetTrackingData(
                VRCPlayerApi.TrackingDataType.LeftHand
            );

            RaycastHit rightHitInfo;
            RaycastHit leftHitInfo;

            bool rightHit = Physics.Raycast(
                rightHand.position,
                rightHand.rotation * vrRaycastTurn * Vector3.forward,
                out rightHitInfo,
                this.raycastMaxDistance,
                this.raycastLayerMask
            );
            bool leftHit = Physics.Raycast(
                leftHand.position,
                leftHand.rotation * vrRaycastTurn * Vector3.forward,
                out leftHitInfo,
                this.raycastMaxDistance,
                this.raycastLayerMask
            );

            if (rightHit)
            {
                this.hitInfo = rightHitInfo;
                return true;
            }

            if (leftHit)
            {
                this.hitInfo = leftHitInfo;
                return true;
            }

            return false;
        }

        private bool uiRunning = false;

        public override void OnPlayerTriggerEnter(VRCPlayerApi player)
        {
            if (player != Networking.LocalPlayer)
                return;

            this.uiRunning = true;
        }

        public override void OnPlayerTriggerExit(VRCPlayerApi player)
        {
            if (player != Networking.LocalPlayer)
                return;

            this.uiRunning = false;
            this.animator.SetBool("ShowControls", false);
        }

        public float raycastElapsed = 0;
        public float autoHideTimeout = 5f;
        private RaycastHit lastHitInfo;

        private void RaycastActivityUpdate()
        {
            if (object.Equals(this.hitInfo, null))
                return;
            if (object.Equals(this.lastHitInfo, null))
                this.lastHitInfo = this.hitInfo;

            float distance = Vector3.Distance(this.lastHitInfo.point, this.hitInfo.point);

            if (distance < 0.1f)
            {
                if (this.raycastElapsed <= this.autoHideTimeout)
                    this.raycastElapsed += this.raycastIntervalSeconds;
            }
            else
            {
                this.animator.SetBool("ShowControls", true);
                this.raycastElapsed = 0;
            }

            this.lastHitInfo = this.hitInfo;

            if (this.raycastElapsed >= this.autoHideTimeout)
            {
                this.animator.SetBool("ShowControls", false);
            }
        }

        private void FixedUpdate()
        {
            if (!this.uiRunning)
                return;

            this.elapsed += Time.fixedUnscaledDeltaTime;
            if (this.elapsed < this.raycastIntervalSeconds)
                return;
            this.elapsed = 0;

            if (Networking.LocalPlayer == null || !Networking.LocalPlayer.IsValid())
                return;

            bool desktopHit = this.CheckDesktopHit();
            bool vrHit = this.CheckVRHit();

            bool isLookingAtUi =
                (desktopHit || vrHit) && hitInfo.transform.gameObject == this.raycastTarget;

            if (isLookingAtUi)
            {
                this.RaycastActivityUpdate();
                return;
            }

            bool shown = this.animator.GetBool("ShowControls");
            if (!shown)
                return;

            this.animator.SetBool("ShowControls", false);
        }

        #endregion

        #region Outputs

        protected override void _Start()
        {
            this.OnSubtitleLanguageOptionsChange(new string[0][]);
        }

        private string currentLanguage = "en";

        public void OnSubtitleSelected()
        {
            string selected = (string)this.subtitlesDropdown.GetValue();
            this.currentLanguage = selected;
            this.ToggleSubtitles(true);
        }

        private bool subtitlesOn = false;

        public void OnToggleSubtitles()
        {
            bool newState = !this.subtitlesOn;
            this.ToggleSubtitles(newState);
        }

        private void ToggleSubtitles(bool newState)
        {
            this.events.OnSubtitleLanguageRequested(newState ? this.currentLanguage : string.Empty);
        }

        protected override void OnSubtitleLanguageRequested(string language)
        {
            bool newState = !string.IsNullOrEmpty(language);
            this.subtitlesToggleButtonIcon.color = newState ? Color.black : Color.white;
            this.subtitlesToggleButtonImage.color = newState ? Color.white : new Color(0, 0, 0, 0);
            this.subtitlesDropdown.button.interactable = newState;
            this.subtitlesDropdown.animator.SetBool("DropdownOpen", false);
            this.subtitlesOn = newState;
        }

        protected override void OnSubtitleLanguageOptionsChange(string[][] newOptions)
        {
            this.subtitlesDropdown.SetListener(this, "OnSubtitleSelected");
            this.subtitlesDropdown.SetOptions(newOptions);
            this.subtitlesDropdown.animator.SetBool("DropdownOpen", false);

            if (newOptions.Length == 0)
            {
                this.subtitlesButton.interactable = false;
                this.subtitlesButtonImage.color = new Color(0, 0, 0, 0);
                this.subtitlesButtonLabel.color = Color.white;
                this.subtitlesButtonIcon.color = Color.white;
                this.subtitlesToggleButtonIcon.sprite = this.subtitlesUnavailable;
                this.subtitlesToggleButton.interactable = false;
                this.subtitlesDropdown.button.interactable = false;
            }
            else
            {
                this.subtitlesButton.interactable = true;
                this.subtitlesButtonImage.color = Color.white;
                this.subtitlesButtonLabel.color = Color.black;
                this.subtitlesButtonIcon.color = Color.black;
                this.subtitlesToggleButtonIcon.sprite = this.subtitlesAvailable;
                this.subtitlesToggleButton.interactable = true;
                this.subtitlesDropdown.button.interactable = this.subtitlesOn;
            }

            if (!this.subtitlesOn)
                return;

            // Automatically re-select the current language after the video was changed, so the user doesn't have to
            // look for their language every time the video changes
            foreach (string[] option in newOptions)
            {
                if (option[0] == this.currentLanguage)
                    this.events.OnSubtitleLanguageRequested(option[0]);
            }
        }

        private void RenderMetadata(
            string title,
            string uploader,
            string description,
            int viewCount,
            int likeCount
        )
        {
            this.titleSlot.text = title;
            this.uploaderSlot.text = uploader;
            this.descriptionSlot.text = description;
            if (viewCount > 0)
                this.viewCountSlot.text = $"{viewCount} views";
            if (likeCount > 0)
                this.likeCountSlot.text = $"{likeCount} likes";
        }

        private void ClearMetadata()
        {
            this.titleSlot.text = "";
            this.uploaderSlot.text = "";
            this.descriptionSlot.text = "";
            this.viewCountSlot.text = "DecentM.VideoPlayer";
            this.likeCountSlot.text = "";
        }

        protected override void OnMetadataChange(
            string title,
            string uploader,
            string siteName,
            int viewCount,
            int likeCount,
            string resolution,
            int fps,
            string description,
            string duration,
            TextAsset[] subtitles
        )
        {
            this.RenderMetadata(title, uploader, description, viewCount, likeCount);
        }

        protected override void OnProgress(float timestamp, float duration)
        {
            this.status.text = this.GetProgressIndicator(timestamp, duration);
            if (!float.IsInfinity(timestamp) && !float.IsInfinity(duration))
                this.progress.SetValueWithoutNotify(Mathf.Max(timestamp / duration, 0.001f));
        }

        protected override void OnAutoRetry(int attempt)
        {
            this.animator.SetBool("Loading", true);
            this.status.text = $"Retrying (attempt {attempt + 1})...";
        }

        protected override void OnAutoRetryAbort()
        {
            this.isLoading = false;
        }

        protected override void OnPlayerSwitch(VideoPlayerHandlerType type)
        {
            this.status.text = "Trying with a different player...";
        }

        protected override void OnAutoRetryLoadTimeout(int timeout)
        {
            this.status.text = $"Load timeout after {timeout} seconds, retrying...";
        }

        protected override void OnLoadRequested(VRCUrl url)
        {
            this.status.text = "Checking URL...";
        }

        protected override void OnLoadApproved(VRCUrl url)
        {
            this.animator.SetBool("Loading", true);
            this.ClearMetadata();
            this.isLoading = true;
            this.status.text = "Waiting for video player...";
            this.RenderScreen(0);
            this.OnSubtitleLanguageOptionsChange(new string[0][]);
        }

        protected override void OnLoadBegin()
        {
            this.animator.SetBool("Loading", true);
            this.status.text = "Loading...";
        }

        protected override void OnLoadBegin(VRCUrl url)
        {
            this.animator.SetBool("Loading", true);
            this.status.text = "Loading...";
            this.urlInput.SetUrl(url);
        }

        protected override void OnVideoPlayerInit()
        {
            this.RenderScreen(0);
        }

        protected override void OnLoadDenied(VRCUrl url, string reason)
        {
            this.urlInput.SetUrl(this.emptyUrl);

            // TODO Display the reason to the user (via a notification of some sort?)
            // (the status text isn't visible at this time, because the URL input field is shown)
            this.status.text = reason;
        }

        private void StoppedScreen(float duration)
        {
            this.playButton.interactable = this.selfOwned;
            this.pauseButton.interactable = false;
            this.stopButton.interactable = false;
            this.progress.interactable = this.selfOwned;

            this.urlInput.gameObject.SetActive(this.selfOwned);
            this.enterButton.gameObject.SetActive(this.selfOwned);

            this.status.gameObject.SetActive(false);
            this.progress.gameObject.SetActive(false);
        }

        private void LoadingScreen(float duration)
        {
            this.playButton.interactable = this.selfOwned;
            this.pauseButton.interactable = false;
            this.stopButton.interactable = this.selfOwned;
            this.progress.interactable = false;

            this.urlInput.gameObject.SetActive(false);
            this.enterButton.gameObject.SetActive(false);

            this.status.gameObject.SetActive(true);
            this.progress.gameObject.SetActive(false);
        }

        private void PausedScreen(float duration)
        {
            this.playButton.interactable = this.selfOwned;
            this.pauseButton.interactable = false;
            this.stopButton.interactable = this.selfOwned;
            this.progress.interactable = this.selfOwned;

            this.urlInput.gameObject.SetActive(false);
            this.enterButton.gameObject.SetActive(false);

            this.status.gameObject.SetActive(true);
            this.progress.gameObject.SetActive(!float.IsInfinity(duration));
        }

        private void PlayingScreen(float duration)
        {
            this.playButton.interactable = false;
            this.pauseButton.interactable = this.selfOwned && !float.IsInfinity(duration);
            this.stopButton.interactable = this.selfOwned;
            this.progress.interactable = this.selfOwned;

            this.urlInput.gameObject.SetActive(false);
            this.enterButton.gameObject.SetActive(false);

            this.status.gameObject.SetActive(true);
            this.progress.gameObject.SetActive(!float.IsInfinity(duration));
        }

        private bool isLoading = false;

        private void RenderScreen(float duration)
        {
            if (this.system.IsPlaying())
            {
                this.PlayingScreen(duration);
                return;
            }

            if (this.isLoading)
            {
                this.LoadingScreen(duration);
                return;
            }

            if (this.system.GetCurrentUrl() == null)
            {
                this.StoppedScreen(duration);
                return;
            }

            this.PausedScreen(duration);
        }

        protected override void OnLoadReady(float duration)
        {
            this.isLoading = false;
            this.status.text = this.selfOwned
                ? "Loaded, press play to begin"
                : "Loaded, waiting for owner to start";
            this.RenderScreen(duration);
            this.animator.SetBool("Loading", false);
        }

        protected override void OnLoadError(VideoError videoError)
        {
            switch (videoError)
            {
                case VideoError.InvalidURL:
                    this.status.text = "Invalid URL";
                    break;

                case VideoError.AccessDenied:
                    this.status.text = "Access denied";
                    break;

                // We don't care about the rest of the errors as they're handled by the AutoRetry plugin
            }

            this.animator.SetBool("Loading", false);
        }

        private Sprite GetBrightnessSprite(float alpha)
        {
            if (alpha < 1f / 3f)
                return this.brightnessLowIcon;
            if (alpha < 2f / 3f)
                return this.brightnessMediumIcon;

            return this.brightnessHighIcon;
        }

        protected override void OnBrightnessChange(float alpha)
        {
            this.brightnessSlider.SetValueWithoutNotify(alpha);
            this.brightnessImage.sprite = this.GetBrightnessSprite(alpha);
        }

        private Sprite GetVolumeSprite(float volume, bool muted)
        {
            if (muted || volume == 0)
                return this.volumeMutedIcon;

            if (volume < 1f / 3f)
                return this.volumeLowIcon;
            if (volume < 2f / 3f)
                return this.volumeMediumIcon;

            return this.volumeHighIcon;
        }

        protected override void OnVolumeChange(float volume, bool muted)
        {
            this.volumeSlider.SetValueWithoutNotify(volume);
            this.volumeImage.sprite = this.GetVolumeSprite(volume, muted);
        }

        protected override void OnMutedChange(bool muted, float volume)
        {
            this.volumeSlider.interactable = !muted;
            this.volumeImage.sprite = this.GetVolumeSprite(volume, muted);
        }

        protected override void OnPlaybackEnd()
        {
            this.ClearMetadata();
            this.status.text = "Playback ended";
            this.RenderScreen(this.system.GetDuration());
        }

        protected override void OnPlaybackStart(float timestamp)
        {
            this.status.text = "Playing...";
            this.RenderScreen(this.system.GetDuration());
        }

        protected override void OnPlaybackStop(float timestamp)
        {
            this.status.text =
                $"Paused - {this.GetProgressIndicator(timestamp, this.system.GetDuration())}";
            this.RenderScreen(this.system.GetDuration());
        }

        protected override void OnUnload()
        {
            this.ClearMetadata();
            this.isLoading = false;
            this.status.text = "Stopped";
            this.RenderScreen(this.system.GetDuration());
            this.urlInput.SetUrl(this.emptyUrl);
            this.animator.SetBool("Loading", false);
        }

        protected override void OnSubtitleRender(string text)
        {
            this.animator.SetBool("SubtitlesOn", true);
            this.subtitleSlot.text = text;
        }

        protected override void OnSubtitleClear()
        {
            this.animator.SetBool("SubtitlesOn", false);
            this.subtitleSlot.text = "";
        }

        protected override void OnScreenResolutionChange(
            ScreenHandler screen,
            float width,
            float height
        )
        {
            if (width / height != 16f / 9f)
            {
                this.info.text = $"{width}x{height}";
            }
            else
            {
                this.info.text = $"{height}p";
            }
        }

        private bool selfOwned = true;

        protected override void OnOwnershipChanged(int previousOwnerId, VRCPlayerApi nextOwner)
        {
            if (nextOwner == null || !nextOwner.IsValid())
                return;

            if (nextOwner != Networking.LocalPlayer)
                this.ownershipButton.image.sprite = this.ownershipTransferIcon;
            else if (this.ownershipLocked)
                this.ownershipButton.image.sprite = this.lockIcon;
            else
                this.ownershipButton.image.sprite = this.unlockIcon;

            this.selfOwned = nextOwner == Networking.LocalPlayer;

            this.ownershipLabel.text = nextOwner.displayName;

            this.RenderScreen(this.system.GetDuration());
        }

        private bool ownershipLocked = false;

        protected override void OnOwnershipSecurityChanged(bool locked)
        {
            this.ownershipLocked = locked;

            this.ownershipButton.image.sprite = locked ? this.lockIcon : this.unlockIcon;
        }

        private VRCPlayerApi[] GetUnloadedPlayers(int[] loadedPlayers)
        {
            VRCPlayerApi[] allPlayers = new VRCPlayerApi[64];
            VRCPlayerApi[] unloadedPlayers = new VRCPlayerApi[0];

            VRCPlayerApi.GetPlayers(allPlayers);

            foreach (VRCPlayerApi player in allPlayers)
            {
                if (player == null || !player.IsValid())
                    continue;

                bool found = false;

                // We always consider the local player as loaded
                if (player != Networking.LocalPlayer)
                {
                    foreach (int loadedPlayer in loadedPlayers)
                    {
                        if (loadedPlayer == player.playerId)
                        {
                            found = true;
                            break;
                        }
                    }
                }

                if (!found)
                {
                    VRCPlayerApi[] tmp = new VRCPlayerApi[unloadedPlayers.Length + 1];
                    Array.Copy(unloadedPlayers, tmp, unloadedPlayers.Length);
                    tmp[tmp.Length - 1] = player;
                    unloadedPlayers = tmp;
                }
            }

            return unloadedPlayers;
        }

        protected override void OnRemotePlayerLoaded(int[] loadedPlayers)
        {
            VRCPlayerApi[] unloadedPlayers = this.GetUnloadedPlayers(loadedPlayers);

            if (unloadedPlayers.Length == 1)
                this.status.text = $"Waiting for {unloadedPlayers[0].displayName}...";
            else
                this.status.text = $"Waiting for {unloadedPlayers.Length} players...";
        }

        #endregion

        #region Inputs

        public void OnPlayButton()
        {
            this.system.StartPlayback();
        }

        public void OnPauseButton()
        {
            this.system.PausePlayback();
        }

        public void OnStopButton()
        {
            this.system.UnloadVideo();
        }

        public void OnBrightnessSlider()
        {
            float brightness = this.system.GetBrightness();

            // Ignore duplicate values
            if (this.brightnessSlider.value == brightness)
                return;

            this.system.SetBrightness(this.brightnessSlider.value);
        }

        public void OnVolumeSlider()
        {
            float volume = this.system.GetVolume();

            // Ignore duplicate values
            if (this.volumeSlider.value == volume)
                return;

            this.system.SetVolume(this.volumeSlider.value);
        }

        public void OnMuteButton()
        {
            this.system.SetMuted(!this.system.GetMuted());
        }

        public void OnProgressSlider()
        {
            this.system.Seek(this.progress.value * this.system.GetDuration());
        }

        public void OnUrlInput()
        {
            this.system.RequestVideo(this.urlInput.GetUrl());
        }

        public void OnOwnershipButton()
        {
            if (selfOwned)
            {
                this.events.OnOwnershipSecurityChanged(!this.ownershipLocked);
            }
            else
            {
                this.events.OnOwnershipRequested();
            }
        }

        #endregion
    }
}
