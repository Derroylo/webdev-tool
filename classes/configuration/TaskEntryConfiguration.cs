using System.Collections.Generic;
using System.ComponentModel;

namespace WebDev.Tool.Classes.Configuration;

internal enum TaskMode
{
    All,
    Local,
    DevContainer
}

internal class TaskEntryConfiguration
{
    public string Name { get; set; } = "";
    
    [DefaultValue(TaskMode.All)]
    public TaskMode Mode { get; set; } = TaskMode.All;
    
    [DefaultValue(true)]
    public bool OnlyMain { get; set; } = true;
    
    public List<string> Init { get; set; } = new();
    
    public List<string> Prebuild { get; set; } = new();
    
    public List<string> Create { get; set; } = new();
    
    public List<string> Start { get; set; } = new();
}