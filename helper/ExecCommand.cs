using System;
using System.Threading;
using System.IO;
using Spectre.Console;
using WebDev.Tool.Helper.Secrets;

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

        public static string ExecWithDirectOutput(string command, bool isInteractive = false, bool disableJobControl = false, string workingDirectory = "")
        {
            string result = "";

            using (System.Diagnostics.Process proc = new())
            {
                SecretsLoader.LoadEnvVarSecrets(false);

                foreach (var secret in SecretsLoader.EnvVarSecrets)
                {
                    proc.StartInfo.EnvironmentVariables[secret.Key] = secret.Value;
                }

                proc.StartInfo.FileName = "/bin/bash";
                proc.StartInfo.Arguments = $"-c" + (isInteractive ? "i" : "") + " \"" + (disableJobControl ? "set +m; " : "") + command.Replace("\"", "\\\"") + " 2>&1\"";
                proc.StartInfo.EnvironmentVariables["WEBDEV_DISABLE_HEADER"] = "true";
                proc.StartInfo.UseShellExecute = false;
                proc.StartInfo.RedirectStandardOutput = true;
                proc.StartInfo.RedirectStandardError = false;
                proc.StartInfo.RedirectStandardInput = true;

                if (workingDirectory != "") {
                    proc.StartInfo.WorkingDirectory = workingDirectory;
                }

                proc.Start();

                // Read output character-by-character to handle progress bars with \r
                ReadStreamWithProgressBar(proc.StandardOutput);

                proc.WaitForExit();
            }

            return result;
        }

        private static void ReadStreamWithProgressBar(StreamReader reader)
        {
            char[] buffer = new char[1];
            
            while (reader.Read(buffer, 0, 1) > 0)
            {
                char c = buffer[0];
                
                if (c == '\r')
                {
                    // Carriage return - write \r to position cursor at start of line
                    // This allows the next content to overwrite the current line
                    AnsiConsole.Write("\r");
                }
                else if (c == '\n')
                {
                    // Newline - just write the newline (characters were already written)
                    AnsiConsole.Write("\n");
                }
                else
                {
                    // Regular character - write immediately for real-time display
                    AnsiConsole.Write(c);
                }
            }
        }
    }
}
