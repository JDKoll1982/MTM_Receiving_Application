using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MTM_Receiving_Application.Module_Core.Contracts.Services;
using MTM_Receiving_Application.Module_Core.Models.Core;
using MTM_Receiving_Application.Module_Core.Models.Reporting;
using MTM_Receiving_Application.Module_Dunnage.Contracts;
using MTM_Receiving_Application.Module_Dunnage.Helpers;
using MTM_Receiving_Application.Module_Dunnage.Models;
using MTM_Receiving_Application.Module_ShipRec_Tools.Contracts;
using MTM_Receiving_Application.Module_ShipRec_Tools.Helpers;
using MTM_Receiving_Application.Module_ShipRec_Tools.Models;

namespace MTM_Receiving_Application.Module_ShipRec_Tools.Services;

/// <summary>
/// Loads dunnage master data and builds a paged 8.5x11 portrait "book" document from
/// the selected parts.
/// </summary>
public class Service_Tool_DunnageBook : IService_Tool_DunnageBook
{
    private const int CardsPerPage = 6;
    private const int TocEntriesPerPage = 32;

    private readonly IService_MySQL_Dunnage _dunnageService;
    private readonly IService_LoggingUtility _logger;

    private static readonly string FallbackImagePath = Path.Combine(
        AppContext.BaseDirectory,
        "Assets",
        "DunnageBookNoImage.png"
    );

    private static readonly string CoverLogoPath = Path.Combine(
        AppContext.BaseDirectory,
        "Assets",
        "MTMLogo.jpg"
    );

    private const string PlaceholderSvgDataUri =
        "data:image/svg+xml;base64,"
        + "PHN2ZyB4bWxucz0iaHR0cDovL3d3dy53My5vcmcvMjAwMC9zdmciIHdpZHRoPSI0MDAiIGhlaWdodD0iMzAwIj48cmVjdCB3aWR0aD0iMTAwJSIgaGVpZ2h0PSIxMDAlIiBmaWxsPSIjZTBlMGUwIi8+PHRleHQgeD0iNTAlIiB5PSI1MCUiIGZvbnQtZmFtaWx5PSJBcmlhbCIgZm9udC1zaXplPSIyNCIgZmlsbD0iIzkwOTA5MCIgdGV4dC1hbmNob3I9Im1pZGRsZSIgZG9taW5hbnQtYmFzZWxpbmU9Im1pZGRsZSI+Tm8gSW1hZ2U8L3RleHQ+PC9zdmc+";

    public Service_Tool_DunnageBook(
        IService_MySQL_Dunnage dunnageService,
        IService_LoggingUtility logger
    )
    {
        ArgumentNullException.ThrowIfNull(dunnageService);
        ArgumentNullException.ThrowIfNull(logger);
        _dunnageService = dunnageService;
        _logger = logger;
    }

    /// <inheritdoc />
    public async Task<Model_Dao_Result<List<Model_Tool_DunnageBook_TypeGroup>>> LoadDunnageGroupsAsync()
    {
        try
        {
            var typesResult = await _dunnageService.GetAllTypesAsync();
            if (!typesResult.IsSuccess || typesResult.Data is null)
            {
                return Model_Dao_Result_Factory.Failure<List<Model_Tool_DunnageBook_TypeGroup>>(
                    typesResult.ErrorMessage
                );
            }

            var partsResult = await _dunnageService.GetAllPartsAsync();
            if (!partsResult.IsSuccess || partsResult.Data is null)
            {
                return Model_Dao_Result_Factory.Failure<List<Model_Tool_DunnageBook_TypeGroup>>(
                    partsResult.ErrorMessage
                );
            }

            var fieldsByType = await LoadCustomFieldsByTypeAsync(typesResult.Data);
            var groups = BuildGroups(typesResult.Data, partsResult.Data, fieldsByType);

            _logger.LogInfo($"Loaded {groups.Count} dunnage type group(s) for the book builder.");
            return Model_Dao_Result_Factory.Success(groups);
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error loading dunnage groups: {ex.Message}", ex);
            return Model_Dao_Result_Factory.Failure<List<Model_Tool_DunnageBook_TypeGroup>>(
                $"Failed to load dunnage data: {ex.Message}",
                ex
            );
        }
    }

