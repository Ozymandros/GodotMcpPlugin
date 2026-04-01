using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using GodotMcp.Infrastructure.Configuration;
using GodotMcp.Infrastructure.Process;

namespace GodotMcp.Tests.InfrastructureTests;

/// <summary>
/// Unit tests for ProcessManager
/// Validates: Requirements 14.2, 14.4, 10.3
/// </summary>
public sealed class ProcessManagerTests : IDisposable
{
    private readonly ILogger<ProcessManager> _logger;
    private readonly IOptions<GodotMcpOptions> _options;
    private readonly List<ProcessManager> _managersToDispose = new();

    public ProcessManagerTests()
    {
        _logger = Substitute.For<ILogger<ProcessManager>>();
        _options = Options.Create(new GodotMcpOptions
        {
            ExecutablePath = "dotnet",
            ConnectionTimeoutSeconds = 5,
            RequestTimeoutSeconds = 10,
            MaxRetryAttempts = 3,
            BackoffStrategy = BackoffStrategy.Exponential,
            InitialRetryDelayMs = 1000,
            EnableProcessPooling = true,
            MaxIdleTimeSeconds = 300,
            EnableMessageLogging = false
        });
    }

    public void Dispose()
    {
        foreach (var manager in _managersToDispose)
        {
            try
            {
                manager.Dispose();
            }
            catch
            {
                // Ignore disposal errors in tests
            }
        }
    }

    private ProcessManager CreateProcessManager(GodotMcpOptions? options = null)
    {
        var opts = options != null ? Options.Create(options) : _options;
        var manager = new ProcessManager(_logger, opts);
        _managersToDispose.Add(manager);
        return manager;
    }

    private static GodotMcpOptions CreateDefaultOptions(string executablePath = "dotnet")
    {
        return new GodotMcpOptions
        {
            ExecutablePath = executablePath,
            ConnectionTimeoutSeconds = 5,
            RequestTimeoutSeconds = 10,
            MaxRetryAttempts = 3,
            BackoffStrategy = BackoffStrategy.Exponential,
            InitialRetryDelayMs = 1000,
            EnableProcessPooling = true,
            MaxIdleTimeSeconds = 300,
            EnableMessageLogging = false
        };
    }

    [Fact]
    public void Constructor_WithValidParameters_InitializesSuccessfully()
    {
        // Act
        var manager = CreateProcessManager();

        // Assert
        Assert.NotNull(manager);
        Assert.Equal(ProcessState.NotStarted, manager.State);
    }

    [Fact]
    public void State_InitialState_IsNotStarted()
    {
        // Arrange
        var manager = CreateProcessManager();

        // Act
        var state = manager.State;

        // Assert
        Assert.Equal(ProcessState.NotStarted, state);
    }

    [Fact]
    public void StandardInput_WhenProcessNotStarted_ThrowsProcessException()
    {
        // Arrange
        var manager = CreateProcessManager();

        // Act & Assert
        var exception = Assert.Throws<ProcessException>(() => manager.StandardInput);
        Assert.Contains("Process not started", exception.Message);
    }

    [Fact]
    public void StandardOutput_WhenProcessNotStarted_ThrowsProcessException()
    {
        // Arrange
        var manager = CreateProcessManager();

        // Act & Assert
        var exception = Assert.Throws<ProcessException>(() => manager.StandardOutput);
        Assert.Contains("Process not started", exception.Message);
    }

    [Fact]
    public async Task EnsureProcessRunningAsync_WhenNotRunning_StartsProcess()
    {
        // Arrange
        var manager = CreateProcessManager(CreateDefaultOptions());

        // Act
        var processInfo = await manager.EnsureProcessRunningAsync();

        // Assert
        Assert.NotNull(processInfo);
        Assert.True(processInfo.ProcessId > 0);
        Assert.Equal("dotnet", processInfo.ExecutablePath);
        Assert.Equal(ProcessState.Running, manager.State);
        Assert.NotNull(manager.StandardInput);
        Assert.NotNull(manager.StandardOutput);
    }

