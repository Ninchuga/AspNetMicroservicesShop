﻿using Microsoft.AspNetCore.Builder;
using Shopping.Common.Logging;

namespace Discount.Grpc.Extensions
{
    internal static class ConfigurationExtensions
    {
        public static void AddCorrelationLoggingMiddleware(this IApplicationBuilder app)
        {
            app.UseMiddleware<CorrelationIdLoggingMiddleware>();
        }
    }
}
