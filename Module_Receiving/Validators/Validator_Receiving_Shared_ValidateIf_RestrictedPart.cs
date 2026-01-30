using System;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using MTM_Receiving_Application.Module_Receiving.Data;

namespace MTM_Receiving_Application.Module_Receiving.Validators;

/// <summary>
/// Validator to check if a part number matches configurable restricted part patterns
/// User Requirement: Configurable patterns (NOT hardcoded MMFSR/MMCSR)
/// Patterns stored in database settings and loaded at runtime
/// </summary>
public class Validator_Receiving_Shared_ValidateIf_RestrictedPart
{
    private readonly Dao_Receiving_Repository_Settings _settingsDao;
    private string[] _restrictedPartPatterns = Array.Empty<string>();
    private DateTime _lastLoadedAt = DateTime.MinValue;
    private readonly TimeSpan _cacheExpiration = TimeSpan.FromMinutes(5);

    public Validator_Receiving_Shared_ValidateIf_RestrictedPart(Dao_Receiving_Repository_Settings settingsDao)
    {
        _settingsDao = settingsDao;
    }

    /// <summary>
    /// Checks if a part number matches any configured restricted part patterns
    /// </summary>
    /// <param name="partNumber">Part number to validate</param>
    /// <returns>Tuple: (IsRestricted, MatchedPattern, RestrictionType)</returns>
    public async Task<(bool IsRestricted, string? MatchedPattern, string? RestrictionType)> ValidateAsync(string partNumber)
    {
        if (string.IsNullOrWhiteSpace(partNumber))
        {
            return (false, null, null);
        }

        // Reload patterns if cache expired
        if (DateTime.UtcNow - _lastLoadedAt > _cacheExpiration)
        {
            await LoadRestrictedPartPatternsAsync();
        }

        // Check each pattern
        foreach (var pattern in _restrictedPartPatterns)
        {
            if (IsMatchingPattern(partNumber, pattern))
            {
                // Extract restriction type from pattern (format: "PATTERN|RestrictionType")
                var parts = pattern.Split('|');
                var matchedPattern = parts[0];
                var restrictionType = parts.Length > 1 ? parts[1] : "Quality Hold Required";

                return (true, matchedPattern, restrictionType);
            }
        }

        return (false, null, null);
    }

    /// <summary>
    /// Loads restricted part patterns from database settings
    /// Setting Key: "QualityHold_RestrictedPartPatterns"
    /// Setting Value: Comma-separated patterns (e.g., "MMFSR*|Weight Sensitive,MMCSR*|Quality Control,CUSTOM*|Special Handling")
    /// </summary>
    private async Task LoadRestrictedPartPatternsAsync()
    {
        try
        {
            var result = await _settingsDao.SelectByKeyAsync("QualityHold_RestrictedPartPatterns", "System");

            if (result.Success && result.Data != null && !string.IsNullOrWhiteSpace(result.Data.SettingValue))
            {
                _restrictedPartPatterns = result.Data.SettingValue
                    .Split(',', StringSplitOptions.RemoveEmptyEntries)
                    .Select(p => p.Trim())
                    .ToArray();

                _lastLoadedAt = DateTime.UtcNow;
            }
            else
            {
                // Fallback to default patterns if setting not found
                _restrictedPartPatterns = new[]
                {
                    "MMFSR*|Weight Sensitive",
                    "MMCSR*|Quality Control"
                };
                _lastLoadedAt = DateTime.UtcNow;
            }
        }
        catch
        {
            // On error, use default patterns
            _restrictedPartPatterns = new[]
            {
                "MMFSR*|Weight Sensitive",
                "MMCSR*|Quality Control"
            };
            _lastLoadedAt = DateTime.UtcNow;
        }
    }

    /// <summary>
    /// Checks if a part number matches a wildcard pattern
    /// Supports * and ? wildcards
    /// </summary>
    private bool IsMatchingPattern(string partNumber, string pattern)
    {
        // Extract pattern (before |)
        var patternOnly = pattern.Split('|')[0];

        // Convert wildcard pattern to regex
        var regexPattern = "^" + Regex.Escape(patternOnly)
            .Replace("\\*", ".*")
            .Replace("\\?", ".") + "$";

        return Regex.IsMatch(partNumber, regexPattern, RegexOptions.IgnoreCase);
    }

    /// <summary>
    /// Clears the pattern cache (useful for testing or after settings update)
    /// </summary>
    public void ClearCache()
    {
        _restrictedPartPatterns = Array.Empty<string>();
        _lastLoadedAt = DateTime.MinValue;
    }
}
