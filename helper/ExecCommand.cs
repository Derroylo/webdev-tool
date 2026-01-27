using System;
using System.Collections.Generic;
using System.Threading;
using System.IO;
using Spectre.Console;
using WebDev.Tool.Helper.Secrets;

namespace WebDev.Tool.Helper
{
    public class ExecCommand(ISecretsLoader _secretsLoader, IPtyHelper _ptyHelper)
    {
        public string Exec(string command, bool isInteractive = false, bool disableJobControl = false, int timeoutInSeconds = 300)
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

        public void ExecWithDirectOutput(string command, bool isInteractive = false, bool disableJobControl = false, string workingDirectory = "", bool useStreaming = false)
        {
            // Load secrets for environment variables
            _secretsLoader.LoadEnvVarSecrets(false);

            // Prepare environment variables dictionary
            Dictionary<string, string> envVars = new Dictionary<string, string>();
            foreach (var secret in SecretsLoader.EnvVarSecrets)
            {
                envVars[secret.Key] = secret.Value;
            }

            if (useStreaming)
            {
                // Use stream-based method for better compatibility with Docker and other commands
                // that don't work well with PTY
                ExecWithStreamingOutput(command, isInteractive, disableJobControl, workingDirectory, envVars);
            }
            else
            {
                // Execute command using PTY to preserve all terminal formatting (default)
                _ptyHelper.ExecWithPty(command, isInteractive, disableJobControl, workingDirectory, envVars);
            }
        }

        private static void ExecWithStreamingOutput(string command, bool isInteractive = false, bool disableJobControl = false, string workingDirectory = "", Dictionary<string, string> additionalEnvVars = null)
        {
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

                // Set environment variables
                if (additionalEnvVars != null)
                {
                    foreach (var envVar in additionalEnvVars)
                    {
                        proc.StartInfo.EnvironmentVariables[envVar.Key] = envVar.Value;
                    }
                }

                // Set working directory if specified
                if (!string.IsNullOrEmpty(workingDirectory))
                {
                    proc.StartInfo.WorkingDirectory = workingDirectory;
                }

                proc.Start();

                // Stream output in real-time
                // Write directly to console to preserve formatting and support progress bars (carriage returns)
                proc.ErrorDataReceived += (sender, errorLine) => 
                { 
                    if (errorLine?.Data != null) 
                    {
                        // Write to stderr to preserve error stream semantics
                        System.Console.Error.WriteLine(errorLine.Data);
                    }
                };
                proc.OutputDataReceived += (sender, outputLine) => 
                { 
                    if (outputLine?.Data != null) 
                    {
                        // Write directly to stdout to preserve all formatting including progress bars
                        // This supports carriage returns (\r) used by progress bars
                        System.Console.Out.WriteLine(outputLine.Data);
                    }
                };

                proc.BeginErrorReadLine();
                proc.BeginOutputReadLine();

                proc.WaitForExit();

                // Add a little sleep to ensure all output is captured
                Thread.Sleep(200);
            }
        }
    }
}
