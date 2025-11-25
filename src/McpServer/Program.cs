using Application.Interfaces;
using Application.Mappings;
using Application.Services;
using Application.Validators;
using FluentValidation;
using Infrastructure.Calendar;
using McpServer;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Serilog;

// Configurar Serilog
Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .WriteTo.File("logs/log-.txt", rollingInterval: RollingInterval.Day)
    .CreateLogger();

try
{
    var host = Host.CreateDefaultBuilder(args)
        .UseSerilog()
        .ConfigureServices((context, services) =>
        {
            var configuration = context.Configuration;

            // Configurar Google Calendar Service
            services.AddSingleton<ICalendarService, GoogleCalendarService>();

            // Configurar Services
            services.AddScoped<IAgendamentoService, AgendamentoService>();

            // Configurar AutoMapper
            services.AddAutoMapper(typeof(MappingProfile));

            // Configurar FluentValidation
            services.AddValidatorsFromAssemblyContaining<CriarAgendamentoDtoValidator>();
            services.AddValidatorsFromAssemblyContaining<AtualizarAgendamentoDtoValidator>();

            // Registrar servidor MCP
            services.AddSingleton<McpServer.McpServer>();
        })
        .Build();

    var server = host.Services.GetRequiredService<McpServer.McpServer>();
    await server.RunAsync();
}
catch (Exception ex)
{
    Log.Fatal(ex, "Erro fatal ao iniciar servidor MCP");
    throw;
}
finally
{
    Log.CloseAndFlush();
}

