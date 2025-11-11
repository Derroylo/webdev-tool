using System.Collections.Generic;
using System.ComponentModel;

namespace WebDev.Tool.Classes.Configuration;

internal class TestEntryConfiguration
{
    public string Name { get; set; } = "";

    public string Image { get; set; } = "";
    
    public List<string> Commands { get; set; } = new();

    public List<string> Tests { get; set; } = new();
}