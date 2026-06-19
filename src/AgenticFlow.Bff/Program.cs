using AgenticFlow.Application.Abstractions;
using AgenticFlow.Bff.Hubs;
using AgenticFlow.Bff.Infrastructure;
using AgenticFlow.Persistence;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddSignalR();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddPersistence(builder.Configuration);
builder.Services.AddSingleton<IEventBus, InMemoryEventBus>();

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