    /// <inheritdoc />
    public Task<Model_Dao_Result<Model_FormattedReportDocument>> BuildBookAsync(
        IReadOnlyList<Model_Tool_DunnageBook_TypeGroup> typeGroups,
        Model_Tool_DunnageBook_Config config
    )
    {
        try
        {
            ArgumentNullException.ThrowIfNull(typeGroups);
            ArgumentNullException.ThrowIfNull(config);

            var title = string.IsNullOrWhiteSpace(config.CoverTitle)
                ? Model_Tool_DunnageBook_Config.DefaultCoverTitle
                : config.CoverTitle.Trim();

            var sections = typeGroups
                .Select(group => new DunnageBookSection(
                    group.TypeName,
                    group.Entries.Where(entry => entry.IsSelected).ToList()
                ))
                .Where(section => section.Entries.Count > 0)
                .OrderBy(section => section.TypeName, StringComparer.OrdinalIgnoreCase)
                .ToList();

            if (sections.Count == 0)
            {
                return Task.FromResult(
                    Model_Dao_Result_Factory.Failure<Model_FormattedReportDocument>(
                        "No dunnage parts are selected. Select at least one part to generate the book."
                    )
                );
            }

            var totalPages = ComputeTotalPages(sections, config);
            var pageAssignments = AssignSectionStartPages(sections, config);
            var fallbackImage = Helper_ImageDataUri.TryGetDataUri(FallbackImagePath)
                ?? PlaceholderSvgDataUri;
            var coverLogo = config.IncludeCoverPage
                ? Helper_ImageDataUri.TryGetDataUri(CoverLogoPath)
                : null;

            var html = new StringBuilder();
            var plainText = new StringBuilder();
            var currentPage = 1;

            plainText.AppendLine(title);
            plainText.AppendLine();

            if (config.IncludeCoverPage)
            {
                html.Append(BuildCoverPage(title, coverLogo));
                currentPage++;
            }

            if (config.IncludeTableOfContents)
            {
                html.Append(
                    BuildTableOfContentsPages(title, sections, pageAssignments)
                );
                currentPage += CountTableOfContentsPages(sections.Count);
            }

            for (var index = 0; index < sections.Count; index++)
            {
                var section = sections[index];
                var entries = section.Entries
                    .OrderBy(entry => entry.PartId, StringComparer.OrdinalIgnoreCase)
                    .ToList();

                html.Append(
                    BuildSectionPages(
                        title,
                        section.TypeName,
                        entries,
                        fallbackImage,
                        currentPage,
                        totalPages
                    )
                );

                plainText.AppendLine($"{section.TypeName} ({entries.Count})");
                foreach (var entry in entries)
                {
                    plainText.AppendLine(
                        $"  {entry.PartId} - Home: {entry.HomeLocation}, Quantity: {entry.QuantityType}"
                    );
                }

                plainText.AppendLine();
                currentPage += ComputeSectionPageCount(entries.Count);
            }

            var document = new Model_FormattedReportDocument
            {
                DocumentTitle = title,
                HtmlFragment = html.ToString(),
                PlainText = plainText.ToString(),
                PageCss = GetBookPrintCss(),
            };

            _logger.LogInfo($"Built dunnage book '{title}' with {totalPages} page(s).");
            return Task.FromResult(Model_Dao_Result_Factory.Success(document));
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error building dunnage book: {ex.Message}", ex);
            return Task.FromResult(
                Model_Dao_Result_Factory.Failure<Model_FormattedReportDocument>(
                    $"Failed to build the dunnage book: {ex.Message}",
                    ex
                )
            );
        }
    }

    private async Task<Dictionary<int, List<Model_CustomFieldDefinition>>> LoadCustomFieldsByTypeAsync(
        IReadOnlyList<Model_DunnageType> types
    )
    {
        var fieldsByType = new Dictionary<int, List<Model_CustomFieldDefinition>>();

        foreach (var type in types)
        {
            var fieldsResult = await _dunnageService.GetCustomFieldsByTypeAsync(type.Id);
            fieldsByType[type.Id] =
                fieldsResult.IsSuccess && fieldsResult.Data is not null
                    ? fieldsResult.Data.OrderBy(field => field.DisplayOrder).ToList()
                    : [];
        }

        return fieldsByType;
    }

