using System;
using System.Collections;
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

namespace DecentM.VideoPlayer.Plugins
{
    public class ScreenAnalysisPlugin : VideoPlayerPlugin
    {
        public float targetFps = 30;
        public float blackCutoff = 0.2f;
        public float historyLengthSeconds = 0.3333f;

        private bool isRunning = false;

        public Texture2D fetchTexture;
        public Texture2D outputTexture;

        public Texture2D averageColourHistoryTexture;
        public Texture2D smoothedAverageColourHistoryTexture;

        public Texture2D mostVibrantColourHistoryTexture;
        public Texture2D smoothedMostVibrantColourHistoryTexture;

        public Texture2D brightestColourHistoryTexture;
        public Texture2D smoothedBrightestColourHistoryTexture;

        private float elapsed = 0;
        private float fps = 0;

        private new Camera camera;

        protected override void _Start()
        {
            this.camera = GetComponent<Camera>();

            if (this.fetchTexture == null)
            {
                Debug.LogError($"[ScreenAnalysisPlugin] Missing fetch texture!");
                this.enabled = false;
                return;
            }

            this.camera.enabled = false;

            this.Reset();
        }

        private void Reset()
        {
            int length = Mathf.CeilToInt(this.targetFps * this.historyLengthSeconds);
            this.averageHistory = new Color[length];
            this.smoothedAverageHistory = new Color[length];
            this.mostVibrantHistory = new Color[length];
            this.smoothedMostVibrantHistory = new Color[length];
            this.brightestHistory = new Color[length];
            this.smoothedBrightestHistory = new Color[length];
        }

        private void LateUpdate()
        {
            if (!this.isRunning || !this.camera) return;

            this.elapsed += Time.unscaledDeltaTime;
            if (elapsed < 1f / this.fps) return;
            elapsed = 0;

            camera.Render();
        }

        private float GetVibrance(Color colour)
        {
            float rgDiff = Mathf.Abs(colour.r - colour.g);
            float rbDiff = Mathf.Abs(colour.r - colour.b);
            float gbDiff = Mathf.Abs(colour.g - colour.b);

            return (rgDiff + rbDiff + gbDiff) / 3;
        }

        private Color GetMostVibrant(Color[] colours)
        {
            Color result = Color.black;

            foreach (Color colour in colours)
            {
                float vibrance = GetVibrance(colour);
                if (vibrance > GetVibrance(result)) result = colour;
            }

            return result;
        }

        private float GetBrightness(Color colour)
        {
            return (colour.r + colour.g + colour.b) / 3;
        }

        private Color GetBrightest(Color[] colours)
        {
            Color result = Color.black;

            foreach (Color colour in colours)
            {
                float brightness = GetBrightness(colour);
                if (brightness > GetBrightness(result)) result = colour;
            }

            return result;
        }

        private Color[] AddColourHistory(Color[] history, Color colour)
        {
            Color[] tmp = new Color[history.Length];
            Array.Copy(history, 0, tmp, 1, history.Length - 1);
            tmp[0] = colour;
            return tmp;
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

        private Color[] averageHistory;
        private Color[] smoothedAverageHistory;
        private Color[] brightestHistory;
        private Color[] smoothedBrightestHistory;
        private Color[] mostVibrantHistory;
        private Color[] smoothedMostVibrantHistory;

        private void OnPostRender()
        {
            this.ResizeFetchTexture(this.outputTexture, new Vector2Int(this.camera.scaledPixelWidth, this.camera.scaledPixelHeight));

            outputTexture.Apply();

            Color[] colours = this.outputTexture.GetPixels();

            if (this.averageColourHistoryTexture != null)
            {
                this.averageHistory = this.AddColourHistory(this.averageHistory, this.GetAverage(colours));
                this.UpdateTexture(this.averageColourHistoryTexture, this.averageHistory);
            }

            if (this.smoothedAverageColourHistoryTexture != null)
            {
                this.smoothedAverageHistory = this.AddColourHistory(this.smoothedAverageHistory, this.GetAverage(this.averageHistory));
                this.UpdateTexture(this.smoothedAverageColourHistoryTexture, this.smoothedAverageHistory);
            }

            if (this.brightestColourHistoryTexture != null)
            {
                this.brightestHistory = this.AddColourHistory(this.brightestHistory, this.GetBrightest(colours));
                this.UpdateTexture(this.brightestColourHistoryTexture, this.brightestHistory);
            }

            if (this.smoothedBrightestColourHistoryTexture != null)
            {
                this.smoothedBrightestHistory = this.AddColourHistory(this.smoothedBrightestHistory, this.GetAverage(this.brightestHistory));
                this.UpdateTexture(this.smoothedBrightestColourHistoryTexture, this.smoothedBrightestHistory);
            }

            if (this.mostVibrantColourHistoryTexture != null)
            {
                this.mostVibrantHistory = this.AddColourHistory(this.mostVibrantHistory, this.GetMostVibrant(colours));
                this.UpdateTexture(this.mostVibrantColourHistoryTexture, this.mostVibrantHistory);
            }

            if (this.smoothedMostVibrantColourHistoryTexture != null)
            {
                this.smoothedMostVibrantHistory = this.AddColourHistory(this.smoothedMostVibrantHistory, this.GetAverage(this.mostVibrantHistory));
                this.UpdateTexture(this.smoothedMostVibrantColourHistoryTexture, this.smoothedMostVibrantHistory);
            }
        }

        protected override void OnMetadataChange(string title, string uploader, string siteName, int viewCount, int likeCount, string resolution, int fps, string description, string duration, string[][] subtitles)
        {
            this.fps = fps <= 0 ? this.targetFps : Mathf.Min(fps, targetFps);
        }

        protected override void OnPlaybackEnd()
        {
            this.isRunning = false;
            this.Reset();
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
            this.Reset();
        }
    }
}

