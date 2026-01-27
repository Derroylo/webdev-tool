using Spectre.Console;

namespace WebDev.Tool.Helper.Internal;

public class DebugOutputHelper : IDebugOutputHelper
{
    private bool _enableDebugOutput = false;

    public void EnableDebugOutput()
    {
        _enableDebugOutput = true;
    }

    public void DisableDebugOutput()
    {
        _enableDebugOutput = false;
    }

    public bool IsDebugOutputEnabled()
    {
        return _enableDebugOutput;
    }
    
    public void WriteInfoOutput(string message, object sourceClass)
    {
        if (!IsDebugOutputEnabled())
        {
            return;
        }

        AnsiConsole.Write(new Pill("Info", PillType.Info));
        AnsiConsole.Write("".PadRight(4));
        AnsiConsole.Write(new Text(sourceClass.GetType().FullName, new Style(foreground: Color.Green)));
        AnsiConsole.WriteLine();
        AnsiConsole.WriteLine("".PadRight(12) + message);
    }

    public void WriteWarningOutput(string message, object sourceClass)
    {
        if (!IsDebugOutputEnabled())
        {
            return;
        }

        AnsiConsole.Write(new Pill("Warning", PillType.Warning));
        AnsiConsole.Write("".PadRight(2));
        AnsiConsole.Write(new Text(sourceClass.GetType().FullName, new Style(foreground: Color.Green)));
        AnsiConsole.WriteLine();
        AnsiConsole.WriteLine("".PadRight(12) + message);
    }

    public void WriteErrorOutput(string message, object sourceClass)
    {
        if (!IsDebugOutputEnabled())
        {
            return;
        }

        AnsiConsole.Write(new Pill("Error", PillType.Error));
        AnsiConsole.Write("".PadRight(4));
        AnsiConsole.Write(new Text(sourceClass.GetType().FullName, new Style(foreground: Color.Green)));
        AnsiConsole.WriteLine();
        AnsiConsole.WriteLine("".PadRight(12) + message);
    }
}