    private static List<Model_Tool_DunnageBook_TypeGroup> BuildGroups(
        IReadOnlyList<Model_DunnageType> types,
        IReadOnlyList<Model_DunnagePart> parts,
        IReadOnlyDictionary<int, List<Model_CustomFieldDefinition>> fieldsByType
    )
    {
        var groups = new List<Model_Tool_DunnageBook_TypeGroup>();
        var partsByType = parts
            .GroupBy(part => part.TypeId)
            .ToDictionary(group => group.Key, group => group.ToList());

        foreach (var type in types.OrderBy(t => t.TypeName, StringComparer.OrdinalIgnoreCase))
        {
            if (!partsByType.TryGetValue(type.Id, out var typeParts))
            {
                continue;
            }

            fieldsByType.TryGetValue(type.Id, out var fields);
            groups.Add(CreateGroup(type.Id, type.TypeName, typeParts, fields ?? []));
        }

        if (partsByType.TryGetValue(0, out var unassignedParts))
        {
            groups.Add(CreateGroup(0, "Unassigned", unassignedParts, []));
        }

        return groups;
    }

    private static Model_Tool_DunnageBook_TypeGroup CreateGroup(
        int typeId,
        string typeName,
        IReadOnlyList<Model_DunnagePart> parts,
        IReadOnlyList<Model_CustomFieldDefinition> fields
    )
    {
        var group = new Model_Tool_DunnageBook_TypeGroup { TypeId = typeId, TypeName = typeName };

        foreach (var part in parts.OrderBy(p => p.PartId, StringComparer.OrdinalIgnoreCase))
        {
            var entry = new Model_Tool_DunnageBook_Entry(part)
            {
                Group = group,
                CustomFieldValues = part
                    .BuildLabeledValues(fields)
                    .Select(pair => new Model_Tool_DunnageBook_FieldValue
                    {
                        Label = pair.Key,
                        Value = pair.Value,
                    })
                    .ToList(),
            };

            group.Entries.Add(entry);
        }

        return group;
    }

    private static int CountTableOfContentsPages(int sectionCount)
    {
        return Math.Max(1, (int)Math.Ceiling(sectionCount / (double)TocEntriesPerPage));
    }

    private static int ComputeSectionPageCount(int entryCount)
    {
        return Math.Max(1, (int)Math.Ceiling(entryCount / (double)CardsPerPage));
    }

    private static int ComputeTotalPages(
        IReadOnlyList<DunnageBookSection> sections,
        Model_Tool_DunnageBook_Config config
    )
    {
        var total = 0;
        if (config.IncludeCoverPage)
        {
            total++;
        }

        if (config.IncludeTableOfContents)
        {
            total += CountTableOfContentsPages(sections.Count);
        }

        total += sections.Sum(section => ComputeSectionPageCount(section.Entries.Count));
        return total;
    }

    private static IReadOnlyDictionary<string, int> AssignSectionStartPages(
        IReadOnlyList<DunnageBookSection> sections,
        Model_Tool_DunnageBook_Config config
    )
    {
        var assignments = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
        var startPage = 1;

        if (config.IncludeCoverPage)
        {
            startPage++;
        }

        if (config.IncludeTableOfContents)
        {
            startPage += CountTableOfContentsPages(sections.Count);
        }

        foreach (var section in sections)
        {
            assignments[section.TypeName] = startPage;
            startPage += ComputeSectionPageCount(section.Entries.Count);
        }

        return assignments;
    }

