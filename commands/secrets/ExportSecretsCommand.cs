using System;
using Spectre.Console.Cli;
using WebDev.Tool.Helper.Secrets;
using WebDev.Tool.Helper.Internal;

namespace WebDev.Tool.Commands.Secrets;

internal class ExportSecretsCommand(IDebugOutputHelper _debugOutputHelper, ISecretsLoader _secretsLoader) : Command<ExportSecretsCommand.Settings>
{
    public class Settings : LogCommandSettings
    {
    }

    public override int Execute(CommandContext context, Settings settings)
    {
        _debugOutputHelper.WriteInfoOutput("Executing ExportSecretsCommand", this);
        
        _secretsLoader.LoadEnvVarSecrets(false);
        
        foreach (var secret in SecretsLoader.EnvVarSecrets)
        {
            Console.WriteLine($"export {secret.Key}={secret.Value}");
        }
        
        return 0;
    }
}