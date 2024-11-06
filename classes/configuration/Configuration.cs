namespace WebDev.Tool.Classes.Configuration
{
    internal class Configuration
    {
        public ConfigConfiguration Config { get; set; } = new();

        public PhpConfiguration Php { get; set; } = new();

        public NodeJsConfiguration Nodejs { get; set; } = new();

        public ServiceConfiguration Services { get; set; } = new();

        public ShellScriptsConfiguration ShellScripts { get; set; } = new();
    }
}