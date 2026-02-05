namespace WebDev.Tool;

using System.ComponentModel;
using Spectre.Console.Cli;

public class LogCommandSettings : CommandSettings
{
    [CommandOption("--debug")]
    [Description("Output debug information")]
    [DefaultValue(false)]
    public bool Debug { get; set; }
}