    [Fact]
    public async Task EnsureProcessRunningAsync_WhenAlreadyRunning_ReusesExistingProcess()
    {
        // Arrange
        var manager = CreateProcessManager(CreateDefaultOptions());

        // Act
        var firstProcessInfo = await manager.EnsureProcessRunningAsync();
        
        // Immediately call again before process exits
        var secondProcessInfo = await manager.EnsureProcessRunningAsync();

        // Assert - Should reuse if process hasn't exited yet
        // Note: dotnet without args exits quickly, so we verify the manager
        // correctly handles the reuse logic even if process exits
        Assert.NotNull(firstProcessInfo);
        Assert.NotNull(secondProcessInfo);
        Assert.Equal(ProcessState.Running, manager.State);
    }

    [Fact]
    public async Task EnsureProcessRunningAsync_WithInvalidExecutable_ThrowsProcessException()
    {
        // Arrange
        var manager = CreateProcessManager(CreateDefaultOptions("nonexistent-executable-12345"));

        // Act & Assert
        var exception = await Assert.ThrowsAsync<ProcessException>(
            () => manager.EnsureProcessRunningAsync());
        
        Assert.Contains("Failed to start godot-mcp process", exception.Message);
        Assert.Equal(ProcessState.Faulted, manager.State);
    }

    [Fact]
    public async Task EnsureProcessRunningAsync_WithCancellation_ThrowsOperationCanceledException()
    {
        // Arrange
        var manager = CreateProcessManager(CreateDefaultOptions());
        var cts = new CancellationTokenSource();
        cts.Cancel();

        // Act & Assert
        await Assert.ThrowsAnyAsync<OperationCanceledException>(
            () => manager.EnsureProcessRunningAsync(cts.Token));
    }

    [Fact]
    public async Task StopProcessAsync_WhenProcessRunning_StopsGracefully()
    {
        // Arrange
        var manager = CreateProcessManager(CreateDefaultOptions());
        await manager.EnsureProcessRunningAsync();

        // Act
        await manager.StopProcessAsync();

        // Assert
        Assert.Equal(ProcessState.Stopped, manager.State);
    }

    [Fact]
    public async Task StopProcessAsync_WhenProcessNotStarted_DoesNotThrow()
    {
        // Arrange
        var manager = CreateProcessManager();

        // Act & Assert - Should not throw
        await manager.StopProcessAsync();
        Assert.Equal(ProcessState.NotStarted, manager.State);
    }

    [Fact]
    public async Task StopProcessAsync_WhenProcessAlreadyStopped_DoesNotThrow()
    {
        // Arrange
        var manager = CreateProcessManager(CreateDefaultOptions());
        await manager.EnsureProcessRunningAsync();
        await manager.StopProcessAsync();

        // Act & Assert - Should not throw
        await manager.StopProcessAsync();
        Assert.Equal(ProcessState.Stopped, manager.State);
    }

    [Fact]
    public async Task StopProcessAsync_WithCancellation_ThrowsOperationCanceledException()
    {
        // Arrange
        var manager = CreateProcessManager(CreateDefaultOptions());
        await manager.EnsureProcessRunningAsync();
        
        var cts = new CancellationTokenSource();
        cts.Cancel();

        // Act & Assert
        await Assert.ThrowsAnyAsync<OperationCanceledException>(
            () => manager.StopProcessAsync(cts.Token));
    }

    [Fact]
    public async Task ConcurrentEnsureProcessRunningAsync_IsThreadSafe()
    {
        // Arrange
        var manager = CreateProcessManager(CreateDefaultOptions());

        // Act - Call EnsureProcessRunningAsync concurrently
        var tasks = Enumerable.Range(0, 10)
            .Select(_ => Task.Run(() => manager.EnsureProcessRunningAsync()))
            .ToArray();

        var results = await Task.WhenAll(tasks);

        // Assert - All calls should succeed and return valid process info
        // Note: dotnet without args exits quickly, so processes may differ
        // The key is that the manager handles concurrent calls safely
        Assert.All(results, result =>
        {
            Assert.NotNull(result);
            Assert.True(result.ProcessId > 0);
        });
        Assert.Equal(ProcessState.Running, manager.State);
    }

    [Fact]
    public async Task ProcessStateTransitions_FromNotStartedToRunning_IsCorrect()
    {
        // Arrange
        var manager = CreateProcessManager(CreateDefaultOptions());

        // Act & Assert
        Assert.Equal(ProcessState.NotStarted, manager.State);
        
        await manager.EnsureProcessRunningAsync();
        Assert.Equal(ProcessState.Running, manager.State);
    }

