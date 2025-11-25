using Application.Interfaces;
using Application.Mappings;
using Application.Services;
using Application.Validators;
using FluentValidation;
using Infrastructure.Calendar;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

// Configurar Serilog
/*Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(builder.Configuration)
    .Enrich.FromLogContext()
    .WriteTo.Console()
    .WriteTo.File("logs/log-.txt", rollingInterval: RollingInterval.Day)
    .CreateLogger();

builder.Host.UseSerilog();

// Add services to the container
builder.Services.AddControllers();

// Configurar Swagger
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo
    {
        Title = "API de Agendamentos",
        Version = "v1",
        Description = "API REST para gerenciamento de agendamentos usando Clean Architecture"
    });
});
*/
// Configurar Google Calendar Service
builder.Services.AddSingleton<ICalendarService, GoogleCalendarService>();

// Configurar Services
builder.Services.AddScoped<IAgendamentoService, AgendamentoService>();

// Configurar AutoMapper
builder.Services.AddAutoMapper(typeof(MappingProfile));

// Configurar FluentValidation
builder.Services.AddValidatorsFromAssemblyContaining<CriarAgendamentoDtoValidator>();
builder.Services.AddValidatorsFromAssemblyContaining<AtualizarAgendamentoDtoValidator>();

var app = builder.Build();

// Configurar o pipeline HTTP
/*if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}



// Middleware de tratamento de exceções
app.UseMiddleware<ExceptionHandlingMiddleware>();*/
app.UseHttpsRedirection();

//app.UseAuthorization();

//app.MapControllers();

app.Run();
