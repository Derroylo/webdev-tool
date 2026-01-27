using System.Collections.Generic;
using System.ComponentModel;

namespace WebDev.Tool.Classes.Configuration;

public enum WorkspaceMode
{
    Vhost,
    DevContainer
}

public class WorkspaceEntryConfiguration
{
    public string Name { get; set; } = "";
    
    public string Description { get; set; } = "";
    
    public string Repository { get; set; } = "";
    
    public string Branch { get; set; } = "";
    
    public string Folder { get; set; } = "";
    
    [DefaultValue("public")]
    public string DocRoot { get; set; } = "public";
    
    [DefaultValue(WorkspaceMode.Vhost)]
    public WorkspaceMode Mode { get; set; } = WorkspaceMode.Vhost;
    
    public List<string> SubDomains { get; set; } = new ();
    
    [DefaultValue(false)]
    public bool DisableWeb { get; set; } = false;

    public string SymLinkSource { get; set; } = "";

    public string SymLinkTarget { get; set; } = "";
}