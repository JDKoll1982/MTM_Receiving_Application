using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using MTM_Receiving_Application.Module_Core.Models.Core;
using MTM_Receiving_Application.Module_Core.Models.InforVisual;
using MTM_Receiving_Application.Module_Shared.Contracts.Lookup;
using MTM_Receiving_Application.Module_Shared.Enums;
using MTM_Receiving_Application.Module_Shared.Models.Lookup;
using MTM_Receiving_Application.Module_Shared.Services.Lookup;

namespace MTM_Receiving_Application.Tests.Unit.Module_Shared.Services;

public class Service_SharedLookupWorkflowTests
{
    [Fact]
    public async Task ValidateAsync_ShouldReturnInvalid_WhenRawInputFailsValidation()
    {
        var strategy = new TestLookupStrategy(Enum_SharedLookupType.PartNumber)
        {
            ValidateRawInputOverride = _ => Model_Dao_Result_Factory.Failure("Part number is required."),
        };

        var service = new Service_SharedLookupWorkflow([strategy]);

        var result = await service.ValidateAsync(new Model_SharedLookupRequest
        {
            LookupType = Enum_SharedLookupType.PartNumber,
            RawInput = string.Empty,
        });

        result.IsValid.Should().BeFalse();
        result.Message.Should().Be("Part number is required.");
        strategy.HasExactMatchCalls.Should().Be(0);
        strategy.SearchFuzzyCalls.Should().Be(0);
    }

    [Fact]
    public async Task ValidateAsync_ShouldApplyFormattingAndReturnExactMatch_WhenExactExists()
    {
        var strategy = new TestLookupStrategy(Enum_SharedLookupType.PartNumber)
        {
            ApplyFormattingOverride = _ => new Model_SharedLookupFormattingResult
            {
                FormattedValue = "MMC0001000",
                HasFormattingRule = true,
                WasFormatted = true,
            },
            HasExactMatchOverride = (_, _, _) =>
                Task.FromResult(Model_Dao_Result_Factory.Success(true)),
        };

        var service = new Service_SharedLookupWorkflow([strategy]);

        var result = await service.ValidateAsync(new Model_SharedLookupRequest
        {
            LookupType = Enum_SharedLookupType.PartNumber,
            RawInput = "MMC1000",
        });

        result.IsValid.Should().BeTrue();
        result.HasFormattingRule.Should().BeTrue();
        result.WasFormatted.Should().BeTrue();
        result.HasExactMatch.Should().BeTrue();
        result.UsedFuzzyFallback.Should().BeFalse();
        result.ResolvedValue.Should().Be("MMC0001000");
    }

    [Fact]
    public async Task ValidateAsync_ShouldReturnExactMatch_WhenExactFoundWithoutFormattingRule()
    {
        var strategy = new TestLookupStrategy(Enum_SharedLookupType.Location)
        {
            ApplyFormattingOverride = _ => new Model_SharedLookupFormattingResult
            {
                FormattedValue = "B1-04",
                HasFormattingRule = false,
                WasFormatted = false,
            },
            HasExactMatchOverride = (_, _, _) =>
                Task.FromResult(Model_Dao_Result_Factory.Success(true)),
        };

        var service = new Service_SharedLookupWorkflow([strategy]);

        var result = await service.ValidateAsync(new Model_SharedLookupRequest
        {
            LookupType = Enum_SharedLookupType.Location,
            RawInput = "B1-04",
        });

        result.IsValid.Should().BeTrue();
        result.HasExactMatch.Should().BeTrue();
        result.UsedFuzzyFallback.Should().BeFalse();
        result.ResolvedValue.Should().Be("B1-04");
    }

    [Fact]
    public async Task ValidateAsync_ShouldReturnFuzzyFallback_WhenExactDoesNotExist()
    {
        var strategy = new TestLookupStrategy(Enum_SharedLookupType.PartNumber)
        {
            ApplyFormattingOverride = _ => new Model_SharedLookupFormattingResult
            {
                FormattedValue = "MMC999",
                HasFormattingRule = false,
                WasFormatted = false,
            },
            HasExactMatchOverride = (_, _, _) =>
                Task.FromResult(Model_Dao_Result_Factory.Success(false)),
            SearchFuzzyOverride = (_, _, _) =>
                Task.FromResult(Model_Dao_Result_Factory.Success(new List<Model_FuzzySearchResult>
                {
                    new() { Key = "MMC0000999", Label = "MMC0000999" },
                })),
            SelectBestFuzzyResultOverride = (_, candidates) => candidates[0].Label,
        };

        var service = new Service_SharedLookupWorkflow([strategy]);

        var result = await service.ValidateAsync(new Model_SharedLookupRequest
        {
            LookupType = Enum_SharedLookupType.PartNumber,
            RawInput = "MMC999",
        });

        result.IsValid.Should().BeTrue();
        result.HasExactMatch.Should().BeFalse();
        result.UsedFuzzyFallback.Should().BeTrue();
        result.ResolvedValue.Should().Be("MMC0000999");
    }

