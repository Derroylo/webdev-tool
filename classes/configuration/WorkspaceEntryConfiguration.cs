namespace WebDev.Tool.Classes.Configuration;

internal enum WorkspaceMode
{
    Vhost,
    DevContainer
}

internal class WorkspaceEntryConfiguration
{
    public string Name { get; set; } = "";
    
    public string Description { get; set; } = "";
    
    public string Repository { get; set; } = "";
    
    public string Branch { get; set; } = "";
    
    public string Folder { get; set; } = "";
    
    public string DocRoot { get; set; } = "public";
    
    public WorkspaceMode Mode { get; set; } = WorkspaceMode.Vhost;
    
    public string SubDomain { get; set; } = "";
    
    public bool DisableWeb { get; set; } = false;
}