using System;
using System.Collections;
using System.Collections.Generic;

#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;

namespace DecentM.Shared
{
    public class DCoroutine
    {
        private static List<DCoroutine> runningCoroutines = new List<DCoroutine>();

        public static DCoroutine Start(IEnumerator routine)
        {
            DCoroutine coroutine = new DCoroutine(routine);
            coroutine.Start();
            return coroutine;
        }

        public static DCoroutine Start(Action action)
        {
            DCoroutine coroutine = new DCoroutine(action);
            coroutine.Start();
            return coroutine;
        }

        readonly IEnumerator routine;

        DCoroutine(IEnumerator _routine)
        {
            routine = _routine;
        }

        readonly Action action;

        DCoroutine(Action _action)
        {
            action = _action;
        }

        void Start()
        {
            if (runningCoroutines.Contains(this))
                return;

#if UNITY_EDITOR
            EditorApplication.update += this.Update;
#endif
            
            runningCoroutines.Add(this);
        }

        public void Stop()
        {
            if (!runningCoroutines.Contains(this))
                return;

#if UNITY_EDITOR
            EditorApplication.update -= this.Update;
#endif

            runningCoroutines.Remove(this);
        }

        public bool IsRunning
        {
            get { return runningCoroutines.Contains(this); }
        }

        void Update()
        {
            if (routine != null)
            {
                try
                {
                    if (!routine.MoveNext())
                        this.Stop();
                }
                catch (Exception ex)
                {
                    Debug.LogException(ex);
                    Debug.LogError(
                        "A step in this coroutine has thrown an exception, stopping coroutine. There's additional debugging information above."
                    );
                    this.Stop();
                }
            }
            else if (action != null)
            {
                action();
                this.Stop();
            }
            else
            {
                this.Stop();
            }
        }
    }
}
