using Serilog;
using Serilog.Debugging;
using OrderService.Services;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using OpenTelemetry.Metrics;

var builder = WebApplication.CreateBuilder(args);

SelfLog.Enable(Console.Error);

var seqUrl = builder.Configuration["Serilog:SeqServerUrl"];

Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(builder.Configuration)
    .WriteTo.Console()
    .WriteTo.Seq(seqUrl!)
    .CreateLogger();

builder.Host.UseSerilog();

builder.Services.AddOpenTelemetry()
    .WithTracing(tracing =>
    {
        tracing
            .SetSampler(new AlwaysOnSampler())
            .AddSource("OrderService")
            .SetResourceBuilder(
                ResourceBuilder.CreateDefault()
                    .AddService("OrderService"))

            .AddAspNetCoreInstrumentation(options =>
            {
                options.RecordException = true;
            })

            .AddHttpClientInstrumentation()

            .AddOtlpExporter(options =>
            {
                options.Endpoint =
                    new Uri("http://seq:5341/ingest/otlp/v1/traces");

                options.Protocol =
                    OpenTelemetry.Exporter.OtlpExportProtocol.HttpProtobuf;
            });
    })
    
     .WithMetrics(metrics =>
      {
          metrics
              .SetResourceBuilder(
                  ResourceBuilder.CreateDefault()
                      .AddService("OrderService"))

              .AddAspNetCoreInstrumentation()

              .AddHttpClientInstrumentation()

              .AddRuntimeInstrumentation()

              .AddPrometheusExporter();
      });

builder.Services.AddSingleton<RabbitMqProducer>();

builder.Services.AddControllers();

builder.Services.AddHealthChecks();

builder.Services.AddEndpointsApiExplorer();

builder.Services.AddSwaggerGen();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.MapHealthChecks("/health");

app.MapPrometheusScrapingEndpoint();

app.MapGet("/ping", () => "pong");

app.Run();