    private static string BuildCoverPage(string title, string? logoDataUri)
    {
        var html = new StringBuilder();
        html.AppendLine("<div class='page cover-page'>");

        if (!string.IsNullOrWhiteSpace(logoDataUri))
        {
            html.AppendLine("<div class='cover-logo-card'>");
            html.AppendLine($"<img class='cover-logo' src='{logoDataUri}' alt='Company logo' />");
            html.AppendLine("</div>");
        }

        html.AppendLine($"<h1 class='cover-title'>{HtmlEncode(title)}</h1>");
        html.AppendLine(
            $"<div class='cover-subtitle'>Generated by {HtmlEncode(Environment.UserName)} on {HtmlEncode(DateTime.Now.ToString("MMMM d, yyyy"))}</div>"
        );
        html.AppendLine("</div>");

        return html.ToString();
    }

    private static string BuildTableOfContentsPages(
        string title,
        IReadOnlyList<DunnageBookSection> sections,
        IReadOnlyDictionary<string, int> pageAssignments
    )
    {
        var html = new StringBuilder();

        foreach (var chunk in sections.Chunk(TocEntriesPerPage))
        {
            html.AppendLine("<div class='page toc-page'>");
            html.AppendLine($"<h2 class='page-heading'>{HtmlEncode(title)}</h2>");
            html.AppendLine("<h3 class='toc-heading'>Table of Contents</h3>");
            html.AppendLine("<table class='toc-table'>");
            html.AppendLine("<tbody>");

            foreach (var section in chunk)
            {
                var pageNumber = pageAssignments.TryGetValue(section.TypeName, out var assigned)
                    ? assigned
                    : 0;
                html.AppendLine("<tr>");
                html.AppendLine(
                    $"<td class='toc-name'>{HtmlEncode(section.TypeName)} ({section.Entries.Count})</td>"
                );
                html.AppendLine($"<td class='toc-page'>{pageNumber}</td>");
                html.AppendLine("</tr>");
            }

            html.AppendLine("</tbody>");
            html.AppendLine("</table>");
            html.AppendLine("</div>");
        }

        return html.ToString();
    }

    private static string BuildSectionPages(
        string title,
        string typeName,
        IReadOnlyList<Model_Tool_DunnageBook_Entry> entries,
        string fallbackImage,
        int startPage,
        int totalPages
    )
    {
        var html = new StringBuilder();
        var pages = entries.Chunk(CardsPerPage).ToList();

        for (var pageIndex = 0; pageIndex < pages.Count; pageIndex++)
        {
            var pageNumber = startPage + pageIndex;
            html.AppendLine("<div class='page'>");

            if (pageIndex == 0)
            {
                html.AppendLine(
                    $"<div class='section-header'>{HtmlEncode(typeName)} ({entries.Count})</div>"
                );
            }

            html.AppendLine("<div class='card-grid'>");
            foreach (var entry in pages[pageIndex])
            {
                html.Append(BuildCard(entry, fallbackImage));
            }

            html.AppendLine("</div>");
            html.AppendLine(
                $"<div class='page-footer'>{HtmlEncode(title)} — Page {pageNumber} of {totalPages}</div>"
            );
            html.AppendLine("</div>");
        }

        return html.ToString();
    }

    private static string BuildCard(Model_Tool_DunnageBook_Entry entry, string fallbackImage)
    {
        // Read from the shared root (source of truth) rather than the cache-preferring
        // display lookup: the local cache is only refreshed at startup, so after a part
        // image is rotated and re-imported in place it would still resolve the stale,
        // unrotated cached copy. The book must always show the current image.
        var imageDataUri = Helper_ImageDataUri.TryGetDataUri(
            Helper_DunnageImagePaths.GetAbsolutePath(entry.ImagePath)
        ) ?? fallbackImage;

        var html = new StringBuilder();
        html.AppendLine("<div class='card'>");
        html.AppendLine("<div class='card-image'>");
        html.AppendLine($"<img src='{imageDataUri}' alt='{HtmlEncode(entry.PartId)}' />");
        html.AppendLine("</div>");
        html.AppendLine("<div class='card-body'>");
        html.AppendLine($"<div class='card-part'>{HtmlEncode(entry.PartId)}</div>");
        html.AppendLine($"<div class='card-type'>{HtmlEncode(entry.TypeName)}</div>");
        html.AppendLine(
            $"<div class='card-field-row'><span class='card-label'>Home Location:</span> {HtmlEncode(entry.HomeLocation)}</div>"
        );
        html.AppendLine(
            $"<div class='card-field-row'><span class='card-label'>Quantity:</span> {HtmlEncode(entry.QuantityType)}</div>"
        );

        foreach (var field in entry.CustomFieldValues)
        {
            if (string.IsNullOrWhiteSpace(field.Value))
            {
                continue;
            }

            html.AppendLine(
                $"<div class='card-field-row'><span class='card-label'>{HtmlEncode(field.Label)}:</span> {HtmlEncode(field.Value)}</div>"
            );
        }

        html.AppendLine("</div>");
        html.AppendLine("</div>");

        return html.ToString();
    }

