using Spectre.Console.Cli;
using WebDev.Tool.Helper.Secrets;

namespace WebDev.Tool.Commands.Secrets;

internal class LoadSecretsCommand: Command
{
    public override int Execute(CommandContext context)
    {
        if (!SecretsLoader.LoadEnvVarSecrets())
        {
            return 1;
        }

        if (!SecretsLoader.LoadFileSecrets())
        {
            return 1;
        }
        
        return 0;
    }
}