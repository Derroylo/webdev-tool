using Spectre.Console.Cli;
using WebDev.Tool.Helper.Secrets;

namespace WebDev.Tool.Commands.Secrets;

internal class LoadSecretsCommand: Command
{
    public override int Execute(CommandContext context)
    {
        SecretsLoader.LoadEnvVarSecrets();
        SecretsLoader.LoadFileSecrets();
        
        return 0;
    }
}