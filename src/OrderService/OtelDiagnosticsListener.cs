using System;
using System.Diagnostics.Tracing;

public sealed class OtelDiagnosticsListener : EventListener
{
    protected override void OnEventSourceCreated(EventSource eventSource)
    {
        if (eventSource.Name.StartsWith("OpenTelemetry"))
        {
            EnableEvents(eventSource, EventLevel.LogAlways);
            Console.WriteLine($"[OTEL-DIAG] Listening to {eventSource.Name}");
        }
    }

    protected override void OnEventWritten(EventWrittenEventArgs eventData)
    {
        try
        {
            Console.WriteLine(
                $"[OTEL-DIAG] {eventData.EventSource.Name} - {eventData.EventName}");
        }
        catch
        {
        }
    }
}