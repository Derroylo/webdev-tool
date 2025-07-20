using Spectre.Console.Cli;
using WebDev.Tool.Helper.Secrets;

namespace WebDev.Tool.Commands.Secrets;

internal class LoadSecretsCommand: Command
{
    public override int Execute(CommandContext context)
    {
        SecretsLoader.LoadSecret(loadEnvVarSecrets: false);
        
        return 0;
    }
}