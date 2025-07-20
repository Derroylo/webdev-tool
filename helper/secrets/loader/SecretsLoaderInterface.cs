using WebDev.Tool.Classes.Configuration;

namespace WebDev.Tool.Helper.Secrets.Loader;

internal interface SecretsLoaderInterface
{
    public bool IsCorrectlyConfigured() => false;
    
    public bool HandleFileSecrets(string secretName, SecretConfiguration secret, bool showMessage) => false;
    
    public bool HandleEnvVarSecrets(string secretName, SecretConfiguration secret, bool showMessage) => false;
}