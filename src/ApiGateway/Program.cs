using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(builder.Configuration)
    .WriteTo.Console()
    .WriteTo.Seq(builder.Configuration["Serilog:SeqServerUrl"]!)
    .CreateLogger();

builder.Host.UseSerilog();
builder.Services.AddOpenTelemetry()
    .WithTracing(tracing =>
    {
        tracing
            .SetResourceBuilder(
                ResourceBuilder.CreateDefault()
                    .AddService("ApiGateway"))

            .AddAspNetCoreInstrumentation()

            .AddHttpClientInstrumentation()

            .AddOtlpExporter(options =>
            {
                options.Endpoint = new Uri("http://seq:5341/ingest/otlp/v1/traces");
                options.Protocol = OpenTelemetry.Exporter.OtlpExportProtocol.HttpProtobuf;
            });
    })
    .WithMetrics(metrics =>
    {
        metrics
            .SetResourceBuilder(
                ResourceBuilder.CreateDefault()
                    .AddService("ApiGateway"))

            .AddAspNetCoreInstrumentation()

            .AddHttpClientInstrumentation()

            .AddRuntimeInstrumentation()

            .AddPrometheusExporter();
    });

builder.Services
    .AddReverseProxy()
    .LoadFromConfig(builder.Configuration.GetSection("ReverseProxy"));

builder.Services.AddHealthChecks();

var app = builder.Build();

app.UseSerilogRequestLogging();

app.MapReverseProxy();

app.MapHealthChecks("/health");

app.MapPrometheusScrapingEndpoint();

app.Run();