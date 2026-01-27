namespace WebDev.Tool.Helper.git;

public interface IGitHelper
{
    bool CloneRepository(string repoUrl, string targetFolder);
    bool CheckoutBranch(string repoPath, string branchName);
}
