using Spectre.Console.Cli;
using WebDev.Tool.Helper.Secrets;
using WebDev.Tool.Helper.Internal;

namespace WebDev.Tool.Commands.Secrets;

internal class LoadSecretsCommand(IDebugOutputHelper _debugOutputHelper, ISecretsLoader _secretsLoader) : Command<LoadSecretsCommand.Settings>
{
    public class Settings : LogCommandSettings
    {
    }

    public override int Execute(CommandContext context, Settings settings)
    {
        _debugOutputHelper.WriteInfoOutput("Executing LoadSecretsCommand", this);
        
        if (!_secretsLoader.LoadEnvVarSecrets())
        {
            return 1;
        }

        if (!_secretsLoader.LoadFileSecrets())
        {
            return 1;
        }
        
        return 0;
    }
}