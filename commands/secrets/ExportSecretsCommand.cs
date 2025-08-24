using System;
using Spectre.Console.Cli;
using WebDev.Tool.Helper.Secrets;

namespace WebDev.Tool.Commands.Secrets;

internal class ExportSecretsCommand: Command
{
    public override int Execute(CommandContext context)
    {
        SecretsLoader.LoadEnvVarSecrets(false);
        
        foreach (var secret in SecretsLoader.EnvVarSecrets)
        {
            Console.WriteLine($"export {secret.Key}={secret.Value}");
        }
        
        return 0;
    }
}