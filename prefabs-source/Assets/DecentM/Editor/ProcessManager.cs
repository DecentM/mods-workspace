using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Diagnostics;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.IO;

namespace DecentM.EditorTools
{
    public struct ProcessResult
    {
        public string stdout;
        public string stderr;
    }

    public enum BlockingBehaviour
    {
        Blocking,
        NonBlocking,
    }

    public static class ProcessManager
    {
        private static Process StartProcess(string filename, string arguments, string workdir, Action<Process> OnExited, Action<Process, DataReceivedEventArgs> OnOutputDataReceived, Action<Process, DataReceivedEventArgs> OnErrorDataReceived)
        {
            Process process = new Process
            {
                StartInfo =
                {
                    FileName = filename,
                    Arguments = arguments,
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    WindowStyle = ProcessWindowStyle.Hidden,
                    CreateNoWindow = true,
                    WorkingDirectory = workdir,
                },

                EnableRaisingEvents = true
            };

            process.Exited += (o, e) => OnExited(process);
            process.OutputDataReceived += (o, e) => OnOutputDataReceived(process, e);
            process.ErrorDataReceived += (o, e) => OnErrorDataReceived(process, e);

            process.Start();

            process.BeginOutputReadLine();
            process.BeginErrorReadLine();

            return process;
        }

        public static void RunProcess(string filename, string arguments, string workdir, Action<ProcessResult> OnFinished)
        {
            StringBuilder stdout = new StringBuilder();
            StringBuilder stderr = new StringBuilder();

            void OnExited(Process process) {
                ProcessResult result = new ProcessResult();

                result.stdout = stdout.ToString();
                result.stderr = stderr.ToString();

                OnFinished(result);
            };

            void OnOutputDataReceived(Process process, DataReceivedEventArgs e)
            {
                stdout.Append(e.Data);
            }

            void OnErrorDataReceived(Process process, DataReceivedEventArgs e)
            {
                stderr.Append(e.Data);
            }

            StartProcess(filename, arguments, workdir, OnExited, OnOutputDataReceived, OnErrorDataReceived);
        }

        /* public static void RunProcessAsync(string filename, string arguments, string workdir, Action<ProcessResult> callback)
        {
            RunProcessAsync(filename, arguments, workdir, 15000, callback);
        }

        public static void RunProcessAsync(string filename, string arguments, Action<ProcessResult> callback)
        {
            RunProcessAsync(filename, arguments, ".", 15000, callback);
        }

        public static void RunProcessAsync(string filename, string arguments, int timeout, Action<ProcessResult> callback)
        {
            RunProcessAsync(filename, arguments, ".", timeout, callback);
        }

        public static Task<ProcessResult> RunProcessAsync(string filename, string arguments, string workdir)
        {
            var tcs = new TaskCompletionSource<ProcessResult>();
            Process process = CreateProcess(filename, arguments, workdir);
            ProcessResult result = new ProcessResult();

            StringBuilder output = new StringBuilder();
            StringBuilder error = new StringBuilder();

            process.Exited += (sender, args) =>
            {
                result.stdout = output.ToString();
                result.stderr = error.ToString();

                tcs.SetResult(result);
                process.Dispose();
            };

            process.Start();

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

                process.BeginOutputReadLine();
                process.BeginErrorReadLine();
            }

            return tcs.Task;
        }

        public static void RunProcessAsync(string filename, string arguments, string workdir, int timeout, Action<ProcessResult> callback)
        {
            Task.Run(() =>
            {
                ProcessResult result = RunProcessSync(filename, arguments, workdir, timeout);
                callback(result);
            });
        }

        public static ProcessResult RunProcessSync(string filename, string arguments, string workdir)
        {
            return RunProcessSync(filename, arguments, workdir, 15000);
        }

        public static ProcessResult RunProcessSync(string filename, string arguments, string workdir, int timeout)
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
                process.StartInfo.WorkingDirectory = workdir;

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
                        result.stdout = output.ToString();
                        result.stderr = error.ToString();

                        return result;
                    }
                    else
                    {
                        result.stdout = output.ToString();
                        result.stderr = error.ToString();

                        return result;
                    }
                }
            }
        } */
    }
}
