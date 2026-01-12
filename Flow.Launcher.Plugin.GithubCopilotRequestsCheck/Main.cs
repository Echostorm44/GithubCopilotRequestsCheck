using Flow.Launcher.Plugin;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace Flow.Launcher.Plugin.GithubCopilotRequestsCheck;

public class GithubCopilotRequestsCheck : IAsyncPlugin, ISettingProvider
{
    private const string IconPath = "icon.png";
    private PluginInitContext _context;
    private Settings _settings;

    public async Task InitAsync(PluginInitContext context)
    {
        _context = context;
        _settings = context.API.LoadSettingJsonStorage<Settings>();
    }

    public Control CreateSettingPanel()
    {
        return new SettingsControl(_settings);
    }

    public async Task<List<Result>> QueryAsync(Query query, CancellationToken token)
    {
        var results = new List<Result>();
        token.ThrowIfCancellationRequested();

        // Validate settings
        if (string.IsNullOrWhiteSpace(_settings.GitHubUsername))
        {
            results.Add(new Result
            {
                Title = "GitHub username not configured",
                SubTitle = "Please set your GitHub username in plugin settings",
                IcoPath = IconPath,
                Action = c =>
                {
                    _context.API.OpenSettingDialog();
                    return true;
                }
            });
            return results;
        }

        // Get PAT from settings first, then fall back to ENV
        var pat = !string.IsNullOrWhiteSpace(_settings.GitHubPAT) 
            ? _settings.GitHubPAT 
            : Environment.GetEnvironmentVariable("GITHUB_COPILOT_PAT", EnvironmentVariableTarget.User);

        if (string.IsNullOrEmpty(pat))
        {
            results.Add(new Result
            {
                Title = "GitHub PAT not configured",
                SubTitle = "Set PAT in plugin settings or GITHUB_COPILOT_PAT env variable",
                IcoPath = IconPath,
                Action = c =>
                {
                    _context.API.OpenSettingDialog();
                    return true;
                }
            });
            return results;
        }

        try
        {
            var request = new HttpRequestMessage(HttpMethod.Get,
                $"https://api.github.com/users/{_settings.GitHubUsername}/settings/billing/premium_request/usage");
            request.Headers.Add("Authorization", $"Bearer {pat}");
            request.Headers.Add("Accept", "application/vnd.github+json");
            request.Headers.Add("X-GitHub-Api-Version", "2022-11-28");
            request.Headers.Add("User-Agent", "FlowLauncher-CopilotCheck");
            using HttpClient _httpClient = new HttpClient();
            var response = await _httpClient.SendAsync(request, token);
            var json = await response.Content.ReadAsStringAsync(token);

            if (!response.IsSuccessStatusCode)
            {
                results.Add(new Result
                {
                    Title = $"API Error: {response.StatusCode}",
                    SubTitle = json.Length > 100 ? $"{(json.Substring(0, 100))}..." : json,
                    IcoPath = IconPath
                });
                return results;
            }

            var doc = JsonDocument.Parse(json);
            double totalUsed = 0;
            foreach (var item in doc.RootElement.GetProperty("usageItems").EnumerateArray())
            {
                totalUsed += item.GetProperty("grossQuantity").GetDouble();
            }

            int remaining = _settings.MonthlyQuota - (int)totalUsed;
            int daysInMonth = DateTime.DaysInMonth(DateTime.UtcNow.Year, DateTime.UtcNow.Month);
            int daysRemaining = daysInMonth - DateTime.UtcNow.Day + 1;
            int perDay = remaining / daysRemaining;
            int highCostPerDay = perDay / 3;

            results.Add(new Result
            {
                Title = $"{remaining} requests left this month",
                SubTitle = $"{perDay}/day standard | {highCostPerDay}/day for 3x models | {daysRemaining} days left",
                IcoPath = IconPath,
                Action = c =>
                {
                    System.Windows.Clipboard.SetText($"Copilot: {remaining} left ({perDay}/day, {highCostPerDay} high-cost/day)");
                    return true;
                }
            });
        }
        catch (Exception ex)
        {
            results.Add(new Result
            {
                Title = "Error fetching Copilot usage",
                SubTitle = ex.Message,
                IcoPath = IconPath
            });
        }

        return results;
    }
}