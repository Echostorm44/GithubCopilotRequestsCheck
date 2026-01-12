using System.Windows;
using System.Windows.Controls;

namespace Flow.Launcher.Plugin.GithubCopilotRequestsCheck;

public partial class SettingsControl : UserControl
{
    private static readonly int[] PredefinedQuotas = { 50, 300, 1000, 1500 };
    public Settings Settings { get; }

    public SettingsControl(Settings settings)
    {
        Settings = settings;
        InitializeComponent();
        PatPasswordBox.Password = Settings.GitHubPAT;
        InitializeQuotaSelection();
    }

    private void InitializeQuotaSelection()
    {
        int index = System.Array.IndexOf(PredefinedQuotas, Settings.MonthlyQuota);
        if (index >= 0)
        {
            QuotaComboBox.SelectedIndex = index;
            CustomQuotaTextBox.Visibility = Visibility.Collapsed;
        }
        else
        {
            QuotaComboBox.SelectedIndex = 4; // Custom
            CustomQuotaTextBox.Visibility = Visibility.Visible;
        }
    }

    private void QuotaComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (QuotaComboBox.SelectedItem is ComboBoxItem item && item.Tag is string tagStr)
        {
            int tag = int.Parse(tagStr);
            if (tag == 0)
            {
                CustomQuotaTextBox.Visibility = Visibility.Visible;
            }
            else
            {
                CustomQuotaTextBox.Visibility = Visibility.Collapsed;
                Settings.MonthlyQuota = tag;
            }
        }
    }

    private void PatPasswordBox_PasswordChanged(object sender, RoutedEventArgs e)
    {
        Settings.GitHubPAT = PatPasswordBox.Password;
    }
}
