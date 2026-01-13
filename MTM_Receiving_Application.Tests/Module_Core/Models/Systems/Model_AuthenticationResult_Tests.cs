using System;
using FluentAssertions;
using MTM_Receiving_Application.Module_Core.Models.Systems;
using Xunit;

namespace MTM_Receiving_Application.Tests.Module_Core.Models.Systems
{
    /// <summary>
    /// Unit tests for Model_AuthenticationResult
    /// </summary>
    [Trait("Category", "Unit")]
    [Trait("Layer", "Model")]
    public class Model_AuthenticationResult_Tests
    {
        [Fact]
        public void SuccessResult_SetsSuccessTrue()
        {
            var user = new Model_User();

            var result = Model_AuthenticationResult.SuccessResult(user);

            result.Success.Should().BeTrue();
        }

        [Fact]
        public void SuccessResult_SetsUser()
        {
            var user = new Model_User();

            var result = Model_AuthenticationResult.SuccessResult(user);

            result.User.Should().BeSameAs(user);
        }

        [Fact]
        public void SuccessResult_SetsErrorMessageEmpty()
        {
            var user = new Model_User();

            var result = Model_AuthenticationResult.SuccessResult(user);

            result.ErrorMessage.Should().BeEmpty();
        }

        [Fact]
        public void SuccessResult_WithNullUser_Throws()
        {
            var act = static () => Model_AuthenticationResult.SuccessResult(null!);

            act.Should().Throw<ArgumentNullException>();
        }

        [Fact]
        public void ErrorResult_SetsSuccessFalse()
        {
            var result = Model_AuthenticationResult.ErrorResult("bad");

            result.Success.Should().BeFalse();
        }

        [Fact]
        public void ErrorResult_SetsUserNull()
        {
            var result = Model_AuthenticationResult.ErrorResult("bad");

            result.User.Should().BeNull();
        }

        [Fact]
        public void ErrorResult_SetsErrorMessage()
        {
            var result = Model_AuthenticationResult.ErrorResult("bad");

            result.ErrorMessage.Should().Be("bad");
        }

        [Fact]
        public void ErrorResult_WithNullMessage_Throws()
        {
            var act = static () => Model_AuthenticationResult.ErrorResult(null!);

            act.Should().Throw<ArgumentException>();
        }

        [Fact]
        public void ErrorResult_WithWhitespaceMessage_Throws()
        {
            var act = static () => Model_AuthenticationResult.ErrorResult("   ");

            act.Should().Throw<ArgumentException>();
        }
    }
}
