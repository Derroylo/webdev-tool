using System;
using System.Threading;
using Spectre.Console;

namespace WebDev.Tool.Helper
{
    internal class ExecCommand
    {
        public static string Exec(string command, bool isInteractive = false, bool disableJobControl = false, int timeoutInSeconds = 300)
        {
            string result = "";

            using (System.Diagnostics.Process proc = new())
            {
                proc.StartInfo.FileName = "/bin/bash";
                proc.StartInfo.Arguments = "-c" + (isInteractive ? "i" : "") + " \"" + (disableJobControl ? "set +m; " : "") + command.Replace("\"", "\\\"") + " \"";
                proc.StartInfo.EnvironmentVariables["WEBDEV_DISABLE_HEADER"] = "true";
                proc.StartInfo.UseShellExecute = false;
                proc.EnableRaisingEvents = true;
                proc.StartInfo.RedirectStandardOutput = true;
                proc.StartInfo.RedirectStandardError = true;
                proc.StartInfo.RedirectStandardInput = true;
                proc.Start();

                proc.ErrorDataReceived += (sender, errorLine) => { if (errorLine != null) result += errorLine.Data + "\n"; };
                proc.OutputDataReceived += (sender, outputLine) => { if (outputLine != null) result += outputLine.Data + "\n"; };

                proc.BeginErrorReadLine();
                proc.BeginOutputReadLine();

                bool exited = proc.WaitForExit(timeoutInSeconds * 1000);

                if (!exited) {
                    proc.Kill();

                    throw new Exception("Command '" + command + "' took longer then the timeout of " + timeoutInSeconds + "s. Check the output for clues on what went wrong: " + result);
                }

                // Add a little sleep so that we can make sure even on success to catch all output
                Thread.Sleep(200);
            }

            return result.TrimEnd('\n');
        }

        public static string ExecWithDirectOutput(string command, bool isInteractive = false, bool disableJobControl = false)
        {
            string result = "";

            using (System.Diagnostics.Process proc = new())
            {
                proc.StartInfo.FileName = "/bin/bash";
                proc.StartInfo.Arguments = "-c" + (isInteractive ? "i" : "") + " \"" + (disableJobControl ? "set +m; " : "") + command.Replace("\"", "\\\"") + "\"";
                proc.StartInfo.EnvironmentVariables["WEBDEV_DISABLE_HEADER"] = "true";
                proc.StartInfo.UseShellExecute = false;
                proc.StartInfo.RedirectStandardOutput = true;
                proc.StartInfo.RedirectStandardError = true;
                proc.StartInfo.RedirectStandardInput = true;
                proc.OutputDataReceived += (sendingProcess, dataLine) => {
                    if (dataLine.Data != null) {
                        AnsiConsole.WriteLine(dataLine.Data);
                    }
                };

                proc.ErrorDataReceived += (sendingProcess, errorLine) => {
                    if (errorLine.Data != null) {
                        AnsiConsole.WriteLine(errorLine.Data);
                    }
                };

                proc.Start();

                proc.BeginOutputReadLine();
                proc.BeginErrorReadLine();

                proc.WaitForExit();
            }

            return result;
        }
    }
}
