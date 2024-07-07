using System.Net;
using System.Net.Http;
using System.Reflection.Metadata;
using System.Text.RegularExpressions;

using Dgmjr.AspNetCore.ReverseProxy.Middleware;
using Dgmjr.AspNetCore.ReverseProxy.Models;

using Microsoft.ApplicationInsights;
using Microsoft.ApplicationInsights.AspNetCore.DiagnosticListeners;
using Microsoft.ApplicationInsights.AspNetCore.Extensions;
using Microsoft.ApplicationInsights.AspNetCore.Logging;
using Microsoft.ApplicationInsights.AspNetCore.TelemetryInitializers;
using Microsoft.ApplicationInsights.DependencyCollector;
using Microsoft.ApplicationInsights.Extensibility;
using Microsoft.ApplicationInsights.W3C;
using Microsoft.AspNetCore.ResponseCompression;
using Microsoft.Extensions.Options;

using Serilog;
// Other required namespaces
using static System.IO.Path;

using Constants = Dgmjr.AspNetCore.ReverseProxy.Constants;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle

var appSettingsBasePath = Join(
    GetDirectoryName(typeof(ReverseProxyMiddleware).Assembly.Location),
    Constants.Configuration
);
var appSettingsJsonFiles = Directory.GetFiles(appSettingsBasePath, $"*{Constants._Json}");
var currentEnvironmentAppSettingsJsonFileRegexString =
    $".*\\.{builder.Environment.EnvironmentName}{Constants._Json}";
var anyEnvironmentAppSettingsJsonFileRegexString = @$"^[^.\s]+{Constants._Json}";

foreach (var appSettingsJsonFile in appSettingsJsonFiles)
{
    if (
        Regex.IsMatch(appSettingsJsonFile, currentEnvironmentAppSettingsJsonFileRegexString)
        || Regex.IsMatch(appSettingsJsonFile, anyEnvironmentAppSettingsJsonFileRegexString)
    )
    {
        builder.Configuration.AddJsonFile(
            appSettingsJsonFile,
            optional: false,
            reloadOnChange: false
        );
    }
}

builder.Configuration.AddSubstitution();

// await builder.Configuration.AddKeyPerJsonFileAsync(
//     Constants.Configuration,
//     Constants.AppSettings,
//     false,
//     false,
//     StaticLogger.GetLogger()
// );

builder.Services.AddApplicationInsightsTelemetry(
    config => builder.Configuration.GetSection(Constants.ApplicationInsights).Bind(config)
);

// Automatically track dependencies
builder.Services.ConfigureTelemetryModule<DependencyTrackingTelemetryModule>(
    (module, o) =>
    {
        builder.Configuration
            .GetSection($"{Constants.ApplicationInsights}:{Constants.DependencyTracking}")
            .Bind(module);
        builder.Configuration.GetSection(Constants.ApplicationInsights).Bind(o);
    }
);

builder.Host.UseSerilog(
    (hostingContext, loggerConfiguration) =>
    {
        loggerConfiguration.ReadFrom
            .Configuration(hostingContext.Configuration)
            .Enrich.FromLogContext()
            .WriteTo.Console();
    }
);

// builder.Services.AddControllers();
builder.Services
    .AddHttpClient(typeof(ReverseProxyMiddleware).FullName)
    .AddDefaultLogger()
    .ConfigurePrimaryHttpMessageHandler(
        () =>
            new HttpClientHandler
            {
                ServerCertificateCustomValidationCallback =
                    HttpClientHandler.DangerousAcceptAnyServerCertificateValidator,
                AutomaticDecompression = DecompressionMethods.All
            }
    );

builder.Services.AddResponseCompression(options =>
{
    options.EnableForHttps = true; // Enable for HTTPS (optional, change as required)
    options.Providers.Add<GzipCompressionProvider>(); // Add gzip provider, handle other as needed
});

builder.Services.Configure<GzipCompressionProviderOptions>(options =>
{
    options.Level = System.IO.Compression.CompressionLevel.Fastest; // Set according to your needs
});

builder.Services.Configure<ReverseProxyConfig>(
    builder.Configuration.GetSection(nameof(ReverseProxyConfig))
);

var app = builder.Build();

// Configure the HTTP request pipeline.
// if (app.Environment.IsDevelopment())
// {
//     app.UseSwagger();
//     app.UseSwaggerUI();
// }

// Enable response compression
app.UseResponseCompression();

app.UseMiddleware<ReverseProxyMiddleware>();
app.UseHttpsRedirection();

await app.RunAsync();
