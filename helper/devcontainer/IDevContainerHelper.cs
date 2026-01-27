using System.Text.Json;

namespace WebDev.Tool.Helper.DevContainer;

public interface IDevContainerHelper
{
    JsonDocument ReadDevContainerConfig(string workspacePath = null);
    string GetDevContainerId();
    bool IsDevContainerCliInstalled();
    bool IsNpmInstalled();
    bool InstallDevContainerCli(bool useSudo = false);
    void ApplyTemplate(string workspacePath, string templateId, string templateArgs = null, string features = null);
    void UpdateNameAndDescription(string workspacePath, string name);
}
