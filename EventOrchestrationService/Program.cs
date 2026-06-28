using EventOrchestrationService;
using EventOrchestrationService.BackgroundServices;
using EventOrchestrationService.Data;
using EventOrchestrationService.Entities;
using EventOrchestrationService.Services.Implementations;
using EventOrchestrationService.Services.Interfaces;
using FluentValidation;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddScoped<IEventService, EventService>();
builder.Services.AddScoped<IBookingService, BookingService>();

var isDevelopment = builder.Environment.IsDevelopment();

var connectionString = builder.Configuration.GetConnectionString("PostgresqlConnection")
                       ?? throw new InvalidOperationException("Connection string 'PostgresqlConnection' not found.");

builder.Services.AddDbContext<AppDbContext>(options =>
{
    options.UseNpgsql(connectionString);

    if (isDevelopment)
        options.LogTo(Console.WriteLine, LogLevel.Information)
            .EnableDetailedErrors()
            .EnableSensitiveDataLogging();
});

builder.Services.AddControllers();
builder.Services.AddHostedService<BookingBackgroundService>();
builder.Services.AddValidatorsFromAssemblyContaining<Event.EventValidator>();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    context.Database.EnsureCreated();
    DbInitializer.Initialize(context);
}

app.UseMiddleware<GlobalExceptionHandlingMiddleware>();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();

app.Run();