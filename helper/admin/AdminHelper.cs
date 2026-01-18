using System;
using System.IO;
using Spectre.Console;

namespace WebDev.Tool.Helper.Admin;

internal class AdminHelper
{
    public static bool StartAdmin(int port = 8000, bool runAsDaemon = true)
    {
        if (!TestRequirements())
        {
            return false;
        }

        if (port <= 0 || port > 65535)
        {
            AnsiConsole.MarkupLine("[red]Invalid port.[/]");
            return false;
        }

        var webDevAdminDir = AppDomain.CurrentDomain.BaseDirectory + "admin";
      
        var result = ExecCommand.Exec($"symfony server:start --port={port} --directory={webDevAdminDir} {(runAsDaemon ? "--daemon" : "")}");

        if (result.Contains("Web server listening"))
        {
            AnsiConsole.MarkupLine("[green]Server started successfully.[/]");
            AnsiConsole.MarkupLine($"[green]Access the admin interface at http://localhost:{port}[/]");
        }
        else if (result.Contains("The local web server is already running"))
        {
            AnsiConsole.MarkupLine("[yellow]Server is already running.[/]");
            AnsiConsole.MarkupLine($"[yellow]Access the admin interface at http://localhost:{port}[/]");
        }
        else
        {
            AnsiConsole.MarkupLine("[red]Failed to start server.[/]");
            AnsiConsole.MarkupLine($"[red]Message:[/]");
            AnsiConsole.MarkupLine(result);

            return false;
        }

        return true;
    }

    public static bool StopAdmin(bool debug = false)
    {
        if (!TestRequirements())
        {
            return false;
        }

        var webDevAdminDir = AppDomain.CurrentDomain.BaseDirectory + "admin";

        var result = ExecCommand.Exec($"symfony server:stop --directory={webDevAdminDir}");

        if (result.Contains("[OK]"))
        {
            AnsiConsole.MarkupLine("[green]Server stopped successfully.[/]");
        }
        else
        {
            AnsiConsole.MarkupLine("[red]Failed to stop server.[/]");
            AnsiConsole.MarkupLine($"[red]{result}[/]");

            return false;
        }

        return true;
    }

    public static bool IsSymfonyCliInstalled(bool debug = false)
    {
        var result = ExecCommand.Exec("symfony version");

        return result.Contains("Symfony CLI");
    }

    private static bool TestRequirements(bool debug = false)
    {
        if (!IsSymfonyCliInstalled())
        {
            AnsiConsole.MarkupLine("[red]Symfony CLI is not installed.[/]");
            AnsiConsole.MarkupLine("[red]Please install it using the following command:[/]");
            AnsiConsole.MarkupLine("[green]wget https://get.symfony.com/cli/installer -O - | bash[/]");
            AnsiConsole.MarkupLine("or");
            AnsiConsole.MarkupLine("[green]curl -sS https://get.symfony.com/cli/installer | bash[/]");

            return false;
        }

        var webDevAdminDir = AppDomain.CurrentDomain.BaseDirectory + "admin";

        if (!Directory.Exists(webDevAdminDir))
        {
            AnsiConsole.MarkupLine("[red]WebDev Admin directory does not exist.[/]");
            AnsiConsole.MarkupLine("[red]Please run the following command to create it:[/]");
            AnsiConsole.MarkupLine($"[green]mkdir -p {webDevAdminDir}[/] and download the admin interface into it.");
            
            return false;
        }

        if (!File.Exists(webDevAdminDir + "/composer.json"))
        {
            AnsiConsole.MarkupLine("[red]WebDev Admin composer.json file does not exist.[/]");
            AnsiConsole.MarkupLine("[red]Please run the following command to create it:[/]");
            
            return false;
        }

        return true;
    }
}