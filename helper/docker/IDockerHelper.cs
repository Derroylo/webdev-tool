using System.Collections.Generic;

namespace WebDev.Tool.Helper.Docker;

public interface IDockerHelper
{
    List<string> GetRunningContainers(string name);
    void StopContainer(string name);
    string GetProjectName();
}
