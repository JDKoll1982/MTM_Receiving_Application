using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MTM_Receiving_Application.Module_Core.Contracts.Services;
using MTM_Receiving_Application.Module_Core.Models.Core;
using MTM_Receiving_Application.Module_Core.Models.InforVisual;
using MTM_Receiving_Application.Module_ShipRec_Tools.Contracts;
using MTM_Receiving_Application.Module_ShipRec_Tools.Models;

namespace MTM_Receiving_Application.Module_ShipRec_Tools.Services;

/// <summary>
/// Business logic for weighted fuzzy ranking of PO line binary/spec search results.
/// </summary>
public class Service_Tool_POLineSpecSearch : IService_Tool_POLineSpecSearch
{
    private readonly IService_InforVisual _inforVisual;
    private readonly IService_LoggingUtility _logger;

    public Service_Tool_POLineSpecSearch(
        IService_InforVisual inforVisual,
        IService_LoggingUtility logger
    )
    {
        ArgumentNullException.ThrowIfNull(inforVisual);
        ArgumentNullException.ThrowIfNull(logger);
        _inforVisual = inforVisual;
        _logger = logger;
    }

    public async Task<Model_Dao_Result<List<Model_Tool_POLineSpecSearchResult>>> SearchAsync(
        string searchTerm,
        Model_Tool_POLineSpecSearchOptions options,
        int maxResults = 200
    )
    {
        if (string.IsNullOrWhiteSpace(searchTerm))
        {
            return Model_Dao_Result_Factory.Failure<List<Model_Tool_POLineSpecSearchResult>>(
                "Search term cannot be empty"
            );
        }

        var normalizedTerm = NormalizeSearch(searchTerm);
        var tokens = Tokenize(normalizedTerm);
        if (tokens.Count == 0)
        {
            return Model_Dao_Result_Factory.Failure<List<Model_Tool_POLineSpecSearchResult>>(
                "Search term cannot be empty"
            );
        }

        options ??= new Model_Tool_POLineSpecSearchOptions();

        var safeMaxResults = Math.Clamp(maxResults, 1, 500);
        var normalizedMode = NormalizeSearchMode(options.SearchMode);
        var statusCodeFilter = MapPoStatusFilterToCode(options.PoStatusFilter);
        _logger.LogInfo(
            $"Searching PO line specs for '{normalizedTerm}' with max results {safeMaxResults}, mode '{normalizedMode}', status '{statusCodeFilter}'"
        );

        var candidateResult = await _inforVisual.SearchPurchaseOrderLineSpecsAsync(
            normalizedTerm,
            Math.Max(safeMaxResults * 3, 150),
            normalizedMode,
            statusCodeFilter
        );

        if (!candidateResult.IsSuccess || candidateResult.Data is null)
        {
            return Model_Dao_Result_Factory.Failure<List<Model_Tool_POLineSpecSearchResult>>(
                candidateResult.ErrorMessage,
                candidateResult.Exception
            );
        }

        var ranked = candidateResult
            .Data.Select(row => BuildRankedResult(row, normalizedTerm, tokens))
            .Where(result => result.Result is not null && result.Score > 0)
            .OrderByDescending(result => result.Score)
            .ThenByDescending(result => result.Result!.PONumber, StringComparer.OrdinalIgnoreCase)
            .ThenBy(result => result.Result!.POLineNumber)
            .Take(safeMaxResults)
            .Select(result =>
            {
                result.Result!.MatchScore = result.Score;
                return result.Result;
            })
            .ToList();

        return Model_Dao_Result_Factory.Success(ranked!);
    }

    private static (Model_Tool_POLineSpecSearchResult? Result, int Score) BuildRankedResult(
        Model_InforVisualPOLineSpecSearchRow row,
        string normalizedTerm,
        IReadOnlyList<string> tokens
    )
    {
        var normalizedSpec = NormalizeSearch(row.SpecText);
        var normalizedSupplemental = NormalizeSearch(row.SupplementalText);
        var combined = $"{normalizedSpec} {normalizedSupplemental}".Trim();
        if (combined.Length == 0)
        {
            return (null, 0);
        }

        var score = 0;

        if (normalizedSpec.Contains(normalizedTerm, StringComparison.Ordinal))
        {
            score += 120;
        }

        if (ContainsOrderedTokens(normalizedSpec, tokens))
        {
            score += 80;
        }

        foreach (var token in tokens)
        {
            if (normalizedSpec.Contains(token, StringComparison.Ordinal))
            {
                score += 20;
                continue;
            }

            if (normalizedSupplemental.Contains(token, StringComparison.Ordinal))
            {
                score += 12;
                continue;
            }

            if (row.PartId.Contains(token, StringComparison.OrdinalIgnoreCase))
            {
                score += 8;
                continue;
            }

            if (row.VendorId.Contains(token, StringComparison.OrdinalIgnoreCase))
            {
                score += 6;
            }
        }

        if (string.Equals(row.BinaryType, "D", StringComparison.OrdinalIgnoreCase))
        {
            score += 4;
        }

        if (score <= 0)
        {
            return (null, 0);
        }

        return (
            new Model_Tool_POLineSpecSearchResult
            {
                PONumber = row.PONumber,
                POLineNumber = row.POLineNumber,
                PartId = row.PartId,
                VendorId = row.VendorId,
                VendorName = row.VendorName,
                VendorPartId = row.VendorPartId,
                QtyOrdered = row.QtyOrdered,
                TotalQtyReceived = row.TotalQtyReceived,
                PoStatus = row.PoStatus,
                BinaryType = row.BinaryType,
                SpecText = row.SpecText,
                SpecExcerpt = BuildExcerpt(row.SpecText, tokens),
                MatchScore = score,
            },
            score
        );
    }

