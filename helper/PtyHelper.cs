using System;
using System.IO;
using System.Text;
using System.Diagnostics;

namespace WebDev.Tool.Helper
{
    public class PtyHelper : IPtyHelper
    {
        /// <summary>
        /// Executes a command in a pseudo-terminal using the 'script' command, preserving all terminal formatting including colors and progress bars.
        /// </summary>
        public void ExecWithPty(string command, bool isInteractive = false, bool disableJobControl = false, string workingDirectory = "", System.Collections.Generic.Dictionary<string, string> additionalEnvVars = null)
        {
            using (Process proc = new Process())
            {
                // Build the command to execute through bash
                string bashArgs = (isInteractive ? "-ci" : "-c");
                string fullCommand = (disableJobControl ? "set +m; " : "") + command;
                
                // Escape the command for use in script arguments
                string escapedCommand = fullCommand.Replace("\\", "\\\\").Replace("\"", "\\\"").Replace("$", "\\$");
                
                // Use 'script' command to create a PTY automatically
                // -q = quiet mode (don't print script messages)
                // -e = return exit code of child process
                // -c = execute command
                // We pass the command to bash through script
                // /dev/null = don't log to file, just use PTY for terminal emulation
                proc.StartInfo.FileName = "/usr/bin/script";
                proc.StartInfo.Arguments = $"-qec \"/bin/bash {bashArgs} \\\"{escapedCommand}\\\"\" /dev/null";
                
                // Set environment variables
                proc.StartInfo.EnvironmentVariables["WEBDEV_DISABLE_HEADER"] = "true";
                proc.StartInfo.EnvironmentVariables["TERM"] = "xterm-256color";
                
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

                // Configure process to not redirect streams - let output go directly to console
                // This preserves all formatting because script creates a PTY
                proc.StartInfo.UseShellExecute = false;
                proc.StartInfo.RedirectStandardOutput = false;
                proc.StartInfo.RedirectStandardError = false;
                proc.StartInfo.RedirectStandardInput = false;

                proc.Start();
                proc.WaitForExit();
            }
        }
    }
}

