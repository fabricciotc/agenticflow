using AgenticFlow.Application.Actions;
using AgenticFlow.Application.Abstractions;
using AgenticFlow.Application.Memory;
using AgenticFlow.Application.Roles;
using AgenticFlow.Application.Runners;
using AgenticFlow.Application.Services;
using AgenticFlow.Application.Skills;
using AgenticFlow.Bff.Hubs;
using AgenticFlow.Bff.Infrastructure;
using AgenticFlow.Persistence;
using EnvironmentImpl = AgenticFlow.Application.Orchestration.Environment;
using OrchestratorImpl = AgenticFlow.Application.Orchestration.Orchestrator;
using PlanEngineImpl = AgenticFlow.Application.Orchestration.PlanEngine;

var builder = WebApplication.CreateBuilder(args);
builder.WebHost.UseWebRoot("Frontend");

builder.Services.AddControllers();
builder.Services.AddSignalR();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddPersistence(builder.Configuration);
builder.Services.AddSingleton<IEventBus, SignalREventBus>();

builder.Services.AddSingleton<IMemoryStore, MemoryStore>();
builder.Services.AddSingleton<IEnvironment, EnvironmentImpl>();
builder.Services.AddSingleton<IPlanEngine, PlanEngineImpl>();
builder.Services.AddSingleton<IOrchestrator, OrchestratorImpl>();
builder.Services.AddSingleton<IBackendRegistry, BackendRegistry>();
builder.Services.AddSingleton<ISkillRegistry, SkillRegistry>();
builder.Services.AddSingleton<ITicketService, TicketService>();

builder.Services.AddSingleton<IAIRunner, KimiCliRunner>();
builder.Services.AddSingleton<IAIRunner, ClaudeCodeRunner>();
builder.Services.AddSingleton<IAIRunner, OpenAiApiRunner>();

builder.Services.AddScoped<ResearchAction>();
builder.Services.AddScoped<ImplementAction>();
builder.Services.AddScoped<CorrectionAction>();
builder.Services.AddScoped<ReviewAction>();
builder.Services.AddScoped<PMResearchRole>();
builder.Services.AddScoped<EngineerRole>();
builder.Services.AddScoped<QARole>();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseDefaultFiles();
app.UseStaticFiles();
app.UseRouting();
app.MapControllers();
app.MapHub<DashboardHub>("/hub");

app.Run();
