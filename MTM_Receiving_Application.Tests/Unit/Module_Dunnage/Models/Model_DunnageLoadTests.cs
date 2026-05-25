using FluentAssertions;
using MTM_Receiving_Application.Module_Dunnage.Models;

namespace MTM_Receiving_Application.Tests.Unit.Module_Dunnage.Models;

public sealed class Model_DunnageLoadTests
{
    [Fact]
    public void PoNumber_WhenBlanketOrderSuffixIsEntered_ShouldNormalizeToCanonicalUppercaseFormat()
    {
        var load = new Model_DunnageLoad();

        load.PoNumber = "po-65421b";

        load.PoNumber.Should().Be("PO-065421B");
    }
}
