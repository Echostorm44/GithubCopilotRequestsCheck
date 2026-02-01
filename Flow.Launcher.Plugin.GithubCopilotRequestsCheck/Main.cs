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
    private PluginInitContext context;
    private Settings settings;

    public async Task InitAsync(PluginInitContext context)
    {
        this.context = context;
        settings = context.API.LoadSettingJsonStorage<Settings>();
    }

    public Control CreateSettingPanel()
    {
        return new SettingsControl(settings);
    }

    public async Task<List<Result>> QueryAsync(Query query, CancellationToken token)
    {
        var results = new List<Result>();
        token.ThrowIfCancellationRequested();

        if (string.IsNullOrWhiteSpace(settings.GitHubUsername))
        {
            results.Add(new Result
            {
                Title = "GitHub username not configured",
                SubTitle = "Please set your GitHub username in plugin settings",
                IcoPath = IconPath,
                Action = c =>
                {
                    context.API.OpenSettingDialog();
                    return true;
                }
            });
            return results;
        }

        var pat = !string.IsNullOrWhiteSpace(settings.GitHubPAT)
            ? settings.GitHubPAT
            : Environment.GetEnvironmentVariable("GITHUB_COPILOT_PAT", EnvironmentVariableTarget.User);

        if (string.IsNullOrEmpty(pat))
        {
            results.Add(new Result
            {
                Title = "GitHub PAT not configured",
                SubTitle = "Set PAT in settings or GITHUB_COPILOT_PAT env variable",
                IcoPath = IconPath,
                Action = c =>
                {
                    context.API.OpenSettingDialog();
                    return true;
                }
            });
            return results;
        }

        try
        {
            using HttpClient httpClient = new HttpClient();
            httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {pat}");
            httpClient.DefaultRequestHeaders.Add("Accept", "application/vnd.github+json");
            httpClient.DefaultRequestHeaders.Add("X-GitHub-Api-Version", "2022-11-28");
            httpClient.DefaultRequestHeaders.Add("User-Agent", "FlowLauncher-CopilotCheck");

            var now = DateTime.UtcNow;
            var baseUrl = $"https://api.github.com/users/{settings.GitHubUsername}/settings/billing/premium_request/usage";

            // 1. Get Monthly Total
            var monthlyResponse = await httpClient.GetAsync(baseUrl, token);
            var monthlyJson = await monthlyResponse.Content.ReadAsStringAsync(token);
            double totalUsed = ParseUsage(monthlyJson);

            // 2. Get Today's Specific Total
            var dailyUrl = $"{baseUrl}?year={now.Year}&month={now.Month}&day={now.Day}";
            var dailyResponse = await httpClient.GetAsync(dailyUrl, token);
            var dailyJson = await dailyResponse.Content.ReadAsStringAsync(token);
            double usedToday = ParseUsage(dailyJson);

            // Calculations
            int remaining = settings.MonthlyQuota - (int)totalUsed;
            int daysInMonth = DateTime.DaysInMonth(now.Year, now.Month);
            int daysRemaining = daysInMonth - now.Day + 1;

            // Avoid division by zero
            int perDay = daysRemaining > 0 ? remaining / daysRemaining : remaining;
            int highCostPerDay = perDay / 3;

            results.Add(new Result
            {
                Title = $"{remaining} requests left | {(int)usedToday} used today",
                SubTitle = $"{perDay}/day standard | {highCostPerDay}/day high-cost | {daysRemaining} days left",
                IcoPath = IconPath,
                Action = c =>
                {
                    System.Windows.Clipboard.SetText($"Copilot: {remaining} left ({perDay}/day)");
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

    private static double ParseUsage(string json)
    {
        using var doc = JsonDocument.Parse(json);
        double total = 0;
        if (doc.RootElement.TryGetProperty("usageItems", out var items))
        {
            foreach (var item in items.EnumerateArray())
            {
                if (item.TryGetProperty("grossQuantity", out var quantityProp))
                {
                    total += quantityProp.GetDouble();
                }
            }
        }
        return total;
    }
}