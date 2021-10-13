using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace Shopping.HealthChecks
{
    public static class HealthCheckResponses
    {
        public static Task WriteJsonResponse(HttpContext context, HealthReport report)
        {
            context.Response.ContentType = "application/json; charset=utf-8";

            var options = new JsonWriterOptions { Indented = true };

            using var writer = new Utf8JsonWriter(context.Response.BodyWriter, options);

            writer.WriteStartObject();
            writer.WriteString("status", report.Status.ToString());

            // Health check may return health from multiple sources and we need to check them all
            if(report.Entries.Count > 0)
            {
                writer.WriteStartArray("results");

                foreach (var (key, value) in report.Entries)
                {
                    writer.WriteStartObject();
                    writer.WriteString("key", key);
                    writer.WriteString("status", value.Status.ToString());
                    writer.WriteString("description", value.Description);
                    writer.WriteStartArray("data");
                    foreach (var (dataKey, dataValue) in value.Data.Where(d => d.Value is object))
                    {
                        writer.WriteStartObject();
                        writer.WritePropertyName(dataKey);
                        JsonSerializer.Serialize(writer, dataValue, dataValue.GetType());
                        writer.WriteEndObject();
                    }
                    writer.WriteEndArray();
                    writer.WriteEndObject();
                }

                writer.WriteEndArray();
            }

            writer.WriteEndObject();

            return Task.CompletedTask;
        }
    }
}
