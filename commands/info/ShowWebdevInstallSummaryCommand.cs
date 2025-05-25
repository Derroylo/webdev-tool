using Spectre.Console;
using Spectre.Console.Cli;

namespace WebDev.Tool.Commands.Info;

public class ShowWebdevInstallSummaryCommand: Command
{
    public override int Execute(CommandContext context)
    {
        AnsiConsole.MarkupLine($"[bold green]WebDev Tool successfully installed! :beating_heart:[/]");
        
        AnsiConsole.MarkupLine($"\n[bold yellow]Need help?[/]");
        AnsiConsole.MarkupLine($"Visit [green]https://derroylo.github.io/[/] to checkout the docs.");
        
        AnsiConsole.MarkupLine($"\n[bold yellow]Something unclear, found an error or have suggestions for improvements?[/]");
        AnsiConsole.MarkupLine($"Visit [green]https://github.com/Derroylo/webdev-tool[/] to create an issue.");
        
        AnsiConsole.MarkupLine($"\n[bold yellow]What´s next?[/]");
        AnsiConsole.MarkupLine($"Just run [green]webdev[/] to see all available commands.");
        AnsiConsole.MarkupLine($"If you have already a project where you added all webdev devcontainer, change to it´s directory and run [green]webdev project start[/].");
        AnsiConsole.MarkupLine($"If you want to add webdev to an existing project, then just run [green]webdev project init[/]. It will guide you through the process.");
        
        return 0;
    }
}
