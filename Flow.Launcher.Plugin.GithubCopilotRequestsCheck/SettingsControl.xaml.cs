using System.Windows.Controls;

namespace Flow.Launcher.Plugin.GithubCopilotRequestsCheck;

public partial class SettingsControl : UserControl
{
    public Settings Settings { get; }

    public SettingsControl(Settings settings)
    {
        Settings = settings;
        InitializeComponent();
        PatPasswordBox.Password = Settings.GitHubPAT;
    }

    private void PatPasswordBox_PasswordChanged(object sender, System.Windows.RoutedEventArgs e)
    {
        Settings.GitHubPAT = PatPasswordBox.Password;
    }
}
