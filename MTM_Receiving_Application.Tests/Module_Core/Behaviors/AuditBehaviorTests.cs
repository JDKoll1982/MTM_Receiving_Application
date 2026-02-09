using FluentAssertions;
using MediatR;
using Microsoft.Extensions.Logging;
using Moq;
using MTM_Receiving_Application.Module_Core.Behaviors;
using MTM_Receiving_Application.Module_Core.Models.Core;
using Xunit;

namespace MTM_Receiving_Application.Tests.Module_Core.Behaviors;

/// <summary>
/// Unit tests for AuditBehavior pipeline behavior.
/// Tests that audit logging occurs for all requests passing through the pipeline.
/// </summary>
public class AuditBehaviorTests
{
    private readonly Mock<ILogger<AuditBehavior<TestRequest, TestResponse>>> _mockLogger;

    public AuditBehaviorTests()
    {
        _mockLogger = new Mock<ILogger<AuditBehavior<TestRequest, TestResponse>>>();
    }

    private AuditBehavior<TestRequest, TestResponse> CreateAuditBehavior()
    {
        return new AuditBehavior<TestRequest, TestResponse>(_mockLogger.Object);
    }

    [Fact]
    public async Task Handle_ShouldLogAuditInformation_WhenRequestIsProcessed()
    {
        // Arrange
        var auditBehavior = CreateAuditBehavior();
        var request = new TestRequest { Data = "test" };
        var expectedResponse = new TestResponse { Success = true };
        var cancellationToken = CancellationToken.None;

        RequestHandlerDelegate<TestResponse> next = (_) => Task.FromResult(expectedResponse);

        // Act
        var result = await auditBehavior.Handle(request, next, cancellationToken);

        // Assert
        result.Should().Be(expectedResponse);
        
        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Audit:") && v.ToString()!.Contains("TestRequest")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    public async Task Handle_ShouldCallNextDelegate_WhenProcessingRequest()
    {
        // Arrange
        var auditBehavior = CreateAuditBehavior();
        var request = new TestRequest { Data = "test" };
        var expectedResponse = new TestResponse { Success = true };
        var cancellationToken = CancellationToken.None;
        var nextCalled = false;

        RequestHandlerDelegate<TestResponse> nextDelegate = (_) =>
        {
            nextCalled = true;
            return Task.FromResult(expectedResponse);
        };

        // Act
        var result = await auditBehavior.Handle(request, nextDelegate, cancellationToken);

        // Assert
        nextCalled.Should().BeTrue();
        result.Should().Be(expectedResponse);
    }

    [Fact]
    public async Task Handle_ShouldIncludeUserName_InAuditLog()
    {
        // Arrange
        var auditBehavior = CreateAuditBehavior();
        var request = new TestRequest { Data = "test" };
        var expectedResponse = new TestResponse { Success = true };
        var cancellationToken = CancellationToken.None;

        RequestHandlerDelegate<TestResponse> next = (_) => Task.FromResult(expectedResponse);

        // Act
        await auditBehavior.Handle(request, next, cancellationToken);

        // Assert
        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains(Environment.UserName)),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    public async Task Handle_ShouldIncludeMachineName_InAuditLog()
    {
        // Arrange
        var auditBehavior = CreateAuditBehavior();
        var request = new TestRequest { Data = "test" };
        var expectedResponse = new TestResponse { Success = true };
        var cancellationToken = CancellationToken.None;

        RequestHandlerDelegate<TestResponse> next = (_) => Task.FromResult(expectedResponse);

        // Act
        await auditBehavior.Handle(request, next, cancellationToken);

        // Assert
        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains(Environment.MachineName)),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    public async Task Handle_ShouldPropagateException_WhenNextThrows()
    {
        // Arrange
        var auditBehavior = CreateAuditBehavior();
        var request = new TestRequest { Data = "test" };
        var cancellationToken = CancellationToken.None;
        var expectedException = new InvalidOperationException("Test exception");

        RequestHandlerDelegate<TestResponse> nextDelegate = (_) => throw expectedException;

        // Act & Assert
        var act = async () => await auditBehavior.Handle(request, nextDelegate, cancellationToken);
        await act.Should().ThrowAsync<InvalidOperationException>().WithMessage("Test exception");
        
        // Audit log should still be written before the exception
        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Audit:")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    #region Test Types

    /// <summary>
    /// Test request type for AuditBehavior testing.
    /// </summary>
    public record TestRequest : IRequest<TestResponse>
    {
        public string Data { get; init; } = string.Empty;
    }

    /// <summary>
    /// Test response type for AuditBehavior testing.
    /// </summary>
    public record TestResponse
    {
        public bool Success { get; init; }
    }

    #endregion
}
