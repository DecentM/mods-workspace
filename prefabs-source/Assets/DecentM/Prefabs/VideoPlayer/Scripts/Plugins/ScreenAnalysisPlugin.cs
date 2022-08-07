using System;
using System.Collections;
using JetBrains.Annotations;

using UnityEngine;

namespace DecentM.Prefabs.VideoPlayer.Plugins
{
    public class ScreenAnalysisPlugin : VideoPlayerPlugin
    {
        public float targetFps = 30;
        public float historyLengthSeconds = 0.3333f;
        public int sampleSize = 40;
        public float sampleRandomisationFactor = 1f;

        private bool isRunning = false;

        public Texture2D fetchTexture;

        private float elapsed = 0;
        private float fps = 0;

        private Camera camera;

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

        public void Reset()
        {
            this.fps = this.targetFps;
            int length = Mathf.CeilToInt(this.targetFps * this.historyLengthSeconds);
            this.UpdateValues(new Color[] { Color.black });
        }

        private void LateUpdate()
        {
            if (!this.isRunning || this.camera == null)
                return;

            this.elapsed += Time.unscaledDeltaTime;
            if (elapsed < 1f / this.fps)
                return;
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

        public Color GetMostVibrant(Color[] colours)
        {
            Color result = Color.black;

            foreach (Color colour in colours)
            {
                float vibrance = GetVibrance(colour);
                if (vibrance > GetVibrance(result))
                    result = colour;
            }

            return result;
        }

        private float GetBrightness(Color colour)
        {
            return (colour.r + colour.g + colour.b) / 3;
        }

        public Color GetBrightest(Color[] colours)
        {
            Color result = Color.black;

            foreach (Color colour in colours)
            {
                float brightness = GetBrightness(colour);
                if (brightness > GetBrightness(result))
                    result = colour;
            }

            return result;
        }

        public Color GetAverage(Color[] colors)
        {
            if (colors == null || colors.Length == 0)
                return Color.black;

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

        private Vector2Int[] GenerateSamplePoints(int sampleCount, Vector2Int size)
        {
            Vector2Int[] result = new Vector2Int[sampleCount];
            int skip = size.x * size.y / sampleCount;
            float twitchXMultiplier = size.x / sampleCount;
            float twitchYMultiplier = size.y / sampleCount;

            for (int i = 0; i < sampleCount; i++)
            {
                Vector2Int rawReadCoords = GetCoordsForIndex(i * skip, size.x);

                float twitchX =
                    UnityEngine.Random.Range(-1, 1)
                    * twitchXMultiplier
                    * this.sampleRandomisationFactor;
                float twitchY =
                    UnityEngine.Random.Range(-1, 1)
                    * twitchYMultiplier
                    * this.sampleRandomisationFactor;
                Vector2Int readCoords = new Vector2Int(
                    Mathf.Max(Mathf.Min(Mathf.RoundToInt(rawReadCoords.x + twitchX), size.x), 0),
                    Mathf.Max(Mathf.Min(Mathf.RoundToInt(rawReadCoords.y + twitchY), size.y), 0)
                );

                result[i] = readCoords;
            }

            return result;
        }

        private Color[] SampleFetchTexture(Vector2Int[] samplePoints)
        {
            Color[] result = new Color[samplePoints.Length];

            for (int i = 0; i < samplePoints.Length; i++)
            {
                Vector2Int readCoords = samplePoints[i];

                fetchTexture.ReadPixels(new Rect(readCoords.x, readCoords.y, 1, 1), 0, 0);
                Color colour = fetchTexture.GetPixel(0, 0);

                result[i] = colour;
            }

            return result;
        }

        private Color average = Color.black;

        [PublicAPI]
        public Color GetAverage()
        {
            return average;
        }

        private Color brightest = Color.black;

        [PublicAPI]
        public Color GetBrightest()
        {
            return brightest;
        }

        private Color mostVibrant = Color.black;

        [PublicAPI]
        public Color GetMostVibrant()
        {
            return mostVibrant;
        }

        private void OnPostRender()
        {
            Vector2Int[] samplePoints = this.GenerateSamplePoints(
                Mathf.Min(Mathf.Max(this.sampleSize, 1), 1000),
                new Vector2Int(this.camera.scaledPixelWidth, this.camera.scaledPixelHeight)
            );
            Color[] colours = SampleFetchTexture(samplePoints);

            this.UpdateValues(colours);
        }

        private void UpdateValues(Color[] colours)
        {
            this.average = this.GetAverage(colours);
            this.brightest = this.GetBrightest(colours);
            this.mostVibrant = this.GetMostVibrant(colours);
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
            this.fps = fps <= 0 ? this.targetFps : Mathf.Min(fps, targetFps);
        }

        protected override void OnPlaybackEnd()
        {
            this.isRunning = false;
            this.Reset();
        }

        protected override void OnLoadApproved(string url)
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
