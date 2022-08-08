using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;

using UnityEngine;

namespace DecentM.Shared
{
    public static class Parallelism
    {
        public static IEnumerator WaitForTask(Task task, Action<bool> OnSettled)
        {
            bool isSettled = false;
            bool isFaulted = false;

            while (!isSettled)
            {
                if (task.IsCompleted || task.IsCanceled || task.IsFaulted)
                {
                    isFaulted = task.IsFaulted;
                    isSettled = true;
                }

                yield return new WaitForSeconds(0.25f);
            }

            if (isSettled)
            {
                OnSettled(!isFaulted);
                yield return null;
            }
        }

        public static IEnumerator WaitForProcess(
            System.Diagnostics.Process process,
            Action<ProcessResult> OnExited
        )
        {
            Task<string> stdoutTask = process.StandardOutput.ReadToEndAsync();
            Task<string> stderrTask = process.StandardError.ReadToEndAsync();

            Task allTask = Task.WhenAny(stdoutTask, stderrTask);

            void OnTasksEnded(bool success)
            {
                ProcessResult result = new ProcessResult();

                result.stdout = stdoutTask.Result;
                result.stderr = stderrTask.Result;

                OnExited(result);
            }

            return WaitForTask(allTask, OnTasksEnded);
        }

        public static IEnumerator WaitForCallback(Action<Action> CallbackReceiver)
        {
            bool isCompleted = false;

            void Callback()
            {
                isCompleted = true;
            }

            CallbackReceiver(Callback);

            while (!isCompleted)
                yield return new WaitForSeconds(0.25f);
        }

#if UNITY_EDITOR
        public static IEnumerator WaitForCoroutines(List<DCoroutine> coroutines, Action OnFinish)
        {
            List<DCoroutine> finishedCoroutines = new List<DCoroutine>();

            while (finishedCoroutines.Count < coroutines.Count)
            {
                foreach (DCoroutine coroutine in coroutines)
                {
                    if (!coroutine.IsRunning && !finishedCoroutines.Contains(coroutine))
                    {
                        finishedCoroutines.Add(coroutine);
                    }
                }

                yield return new WaitForSeconds(0.25f);
            }

            OnFinish();
        }
#endif

        public static Action<T1> WrapCallback<T1>(Action<T1> Callback)
        {
            void WrappedCallback(T1 a1)
            {
                try
                {
                    Callback(a1);
                }
                catch (Exception e)
                {
                    Debug.LogException(e);
                    Debug.LogError(
                        "Error in wrapped callback, this action will not continue. There's additional debugging information above."
                    );
                }
            }

            return WrappedCallback;
        }

        public static Action<T1, T2> WrapCallback<T1, T2>(Action<T1, T2> Callback)
        {
            void WrappedCallback(T1 a1, T2 a2)
            {
                try
                {
                    Callback(a1, a2);
                }
                catch (Exception e)
                {
                    Debug.LogException(e);
                    Debug.LogError(
                        "Error in wrapped callback, this action will not continue. There's additional debugging information above."
                    );
                }
            }

            return WrappedCallback;
        }
    }
}