    private static string HtmlEncode(string? value)
    {
        return System.Net.WebUtility.HtmlEncode(value ?? string.Empty);
    }

    private sealed record DunnageBookSection(
        string TypeName,
        List<Model_Tool_DunnageBook_Entry> Entries
    );

    private static string GetBookPrintCss()
    {
        return """
@page { size: Letter portrait; margin: 0.3in; }
body { margin: 0; background: #ffffff; font-family: Calibri, Arial, sans-serif; color: #111827; }
.page {
    display: flex;
    flex-direction: column;
    min-height: 10.1in;
    box-sizing: border-box;
    page-break-after: always;
    break-after: page;
}
.page:last-child { page-break-after: auto; break-after: auto; }
.page-footer {
    margin-top: auto;
    padding-top: 0.2in;
    font-size: 9pt;
    color: #6b7280;
    text-align: center;
    border-top: 1px solid #e5e7eb;
}
.page-heading { font-size: 18pt; margin: 0 0 0.1in 0; }
.section-header {
    font-size: 13pt;
    font-weight: 700;
    color: #ffffff;
    background-color: #1f4e78;
    border-radius: 6px;
    padding: 0.12in 0.16in;
    margin-bottom: 0.14in;
    break-inside: avoid;
}
.card-grid {
    display: grid;
    grid-template-columns: 1fr 1fr;
    grid-auto-rows: 2.9in;
    gap: 0.14in;
}
.card {
    display: flex;
    flex-direction: row;
    border: 1px solid #d1d5db;
    border-radius: 8px;
    overflow: hidden;
    break-inside: avoid;
    page-break-inside: avoid;
    background: #ffffff;
}
.card-image {
    width: 2.1in;
    flex-shrink: 0;
    background: #f3f4f6;
    display: flex;
    align-items: center;
    justify-content: center;
    padding: 0.08in;
    box-sizing: border-box;
}
.card-image img { max-width: 100%; max-height: 2.5in; object-fit: contain; }
.card-body { flex: 1; padding: 0.14in 0.16in; box-sizing: border-box; overflow: hidden; }
.card-part { font-size: 13pt; font-weight: 700; margin-bottom: 0.02in; }
.card-type { font-size: 9pt; font-weight: 600; color: #1f4e78; text-transform: uppercase; letter-spacing: 0.03em; margin-bottom: 0.08in; }
.card-field-row { font-size: 9.5pt; line-height: 1.3; }
.card-label { font-weight: 700; }
.cover-page { align-items: center; justify-content: center; text-align: center; }
.cover-logo-card {
    border: 1px solid #d1d5db;
    border-radius: 12px;
    padding: 0.25in;
    margin-bottom: 0.4in;
    background: #ffffff;
}
.cover-logo { max-height: 1.4in; max-width: 3.5in; object-fit: contain; }
.cover-title { font-size: 30pt; margin: 0 0 0.15in 0; }
.cover-subtitle { font-size: 12pt; color: #4b5563; }
.toc-heading { font-size: 16pt; margin: 0.1in 0 0.15in 0; border-bottom: 2px solid #1f4e78; padding-bottom: 0.08in; }
.toc-table { width: 100%; border-collapse: collapse; }
.toc-table td { padding: 0.1in 0; font-size: 11pt; border-bottom: 1px solid #e5e7eb; }
.toc-name { text-align: left; }
.toc-page { text-align: right; font-weight: 700; color: #1f4e78; }
@media print {
    body { -webkit-print-color-adjust: exact; print-color-adjust: exact; }
}
""";
    }
}
