using FluentAssertions;
using MTM_Receiving_Application.Module_Dunnage.Models;

namespace MTM_Receiving_Application.Tests.Unit.Module_Dunnage.Models;

public sealed class Model_CustomFieldDefinitionTests
{
    [Theory]
    [InlineData(1, "udc1")]
    [InlineData(10, "udc10")]
    [InlineData(0, "")]
    [InlineData(11, "")]
    public void UdcColumnName_ShouldMapDisplayOrderToSlotColumn(
        int displayOrder,
        string expectedColumnName
    )
    {
        var field = new Model_CustomFieldDefinition { DisplayOrder = displayOrder };

        field.UdcColumnName.Should().Be(expectedColumnName);
    }
}
