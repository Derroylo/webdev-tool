using System;
using System.IO;
using Spectre.Console;
using WebDev.Tool.Helper.FrankenPHP;

namespace WebDev.Tool.Helper.Admin;

public class AdminHelper(IFrankenPHPHelper _frankenPHPHelper, IAdminUpdateHelper _adminUpdateHelper) : IAdminHelper
{
    public bool StartAdmin(int port = 8000)
    {
        if (port <= 0 || port > 65535)
        {
            AnsiConsole.MarkupLine("[red]Invalid port.[/]");
            return false;
        }

        var frankenPHPPath = _frankenPHPHelper.GetOrDownloadFrankenPHPAsync().GetAwaiter().GetResult();
        if (string.IsNullOrEmpty(frankenPHPPath)) {
            return false;
        }

        if (!_adminUpdateHelper.InstallOrUpdateIfNeededAsync().GetAwaiter().GetResult()) {
            return false;
        }

        var baseDir = AppDomain.CurrentDomain.BaseDirectory;
        var adminDir = Path.Combine(baseDir, "admin");
        var publicDir = Path.Combine(adminDir, "public");
        if (!Directory.Exists(publicDir))
        {
            AnsiConsole.MarkupLine("[red]Admin backend public directory not found.[/]");
            return false;
        }

        var pidFile = Path.Combine(baseDir, ".admin_pid");
        if (File.Exists(pidFile))
        {
            AnsiConsole.MarkupLine("[yellow]Server is already running.[/]");
            AnsiConsole.MarkupLine($"[yellow]Access the admin interface at http://localhost:{port}[/]");
            return true;
        }

        _frankenPHPHelper.StartFrankenPHP(publicDir, port, pidFile);

        if (!File.Exists(pidFile)) {
            return false;
        }

        AnsiConsole.MarkupLine("[green]Server started successfully.[/]");
        AnsiConsole.MarkupLine($"[green]Access the admin interface at http://localhost:{port}[/]");
        return true;
    }

    public bool StopAdmin(bool debug = false)
    {
        var baseDir = AppDomain.CurrentDomain.BaseDirectory;
        var pidFile = Path.Combine(baseDir, ".admin_pid");

        var stopped = _frankenPHPHelper.StopFrankenPHP(pidFile);
        if (stopped) {
            AnsiConsole.MarkupLine("[green]Server stopped successfully.[/]");
        }

        return stopped;
    }
}