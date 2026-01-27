namespace WebDev.Tool.Helper.Secrets;

public interface ISecretsLoader
{
    bool LoadFileSecrets(bool showMessages = true);
    bool LoadEnvVarSecrets(bool showMessages = true);
}