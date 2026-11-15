namespace MTM_Database_Deployment.Models;

/// <summary>
/// Outcome of an apply run, including anything that was skipped or reported.
/// </summary>
public sealed class Model_DeploymentResult
{
    public int AppliedCount { get; private set; }
    public int SkippedCount { get; private set; }
    public int FailedCount { get; private set; }
    public int BenignSkipCount { get; private set; }
    public TimeSpan Duration { get; set; }
    public IReadOnlyList<string> Messages => _messages;
    public IReadOnlyList<string> Failures => _failures;
    public IReadOnlyDictionary<string, int> BenignSkips => _benignSkips;

    private readonly List<string> _messages = new();
    private readonly List<string> _failures = new();
    private readonly Dictionary<string, int> _benignSkips = new(StringComparer.Ordinal);

    public bool IsSuccess => _failures.Count == 0;

    public void MarkApplied(string message)
    {
        AppliedCount++;
        _messages.Add(message);
    }

    public void MarkSkipped(string message)
    {
        SkippedCount++;
        _messages.Add(message);
    }

    public void MarkBenignSkip(string message)
    {
        BenignSkipCount++;
        _benignSkips[message] = _benignSkips.TryGetValue(message, out var count) ? count + 1 : 1;
    }

    public void MarkFailed(string message)
    {
        FailedCount++;
        _failures.Add(message);
    }

    public void AddMessage(string message) => _messages.Add(message);

    public string ToReportText()
    {
        var builder = new System.Text.StringBuilder();
        builder.AppendLine($"Applied: {AppliedCount} | Skipped: {SkippedCount} | Already applied: {BenignSkipCount} | Failed: {FailedCount}");
        builder.AppendLine($"Duration: {Duration:h\\:mm\\:ss}");
        builder.AppendLine(new string('-', 100));

        foreach (var message in _messages)
        {
            builder.AppendLine(message);
        }

        if (_benignSkips.Count > 0)
        {
            builder.AppendLine();
            builder.AppendLine("ALREADY APPLIED (benign duplicates, nothing changed)");
            foreach (var entry in _benignSkips)
            {
                builder.AppendLine($"  - {entry.Key} (x{entry.Value})");
            }
        }

        if (_failures.Count > 0)
        {
            builder.AppendLine();
            builder.AppendLine("FAILURES");
            foreach (var failure in _failures)
            {
                builder.AppendLine("  - " + failure);
            }
        }

        return builder.ToString();
    }
}
