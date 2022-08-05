using System;
using System.Diagnostics;
using System.Text;
using System.Threading;

namespace DecentM.Shared
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
        private static Process StartProcess(string filename, string arguments, string workdir)
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
                EnableRaisingEvents = true,
            };

            process.Start();

            return process;
        }

        public static void RunProcess(
            string filename,
            string arguments,
            string workdir,
            Action<ProcessResult> OnFinished
        )
        {
            StringBuilder stdout = new StringBuilder();
            StringBuilder stderr = new StringBuilder();

            Process runProcess = StartProcess(filename, arguments, workdir);

#if UNITY_EDITOR
            DCoroutine.Start(Parallelism.WaitForProcess(runProcess, OnFinished));
#else
            runProcess.Exited += new EventHandler((object sender, System.EventArgs e) =>
            {
                ProcessResult result = new ProcessResult();

                result.stdout = stdout.ToString();
                result.stderr = stderr.ToString();

                OnFinished(result);
            });
#endif
        }

        public static ProcessResult RunProcessSync(
            string filename,
            string arguments,
            string workdir
        )
        {
            return RunProcessSync(filename, arguments, workdir, 15000);
        }

        public static ProcessResult RunProcessSync(
            string filename,
            string arguments,
            string workdir,
            int timeout
        )
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
                    process.OutputDataReceived += (sender, e) =>
                    {
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

                    if (
                        process.WaitForExit(timeout)
                        && outputWaitHandle.WaitOne(timeout)
                        && errorWaitHandle.WaitOne(timeout)
                    )
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
        }
    }
}