    private static bool ContainsOrderedTokens(string text, IReadOnlyList<string> tokens)
    {
        var cursor = 0;
        foreach (var token in tokens)
        {
            var index = text.IndexOf(token, cursor, StringComparison.Ordinal);
            if (index < 0)
            {
                return false;
            }

            cursor = index + token.Length;
        }

        return true;
    }

    private static string NormalizeSearchMode(string searchMode)
    {
        if (
            Model_Tool_POLineSpecSearchOptions.SearchModeOptions.Contains(
                searchMode,
                StringComparer.OrdinalIgnoreCase
            )
        )
        {
            return Model_Tool_POLineSpecSearchOptions.SearchModeOptions.First(mode =>
                string.Equals(mode, searchMode, StringComparison.OrdinalIgnoreCase)
            );
        }

        return Model_Tool_POLineSpecSearchOptions.DefaultSearchMode;
    }

    private static string MapPoStatusFilterToCode(string poStatusFilter)
    {
        if (string.IsNullOrWhiteSpace(poStatusFilter))
        {
            return string.Empty;
        }

        return poStatusFilter.Trim().ToUpperInvariant() switch
        {
            "FIRMED" or "F" => "F",
            "RELEASED" or "R" => "R",
            "CLOSED" or "C" => "C",
            "CANCELLED/VOID" or "CANCELLED" or "CANCELED" or "VOID" or "V" => "V",
            _ => string.Empty,
        };
    }

    private static string BuildExcerpt(string specText, IReadOnlyList<string> tokens)
    {
        var normalizedDisplay = NormalizeWhitespace(specText);
        if (string.IsNullOrWhiteSpace(normalizedDisplay))
        {
            return string.Empty;
        }

        var words = normalizedDisplay.Split(' ', StringSplitOptions.RemoveEmptyEntries);
        if (words.Length == 0)
        {
            return string.Empty;
        }

        var phrase = tokens.Count > 0 ? string.Join(' ', tokens) : string.Empty;
        var matchIndex = string.IsNullOrWhiteSpace(phrase)
            ? -1
            : normalizedDisplay.IndexOf(phrase, StringComparison.OrdinalIgnoreCase);
        var matchLength = matchIndex >= 0 ? phrase.Length : 0;

        if (matchIndex < 0 && tokens.Count > 0)
        {
            matchIndex = normalizedDisplay.IndexOf(tokens[0], StringComparison.OrdinalIgnoreCase);
            if (matchIndex >= 0)
            {
                matchLength = tokens[0].Length;
            }
        }

        if (matchIndex < 0)
        {
            return normalizedDisplay;
        }

        var matchEndExclusive = matchIndex + Math.Max(matchLength, 1);
        var wordStartIndex = 0;
        var wordEndIndex = words.Length - 1;
        var cursor = 0;

        for (var i = 0; i < words.Length; i++)
        {
            var start = cursor;
            var end = start + words[i].Length;

            if (matchIndex >= start && matchIndex < end)
            {
                wordStartIndex = i;
            }

            if (matchEndExclusive > start && matchEndExclusive <= end)
            {
                wordEndIndex = i;
                break;
            }

            cursor = end + 1;
        }

        var excerptStartWord = Math.Max(0, wordStartIndex - 1);
        var excerptEndWord = Math.Min(words.Length - 1, wordEndIndex + 1);
        var excerpt = string.Join(
            ' ',
            words.Skip(excerptStartWord).Take(excerptEndWord - excerptStartWord + 1)
        );

        if (excerptStartWord > 0)
        {
            excerpt = "... " + excerpt;
        }

        if (excerptEndWord < words.Length - 1)
        {
            excerpt += " ...";
        }

        return excerpt;
    }

    private static string NormalizeSearch(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return string.Empty;
        }

        var upper = value.ToUpperInvariant();
        return NormalizeWhitespace(upper);
    }

    private static string NormalizeWhitespace(string value)
    {
        var builder = new StringBuilder(value.Length);
        var previousWasWhitespace = false;

        foreach (var ch in value)
        {
            if (char.IsWhiteSpace(ch))
            {
                if (!previousWasWhitespace)
                {
                    builder.Append(' ');
                }

                previousWasWhitespace = true;
                continue;
            }

            builder.Append(ch);
            previousWasWhitespace = false;
        }

        return builder.ToString().Trim();
    }

    private static List<string> Tokenize(string normalizedTerm)
    {
        return normalizedTerm
            .Split(' ', StringSplitOptions.RemoveEmptyEntries)
            .Distinct(StringComparer.Ordinal)
            .ToList();
    }
}
