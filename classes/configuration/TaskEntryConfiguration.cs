using System.Collections.Generic;

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
    
    public TaskMode Mode { get; set; } = TaskMode.All;
    
    public List<string> Init { get; set; } = new();
    
    public List<string> Prebuild { get; set; } = new();
    
    public List<string> Create { get; set; } = new();
    
    public List<string> Start { get; set; } = new();
}