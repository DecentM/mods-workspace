using System;
using System.Collections;
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

namespace DecentM.VideoPlayer.Plugins
{
    public class RealtimeLightOutput : VideoPlayerPlugin
    {
        public float targetFps = 30;
        public float blackCutoff = 0.2f;
        public float lightFadeTimeSeconds = 0.5f;
        public float brightnessOffset = 0.15f;
        public bool enableSmoothing = true;
        public bool applyOutputTexture = true;

        private bool isRunning = false;

        public Texture2D fetchTexture;
        public Texture2D historyTexture;
        public Texture2D outputTexture;
        public Texture2D smoothedAverageColourTexture;
        public Texture2D primaryColourTexture;
        public Texture2D brightestRecentColourTexture;

        private Light[] lights;
        private float elapsed = 0;
        private float fps = 0;

        private new Camera camera;

        protected override void _Start()
        {
            this.camera = GetComponent<Camera>();

            if (this.fetchTexture == null)
            {
                Debug.LogError($"Missing fetch texture!");
                this.enabled = false;
                return;
            }

            this.previousColours = new Color[Mathf.CeilToInt(this.targetFps * this.lightFadeTimeSeconds)];
            this.lights = GetComponentsInChildren<Light>();
            this.camera.enabled = false;
        }

        private void LateUpdate()
        {
            if (!this.isRunning || !this.camera) return;

            this.elapsed += Time.unscaledDeltaTime;
            if (elapsed < 1f / this.fps) return;
            elapsed = 0;

            camera.Render();
        }

        private float GetProximity(Color colourA, Color colourB)
        {
            return (colourB.r - colourA.r) + (colourB.b - colourA.b) + (colourB.b - colourA.b);
        }

        private Color[] previousColours;

        private Color[] AddColourHistory(Color[] history, Color colour)
        {
            Color[] tmp = new Color[this.previousColours.Length];
            Array.Copy(this.previousColours, 0, tmp, 1, this.previousColours.Length - 1);
            tmp[0] = colour;
            return tmp;
        }

        private bool IsAboveCutoff(Color colour)
        {
            return colour.r + colour.g + colour.b > this.blackCutoff;
        }

        private Color AdjustBrightness(Color colour, float offset)
        {
            return new Color(colour.r + offset, colour.g + offset, colour.b + offset, colour.a);
        }

        private Color GetAverage(Color[] colors)
        {
            if (colors == null || colors.Length == 0) return Color.black;

            Color average = new Color();

            foreach (Color color in colors)
            {
                average.r += color.r;
                average.g += color.g;
                average.b += color.b;
            }

            average.r /= colors.Length;
            average.g /= colors.Length;
            average.b /= colors.Length;

            return average;
        }

        private Vector2Int GetCoordsForIndex(int index, int width)
        {
            Vector2Int result = new Vector2Int();

            result.x = index % width;
            result.y = Mathf.FloorToInt(index / width);

            return result;
        }

        private void ResizeFetchTexture(Texture2D outputTexture, Vector2Int oldSize)
        {
            float widthSkip = (float)oldSize.x / outputTexture.width;
            float heightSkip = (float)oldSize.y / outputTexture.height;

            Vector2Int center = new Vector2Int(oldSize.x / 2, oldSize.y / 2);

            for (int i = 0; i < outputTexture.width * outputTexture.height; i++)
            {
                Vector2Int writeCoords = GetCoordsForIndex(i, outputTexture.width);

                Vector2Int rawReadCoords = new Vector2Int(Mathf.FloorToInt(writeCoords.x * widthSkip), Mathf.FloorToInt(writeCoords.y * heightSkip));
                Vector2Int readCoords = new Vector2Int(rawReadCoords.x + (center.x - rawReadCoords.x) / outputTexture.width / 3, rawReadCoords.y + (center.y - rawReadCoords.y) / outputTexture.height / 3);

                fetchTexture.ReadPixels(new Rect(readCoords.x, readCoords.y, 1, 1), 0, 0);
                Color colour = fetchTexture.GetPixel(0, 0);

                outputTexture.SetPixel(writeCoords.x, outputTexture.height - writeCoords.y, colour);
            }
        }

        private void UpdateTexture(Texture2D texture, Color[] colours)
        {
            // Create an output of all blacks in case there are less colours in the input
            Color[] output = new Color[texture.width * texture.height];
            for (int i = 0; i < output.Length; i++) output[i] = Color.black;

            // Copy all colours to the output array and throw away ones that wouldn't fit
            if (colours.Length <= output.Length) Array.Copy(colours, output, colours.Length);
            else Array.Copy(colours, output, output.Length);

            texture.SetPixels(output);
            texture.Apply();
        }

        private void SetLights(Color colour)
        {
            foreach (Light light in this.lights)
            {
                light.color = this.AdjustBrightness(colour, this.brightnessOffset);
            }
        }

        private void OnPostRender()
        {
            this.ResizeFetchTexture(this.outputTexture, new Vector2Int(this.camera.scaledPixelWidth, this.camera.scaledPixelHeight));

            if (this.applyOutputTexture) outputTexture.Apply();

            Color[] colours = this.outputTexture.GetPixels();
            Color average;

            if (this.enableSmoothing)
            {
                this.previousColours = this.AddColourHistory(this.previousColours, this.GetAverage(colours));
                if (this.historyTexture != null) this.UpdateTexture(this.historyTexture, this.previousColours);

                average = this.GetAverage(this.previousColours);
            }
            else
            {
                average = this.GetAverage(colours);
            }

            bool lightsOn = this.IsAboveCutoff(average);
            this.ToggleLights(lightsOn);
            if (!lightsOn) return;
            this.SetLights(average);
        }

        private void ToggleLights(bool state)
        {
            foreach (Light light in this.lights)
            {
                light.enabled = state;
            }
        }

        protected override void OnMetadataChange(string title, string uploader, string siteName, int viewCount, int likeCount, string resolution, int fps, string description, string duration, string[][] subtitles)
        {
            this.fps = fps <= 0 ? this.targetFps : Mathf.Min(fps, targetFps);
            this.previousColours = new Color[Mathf.CeilToInt(this.fps * this.lightFadeTimeSeconds)];
        }

        protected override void OnPlaybackEnd()
        {
            this.isRunning = false;
            this.ToggleLights(false);
        }

        protected override void OnPlaybackStart(float timestamp)
        {
            this.isRunning = true;
        }

        protected override void OnPlaybackStop(float timestamp)
        {
            this.isRunning = false;
        }

        protected override void OnUnload()
        {
            this.isRunning = false;
            this.ToggleLights(false);
        }
    }
}

