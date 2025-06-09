using Spectre.Console;

namespace WebDev.Tool.Helper.apache;

internal class ApacheHelper
{
    public static bool DisableVhost(string siteName, bool debug = false)
    {
        var enableSiteCommand = $"a2dissite {siteName}";
        var process = new System.Diagnostics.Process
        {
            StartInfo = new System.Diagnostics.ProcessStartInfo
            {
                FileName = "bash",
                Arguments = $"-c \"{enableSiteCommand}\"",
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true
            }
        };
            
        process.Start();
        process.WaitForExit();

        if (process.ExitCode != 0)
        {
            AnsiConsole.MarkupLine($"[red]Failed to disable site: {siteName}[/]");
            AnsiConsole.MarkupLine($"[red]{process.StandardError.ReadToEnd()}[/]");

            return false;
            
        }
        
        if (debug)
        {
            AnsiConsole.MarkupLine($"[green]Disabled site: {siteName}[/]");
        }

        return true;
    }
    
    public static bool EnableVhost(string siteName, bool debug = false)
    {
        var enableSiteCommand = $"a2ensite {siteName}";
        var process = new System.Diagnostics.Process
        {
            StartInfo = new System.Diagnostics.ProcessStartInfo
            {
                FileName = "bash",
                Arguments = $"-c \"{enableSiteCommand}\"",
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true
            }
        };
            
        process.Start();
        process.WaitForExit();

        if (process.ExitCode != 0)
        {
            AnsiConsole.MarkupLine($"[red]Failed to enable site: {siteName}[/]");
            AnsiConsole.MarkupLine($"[red]{process.StandardError.ReadToEnd()}[/]");

            return false;
            
        }
        
        if (debug)
        {
            AnsiConsole.MarkupLine($"[green]Enabled site: {siteName}[/]");
        }

        return true;
    }

    public static bool ReloadApache(bool debug = false)
    {
        var reloadCommand = "apachectl restart";
        var reloadProcess = new System.Diagnostics.Process
        {
            StartInfo = new System.Diagnostics.ProcessStartInfo
            {
                FileName = "bash",
                Arguments = $"-c \"{reloadCommand}\"",
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true
            }
        };
        
        reloadProcess.Start();
        reloadProcess.WaitForExit();
        
        if (reloadProcess.ExitCode != 0)
        {
            AnsiConsole.MarkupLine("[red]Failed to restart Apache.[/]");
            AnsiConsole.MarkupLine($"[red]{reloadProcess.StandardError.ReadToEnd()}[/]");

            return false;
        }
        
        if (debug)
        {
            AnsiConsole.MarkupLine("[green]Apache reloaded successfully.[/]");
        }

        return true;
    }
}