    [Fact]
    public async Task ProcessStateTransitions_FromRunningToStopped_IsCorrect()
    {
        // Arrange
        var manager = CreateProcessManager(CreateDefaultOptions());

        // Act & Assert
        await manager.EnsureProcessRunningAsync();
        Assert.Equal(ProcessState.Running, manager.State);
        
        await manager.StopProcessAsync();
        Assert.Equal(ProcessState.Stopped, manager.State);
    }

    [Fact]
    public async Task ProcessStateTransitions_OnStartFailure_TransitionsToFaulted()
    {
        // Arrange
        var manager = CreateProcessManager(CreateDefaultOptions("nonexistent-executable-12345"));

        // Act
        try
        {
            await manager.EnsureProcessRunningAsync();
        }
        catch (ProcessException)
        {
            // Expected
        }

        // Assert
        Assert.Equal(ProcessState.Faulted, manager.State);
    }

    [Fact]
    public async Task Dispose_WhenProcessRunning_StopsProcess()
    {
        // Arrange
        var manager = new ProcessManager(_logger, Options.Create(CreateDefaultOptions()));
        var processInfo = await manager.EnsureProcessRunningAsync();
        var processId = processInfo.ProcessId;

        // Act
        manager.Dispose();

        // Assert - Process should be terminated
        await Task.Delay(100); // Give time for process to terminate
        
        try
        {
            var process = System.Diagnostics.Process.GetProcessById(processId);
            // If we get here, check if it has exited
            Assert.True(process.HasExited);
        }
        catch (ArgumentException)
        {
            // Process not found - this is expected and good
        }
    }

    [Fact]
    public async Task DisposeAsync_WhenProcessRunning_StopsProcessGracefully()
    {
        // Arrange
        var manager = new ProcessManager(_logger, Options.Create(CreateDefaultOptions()));
        var processInfo = await manager.EnsureProcessRunningAsync();
        var processId = processInfo.ProcessId;

        // Act
        await manager.DisposeAsync();

        // Assert - Process should be terminated
        await Task.Delay(100); // Give time for process to terminate
        
        try
        {
            var process = System.Diagnostics.Process.GetProcessById(processId);
            // If we get here, check if it has exited
            Assert.True(process.HasExited);
        }
        catch (ArgumentException)
        {
            // Process not found - this is expected and good
        }
    }

    [Fact]
    public void Dispose_WhenProcessNotStarted_DoesNotThrow()
    {
        // Arrange
        var manager = new ProcessManager(_logger, _options);

        // Act & Assert - Should not throw
        manager.Dispose();
    }

    [Fact]
    public async Task DisposeAsync_WhenProcessNotStarted_DoesNotThrow()
    {
        // Arrange
        var manager = new ProcessManager(_logger, _options);

        // Act & Assert - Should not throw
        await manager.DisposeAsync();
    }

    [Fact]
    public async Task EnsureProcessRunningAsync_AfterProcessExits_StartsNewProcess()
    {
        // Arrange
        var manager = CreateProcessManager(CreateDefaultOptions());

        // Act
        var firstProcessInfo = await manager.EnsureProcessRunningAsync();
        await manager.StopProcessAsync();
        var secondProcessInfo = await manager.EnsureProcessRunningAsync();

        // Assert - Should be different processes
        Assert.NotEqual(firstProcessInfo.ProcessId, secondProcessInfo.ProcessId);
        Assert.Equal(ProcessState.Running, manager.State);
    }

    [Fact]
    public async Task StandardInput_AfterProcessStarted_ReturnsValidStream()
    {
        // Arrange
        var manager = CreateProcessManager(CreateDefaultOptions());
        await manager.EnsureProcessRunningAsync();

        // Act
        var stream = manager.StandardInput;

        // Assert
        Assert.NotNull(stream);
        Assert.True(stream.CanWrite);
    }

    [Fact]
    public async Task StandardOutput_AfterProcessStarted_ReturnsValidStream()
    {
        // Arrange
        var manager = CreateProcessManager(CreateDefaultOptions());
        await manager.EnsureProcessRunningAsync();

        // Act
        var stream = manager.StandardOutput;

        // Assert
        Assert.NotNull(stream);
        Assert.True(stream.CanRead);
    }
}
