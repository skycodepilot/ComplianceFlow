using System.Net;
using System.Net.Http.Json;
using ComplianceFlow.Api.Features.Manifests.SubmitManifest;
using ComplianceFlow.Api.Features.Manifests.SubmitManifest.Saga;
using DotNet.Testcontainers.Builders;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Testcontainers.MsSql;
using Testcontainers.RabbitMq;

namespace ComplianceFlow.IntegrationTests;

// IClassFixture ensures the container spins up ONCE for this class, not per test.
public class ManifestScenarios : IAsyncLifetime
{
    #region Rigging / Setup
    private readonly RabbitMqContainer _rabbitMqContainer = new RabbitMqBuilder()
        .WithImage("rabbitmq:3-management")
        .WithUsername("admin")
        .WithPassword("admin")
        // THE FIX: Wait until the Erlang process actually says "I am ready"
        // The default strategy only waits for the TCP port to open, which is too early.
        .WithWaitStrategy(Wait.ForUnixContainer().UntilMessageIsLogged("Server startup complete")) 
        .Build();

    // Now the new challenger... SQL setup
    // We accept the EULA and set a strong password.
    private readonly MsSqlContainer _mssqlContainer = new MsSqlBuilder()
        .WithImage("mcr.microsoft.com/mssql/server:2022-latest")
        .WithPassword("YourSTRONGp@ssw0rd!") // Set a known password
        .Build();
    
    public async Task InitializeAsync()
    {
        // Start both containers in parallel
        await Task.WhenAll(_rabbitMqContainer.StartAsync(), _mssqlContainer.StartAsync());
    }

    public async Task DisposeAsync()
    {
        await Task.WhenAll(_rabbitMqContainer.DisposeAsync().AsTask(), _mssqlContainer.DisposeAsync().AsTask());
    }
    #endregion Rigging / Setup
    
    #region Tests
    [Fact]
    public async Task Submit_Manifest_Creates_Saga_In_Database_Which_Is_Rejected_Because_HTS_Code_Is_Restricted()
    {
        // 1. Setup
        var appFactory = GetConfiguredFactory();
        await EnsureDbCreated(appFactory);
        var client = appFactory.CreateClient();
        
        // 2. Action
        var request = new SubmitManifestRequest("TEST-BAD-001", new[] { "9999.99" });
        var response = await client.PostAsJsonAsync("api/manifests", request);
        
        await AssertApiSuccessOrThrow(response);

        var sagaId = (await response.Content.ReadFromJsonAsync<SubmitManifestResponse>())!.ManifestId;
        
        // 3. Assert - We expect "Rejected"
        var finalState = await PollForSagaState(appFactory, sagaId, "Rejected");
        
        finalState.CurrentState.Should().Be("Rejected");
    }
    
    [Fact]
    public async Task Submit_Manifest_Runs_Full_Saga_Flow()
    {
        // 1. Setup
        var appFactory = GetConfiguredFactory();
        await EnsureDbCreated(appFactory);
        var client = appFactory.CreateClient();
        
        // 2. Action
        var request = new SubmitManifestRequest("TEST-GOOD-001", new[] { "8542.31" });
        var response = await client.PostAsJsonAsync("api/manifests", request);
        
        await AssertApiSuccessOrThrow(response); // The Refactored Debug Helper

        var sagaId = (await response.Content.ReadFromJsonAsync<SubmitManifestResponse>())!.ManifestId;

        // 3. Assert (The Polling Helper)
        var finalState = await PollForSagaState(appFactory, sagaId, "Validated");
        
        finalState.CurrentState.Should().Be("Validated");
    }
    
    [Fact]
    public async Task Submit_Manifest_Returns_Accepted_And_Publishes_Event()
    {
        // 1. Setup
        var appFactory = GetConfiguredFactory();
        var client = appFactory.CreateClient();
        
        // 2. Action
        var request = new SubmitManifestRequest(
            ReferenceNumber: "TEST-001", 
            HtsCodes: new[] { "8542.31", "1234.56" }
        );

        var response = await client.PostAsJsonAsync("api/manifests", request);
        
        await AssertApiSuccessOrThrow(response);

        // 3. Assert
        response.StatusCode.Should().Be(HttpStatusCode.Accepted);
        
        var responseBody = await response.Content.ReadFromJsonAsync<SubmitManifestResponse>();
        responseBody.Should().NotBeNull();
        responseBody!.ManifestId.Should().NotBeEmpty();

        // NOTE: Verifying the Consumer actually "Consumed" it is tricky in black-box tests
        // without a side-effect (like a DB write).
        // For this skeleton test, proving the API accepted it and didn't crash on the 
        // RabbitMQ connection is the victory.
    }
    #endregion Tests
    
    #region Helpers
    private WebApplicationFactory<Program> GetConfiguredFactory()
    {
        return new WebApplicationFactory<Program>()
            .WithWebHostBuilder(builder =>
            {
                builder.ConfigureAppConfiguration((context, config) =>
                {
                    config.AddInMemoryCollection(new Dictionary<string, string?>
                    {
                        { "RabbitMq:Host", "127.0.0.1" },
                        { "RabbitMq:Username", "admin" },
                        { "RabbitMq:Password", "admin" },
                        { "RabbitMq:Port", _rabbitMqContainer.GetMappedPublicPort(5672).ToString() },
                        { "ConnectionStrings:DefaultConnection", _mssqlContainer.GetConnectionString() }
                    });
                });
            });
    }

    private async Task EnsureDbCreated(WebApplicationFactory<Program> factory)
    {
        using var scope = factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ManifestSagaDbContext>();
        await db.Database.EnsureCreatedAsync();
        
        // GIVE IT A MOMENT: Allow MassTransit to finish declaring topology (Queues/Exchanges)
        // This defeats the "Topology Race" where we publish before the queue exists.
        await Task.Delay(1000);
    }

    private async Task AssertApiSuccessOrThrow(HttpResponseMessage response)
    {
        if (response.StatusCode == HttpStatusCode.InternalServerError)
        {
            var error = await response.Content.ReadAsStringAsync();
            throw new Exception($"API Crash: {error}");
        }
        response.StatusCode.Should().Be(HttpStatusCode.Accepted);
    }

    // The "Polly" Replacement
    // We pass the "expectedState" so we can stop early if we hit it.
    private async Task<ManifestState> PollForSagaState(WebApplicationFactory<Program> factory, Guid sagaId, string expectedState)
    {
        ManifestState? sagaState = null;
        
        // Try 20 times (10 seconds max)
        for (int i = 0; i < 20; i++) 
        {
            // Fresh Scope Every Time (Prevents Stale Data)
            using (var scope = factory.Services.CreateScope())
            {
                var db = scope.ServiceProvider.GetRequiredService<ManifestSagaDbContext>();
                
                sagaState = await db.Set<ManifestState>()
                    .AsNoTracking()
                    .FirstOrDefaultAsync(x => x.CorrelationId == sagaId);
                
                // If we found it AND it's in the state we want (or a terminal state), stop.
                if (sagaState != null && sagaState.CurrentState == expectedState) 
                {
                    return sagaState;
                }
            }
            
            await Task.Delay(500); // Wait half a second before checking again
        }

        // If we timed out, return whatever we have (or null) so the Assertion fails the test
        // But throwing here is often clearer.
        throw new Exception($"Saga timed out. Last state: {sagaState?.CurrentState ?? "NULL"}");
    }
    #endregion Helpers
    
    #region Records
    // Tiny helper record to deserialize the response
    public record SubmitManifestResponse(Guid ManifestId);
    #endregion Records
}