namespace WebDev.Tool.Helper.Internal;

public interface ITranslationHelper
{
    void LoadTranslations();
    string GetString(string key, params object[] args);
    void SetLanguage(string languageCode);
    string CurrentLanguage { get; }
}