    [Fact]
    public async Task ValidateAsync_ShouldReturnInvalid_WhenFuzzyCannotResolveCandidate()
    {
        var strategy = new TestLookupStrategy(Enum_SharedLookupType.PartNumber)
        {
            HasExactMatchOverride = (_, _, _) =>
                Task.FromResult(Model_Dao_Result_Factory.Success(false)),
            SearchFuzzyOverride = (_, _, _) =>
                Task.FromResult(Model_Dao_Result_Factory.Success(new List<Model_FuzzySearchResult>())),
            SelectBestFuzzyResultOverride = (_, _) => string.Empty,
        };

        var service = new Service_SharedLookupWorkflow([strategy]);

        var result = await service.ValidateAsync(new Model_SharedLookupRequest
        {
            LookupType = Enum_SharedLookupType.PartNumber,
            RawInput = "UNKNOWN",
        });

        result.IsValid.Should().BeFalse();
        result.HasExactMatch.Should().BeFalse();
        result.UsedFuzzyFallback.Should().BeFalse();
        result.ResolvedValue.Should().BeEmpty();
    }

    private sealed class TestLookupStrategy : ISharedLookupStrategy
    {
        public TestLookupStrategy(Enum_SharedLookupType lookupType)
        {
            LookupType = lookupType;
        }

        public Enum_SharedLookupType LookupType { get; }

        public int HasExactMatchCalls { get; private set; }

        public int SearchFuzzyCalls { get; private set; }

        public Func<string, Model_Dao_Result>? ValidateRawInputOverride { get; init; }

        public Func<Model_SharedLookupRequest, Model_SharedLookupFormattingResult>? ApplyFormattingOverride { get; init; }

        public Func<string, Model_SharedLookupRequest, CancellationToken, Task<Model_Dao_Result<bool>>>? HasExactMatchOverride { get; init; }

        public Func<string, Model_SharedLookupRequest, CancellationToken, Task<Model_Dao_Result<List<Model_FuzzySearchResult>>>>? SearchFuzzyOverride { get; init; }

        public Func<string, IReadOnlyList<Model_FuzzySearchResult>, string>? SelectBestFuzzyResultOverride { get; init; }

        public Model_Dao_Result ValidateRawInput(string rawInput)
        {
            return ValidateRawInputOverride?.Invoke(rawInput) ?? Model_Dao_Result_Factory.Success();
        }

        public Model_SharedLookupFormattingResult ApplyFormatting(Model_SharedLookupRequest request)
        {
            return ApplyFormattingOverride?.Invoke(request) ?? new Model_SharedLookupFormattingResult
            {
                FormattedValue = request.RawInput,
                HasFormattingRule = false,
                WasFormatted = false,
            };
        }

        public Task<Model_Dao_Result<bool>> HasExactMatchAsync(
            string formattedValue,
            Model_SharedLookupRequest request,
            CancellationToken cancellationToken = default
        )
        {
            HasExactMatchCalls++;
            return HasExactMatchOverride?.Invoke(formattedValue, request, cancellationToken)
                ?? Task.FromResult(Model_Dao_Result_Factory.Success(false));
        }

        public Task<Model_Dao_Result<List<Model_FuzzySearchResult>>> SearchFuzzyAsync(
            string formattedValue,
            Model_SharedLookupRequest request,
            CancellationToken cancellationToken = default
        )
        {
            SearchFuzzyCalls++;
            return SearchFuzzyOverride?.Invoke(formattedValue, request, cancellationToken)
                ?? Task.FromResult(
                    Model_Dao_Result_Factory.Success(new List<Model_FuzzySearchResult>())
                );
        }

        public string SelectBestFuzzyResult(
            string formattedValue,
            IReadOnlyList<Model_FuzzySearchResult> candidates
        )
        {
            return SelectBestFuzzyResultOverride?.Invoke(formattedValue, candidates)
                ?? string.Empty;
        }
    }
}
