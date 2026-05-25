using System.Collections.Generic;
using System.Linq;
using FluentValidation;
using MediatR;
using MTM_Receiving_Application.Module_Core.Models.Core;
using MTM_Receiving_Application.Module_ShipRec_Tools.Models;

namespace MTM_Receiving_Application.Module_ShipRec_Tools.Services.CustomerPullPack.Commands;

/// <summary>
/// Saves one Customer Pull n' Pack waitlist entry per selected source line.
/// </summary>
/// <param name="Entries"></param>
public sealed record Command_CustomerPullPackBatchUpsert(
    IReadOnlyList<Model_CustomerPullPack_WaitlistEntry> Entries
) : IRequest<Model_Dao_Result<List<Model_CustomerPullPack_WaitlistEntry>>>;

/// <summary>
/// Validates Customer Pull n' Pack batch upsert requests.
/// </summary>
public class Command_CustomerPullPackBatchUpsertValidator
    : AbstractValidator<Command_CustomerPullPackBatchUpsert>
{
    public Command_CustomerPullPackBatchUpsertValidator()
    {
        RuleFor(command => command.Entries).NotEmpty();

        RuleForEach(command => command.Entries)
            .NotNull()
            .ChildRules(entry =>
            {
                entry.RuleFor(item => item.SourceLineKey).NotEmpty();
                entry.RuleFor(item => item.CustomerId).NotEmpty();
                entry.RuleFor(item => item.ParentPartId).NotEmpty();
                entry.RuleFor(item => item.RequestedByUserId).NotEmpty();
                entry.RuleFor(item => item.RequestedQuantity).GreaterThan(0);
                entry
                    .RuleFor(item => item.RequesterContextNote)
                    .NotEmpty()
                    .When(item => item.SelectedLocations.Count == 0);
            });

        RuleFor(command => command.Entries)
            .Must(entries =>
            {
                var normalized = entries
                    .Where(static entry => entry is not null)
                    .Select(static entry => entry.SourceLineKey.Trim().ToUpperInvariant())
                    .ToList();
                return normalized.Distinct().Count() == normalized.Count;
            })
            .WithMessage("Each source line can appear only once in a batch save.");

        RuleFor(command => command.Entries)
            .Must(entries =>
            {
                var customers = entries
                    .Where(static entry => string.IsNullOrWhiteSpace(entry.CustomerId) is false)
                    .Select(static entry => entry.CustomerId.Trim().ToUpperInvariant())
                    .Distinct()
                    .ToList();
                return customers.Count <= 1;
            })
            .WithMessage("All selected source lines must belong to the same customer.");

        RuleFor(command => command.Entries)
            .Must(entries =>
            {
                var parentParts = entries
                    .Where(static entry => string.IsNullOrWhiteSpace(entry.ParentPartId) is false)
                    .Select(static entry => entry.ParentPartId.Trim().ToUpperInvariant())
                    .Distinct()
                    .ToList();
                return parentParts.Count <= 1;
            })
            .WithMessage("All selected source lines must belong to the same parent part.");
    }
}
