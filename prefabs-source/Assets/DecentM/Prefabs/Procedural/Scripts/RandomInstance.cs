using UnityEngine;
using UdonSharp;

/*
 * Shoutout to Unity for making Random a singleton,
 * and shoutout to VRChat for not exposing Random.state to Udon.
 * Enter this class that solves both problems in a way that screws with
 * everything else that dares use Random directly without going through
 * RandomInstance.
 *
 * Providing a seed will ensure that the random values are deterministic.
 * If no seed is set, it behaves just like normal Random.
 */

namespace DecentM.Tools
{
    [UdonBehaviourSyncMode(BehaviourSyncMode.None)]
    public class RandomInstance : UdonSharpBehaviour
    {
        private int seed = -1;

        public void SetSeed(int seed)
        {
            this.seed = seed;
        }

        private bool initialised
        {
            get { return this.seed >= 0; }
        }

        public float Range(float min, float max)
        {
            if (this.initialised)
                Random.InitState(seed++);

            return Random.Range(min, max);
        }

        public int Range(int min, int max)
        {
            if (this.initialised)
                Random.InitState(seed++);

            return Random.Range(min, max);
        }

        public Color ColorHSV()
        {
            if (this.initialised)
                Random.InitState(seed++);

            return Random.ColorHSV();
        }

        public Color ColorHSV(float hueMin, float hueMax)
        {
            if (this.initialised)
                Random.InitState(seed++);

            return Random.ColorHSV(hueMin, hueMax);
        }

        public Color ColorHSV(float hueMin, float hueMax, float saturationMin, float saturationMax)
        {
            if (this.initialised)
                Random.InitState(seed++);

            return Random.ColorHSV(hueMin, hueMax, saturationMin, saturationMax);
        }

        public Color ColorHSV(
            float hueMin,
            float hueMax,
            float saturationMin,
            float saturationMax,
            float valueMin,
            float valueMax
        )
        {
            if (this.initialised)
                Random.InitState(seed++);

            return Random.ColorHSV(
                hueMin,
                hueMax,
                saturationMin,
                saturationMax,
                valueMin,
                valueMax
            );
        }

        public Color ColorHSV(
            float hueMin,
            float hueMax,
            float saturationMin,
            float saturationMax,
            float valueMin,
            float valueMax,
            float alphaMin,
            float alphaMax
        )
        {
            if (this.initialised)
                Random.InitState(seed++);

            return Random.ColorHSV(
                hueMin,
                hueMax,
                saturationMin,
                saturationMax,
                valueMin,
                valueMax,
                alphaMin,
                alphaMax
            );
        }

        public float value
        {
            get
            {
                if (this.initialised)
                    Random.InitState(seed++);

                return Random.value;
            }
        }

        public Quaternion rotation
        {
            get
            {
                if (this.initialised)
                    Random.InitState(seed++);

                return Random.rotation;
            }
        }

        public Quaternion rotationUniform
        {
            get
            {
                if (this.initialised)
                    Random.InitState(seed++);

                return Random.rotationUniform;
            }
        }

        public Vector3 onUnitSphere
        {
            get
            {
                if (this.initialised)
                    Random.InitState(seed++);

                return Random.onUnitSphere;
            }
        }

        public Vector3 insideUnitSphere
        {
            get
            {
                if (this.initialised)
                    Random.InitState(seed++);

                return Random.insideUnitSphere;
            }
        }

        public Vector2 insideUnitCircle
        {
            get
            {
                if (this.initialised)
                    Random.InitState(seed++);

                return Random.insideUnitCircle;
            }
        }
    }
}
