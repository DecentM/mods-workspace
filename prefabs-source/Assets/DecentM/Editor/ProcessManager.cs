using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Diagnostics;
using System.Text;
using System.Threading;
using System.IO;

namespace DecentM.EditorTools
{
    public struct ProcessResult
    {
        public Process process;
        public string stdout;
        public string stderr;
    }

    public static class ProcessManager
    {
        private static IEnumerator ProcessCoroutine(Process process, int timeout, Action<Process> callback)
        {
            StreamReader read = process.StandardOutput;

            /* while (read.Peek() == 0)
                yield return new WaitForSeconds(0.1f); */

            while (!process.HasExited)
                yield return new WaitForSeconds(0.1f);

            callback(process);
            read.Close();
        }

        private static Process CreateProcess(string filename, string arguments)
        {
            Process process = new Process();

            process.StartInfo.FileName = filename;
            process.StartInfo.Arguments = arguments;
            process.StartInfo.UseShellExecute = false;
            process.StartInfo.RedirectStandardOutput = true;
            process.StartInfo.RedirectStandardError = true;
            process.StartInfo.WindowStyle = ProcessWindowStyle.Hidden;
            process.StartInfo.CreateNoWindow = true;

            return process;
        }

        public static IEnumerator CreateProcessCoroutine(string filename, string arguments, int timeout, Action<Process> callback)
        {
            Process process = CreateProcess(filename, arguments);
            process.Start();
            return ProcessCoroutine(process, timeout, callback);
        }

        public static EditorCoroutine RunProcessCoroutine(string filename, string arguments, int timeout, Action<Process> callback)
        {
            IEnumerator coroutine = CreateProcessCoroutine(filename, arguments, timeout, callback);
            return EditorCoroutine.Start(coroutine);
        }

        private static ProcessResult RunProcessSync(string filename, string arguments, int timeout)
        {
            using (Process process = new Process())
            {
                process.StartInfo.FileName = filename;
                process.StartInfo.Arguments = arguments;
                process.StartInfo.UseShellExecute = false;
                process.StartInfo.RedirectStandardOutput = true;
                process.StartInfo.RedirectStandardError = true;
                process.StartInfo.WindowStyle = ProcessWindowStyle.Hidden;
                process.StartInfo.CreateNoWindow = true;

                StringBuilder output = new StringBuilder();
                StringBuilder error = new StringBuilder();

                using (AutoResetEvent outputWaitHandle = new AutoResetEvent(false))
                using (AutoResetEvent errorWaitHandle = new AutoResetEvent(false))
                {
                    process.OutputDataReceived += (sender, e) => {
                        if (e.Data == null)
                        {
                            outputWaitHandle.Set();
                        }
                        else
                        {
                            output.AppendLine(e.Data);
                        }
                    };
                    process.ErrorDataReceived += (sender, e) =>
                    {
                        if (e.Data == null)
                        {
                            errorWaitHandle.Set();
                        }
                        else
                        {
                            error.AppendLine(e.Data);
                        }
                    };

                    process.Start();

                    process.BeginOutputReadLine();
                    process.BeginErrorReadLine();

                    ProcessResult result = new ProcessResult();

                    if (process.WaitForExit(timeout) &&
                        outputWaitHandle.WaitOne(timeout) &&
                        errorWaitHandle.WaitOne(timeout))
                    {
                        result.process = process;
                        result.stdout = output.ToString();
                        result.stderr = error.ToString();

                        return result;
                    }
                    else
                    {
                        result.process = process;
                        result.stdout = output.ToString();
                        result.stderr = error.ToString();

                        return result;
                    }
                }
            }
        }
    }
}
