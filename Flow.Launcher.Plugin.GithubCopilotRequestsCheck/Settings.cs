namespace Flow.Launcher.Plugin.GithubCopilotRequestsCheck;

public class Settings
{
    public string GitHubUsername { get; set; } = string.Empty;
    public string GitHubPAT { get; set; } = string.Empty;
    public int MonthlyQuota { get; set; } = 1500;
}
