using WebDev.Tool.Classes.Configuration;

namespace WebDev.Tool.Helper.Secrets.Handler;

internal interface SecretsHandlerInterface
{
    public bool IsCorrectlyConfigured() => false;

    public bool SupportsFileLoad() => false;

    public bool SupportsFileWrite() => false;

    public string HandleLoadSecret(string secretName, SecretConfiguration secret, bool showMessage) => null;
    
    public bool HandleWriteSecret(string secretName, string secretContent, SecretConfiguration secret, bool showMessage) => false;
}