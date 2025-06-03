using System.Data;
using Ical.Net.Serialization;
using ICalAnonymizer;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Scalar.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

builder.Configuration.AddJsonFile("appsettings.User.json", true);

builder.Services.AddHttpClient();
builder.Services.AddOpenApi();
builder.Services.AddTransient<CalendarService>();
builder.Services.AddSingleton<ConfigService>();

builder.Services.Configure<AppOptions>(builder.Configuration);

var app = builder.Build();

if(string.IsNullOrWhiteSpace(app.Services.GetRequiredService<IOptions<AppOptions>>().Value.ApiKey)) throw new EvaluateException("Missing API key");

app.MapOpenApi();
app.MapScalarApiReference();

app.MapGet("/ical", async ([FromQuery]string key, CalendarService service, CancellationToken ct) => Results.Content(new CalendarSerializer(await service.GetCalendar(key,ct)).SerializeToString(),"text/calendar")).WithName("ical");

app.MapPut("/config/{key}",
    async (string key, [FromHeader(Name = "x-api-key")] string apiKey,
        [FromBody] CalendarConfig calendars, ConfigService service, CancellationToken ct) =>
    {
        await service.UpdateConfig(key, apiKey, calendars, ct);
        return Results.CreatedAtRoute("config", new { key,});
    });

app.MapDelete("/config/{key}",
    async (string key, [FromHeader(Name = "x-api-key")] string apiKey,
        ConfigService service, CancellationToken ct) =>
    {
        await service.DeleteConfig(key, apiKey, ct);
        return Results.NoContent();
    });

app.MapGet("/config/{key}",
    async (string key, [FromHeader(Name = "x-api-key")] string apiKey, ConfigService service, CancellationToken ct) =>
    {
        var config = await service.GetConfig(key, apiKey, ct);
        return config == null ? Results.NotFound() : Results.Ok(config);
    });

app.MapGet("/config",
    async ([FromHeader(Name = "x-api-key")] string apiKey, ConfigService service, CancellationToken ct) =>
    {
        var configs = await service.GetConfigIds(apiKey, ct);
        return Results.Ok(new {configs});
    });

app.UseHttpsRedirection();

app.Run();
