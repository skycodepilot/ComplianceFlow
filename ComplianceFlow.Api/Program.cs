using ComplianceFlow.Api.Features.Manifests.SubmitManifest.Saga;
using MassTransit;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddMassTransit(x =>
{
    x.AddConsumers(typeof(Program).Assembly);
    
    // 1. REGISTER THE STATE MACHINE
    // "Hey MassTransit, here is the Logic (Machine) and the Storage (State)."
    x.AddSagaStateMachine<ManifestStateMachine, ManifestState> ()
        .EntityFrameworkRepository(r =>
        {
            // 2. Configure SQL Server
            r.ExistingDbContext<ManifestSagaDbContext>();
            r.UseSqlServer(); // Locking strategy for SQL
        });

    x.UsingRabbitMq((context, cfg) =>
    {
        // Resolve Configuration explicitly from the Container
        // This guarantees we get the version that includes Testcontainer overrides.
        var config = context.GetRequiredService<IConfiguration>();
        
        var host = config["RabbitMq:Host"] ?? throw new InvalidOperationException("RabbitMQ Host is missing.");
        var port = config["RabbitMq:Port"]; 

        if (ushort.TryParse(port, out var portNumber))
        {
            cfg.Host(host, portNumber, "/", h =>
            {
                h.Username(config["RabbitMq:Username"]);
                h.Password(config["RabbitMq:Password"]);
            });
        }
        else 
        {
            cfg.Host(host, "/", h =>
            {
                h.Username(config["RabbitMq:Username"]);
                h.Password(config["RabbitMq:Password"]);
            });
        }

        // 3. AUTO-CONFIGURE ENDPOINTS
        // This is crucial. It creates the queues for the Saga automatically.
        cfg.ConfigureEndpoints(context);
    });
});

// 4. REGISTER THE DB CONTEXT (Standard EF Core work)
// We pull the connection string from config (which we will set in a moment).
builder.Services.AddDbContext<ManifestSagaDbContext>(options =>
{
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"));
});

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(); // This enables the UI at /swagger
}

app.UseHttpsRedirection();

app.UseCors(policy => policy
    .WithOrigins("http://localhost:4200")
    .AllowAnyMethod()
    .AllowAnyHeader());

app.MapControllers();

// 🛑 ADD THIS BLOCK TO CREATE THE DB TABLES
if (app.Environment.IsDevelopment())
{
    using (var scope = app.Services.CreateScope())
    {
        var db = scope.ServiceProvider.GetRequiredService<ManifestSagaDbContext>();
        // This creates the database and tables if they don't exist
        await db.Database.EnsureCreatedAsync();
    }
}

app.Run();

public partial class Program { }