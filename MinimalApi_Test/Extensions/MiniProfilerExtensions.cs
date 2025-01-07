using System.Data.Common;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.Caching.Memory;
using MinimalApi_Test.Context;
using StackExchange.Profiling;
using StackExchange.Profiling.Storage;

public static class MiniProfilerExtensions
{
    public static void AddCustomMiniProfiler(this WebApplicationBuilder builder)
    {
        // Add Memory Cache
        builder.Services.AddMemoryCache();

        // Add MiniProfiler services
        builder.Services.AddMiniProfiler(options =>
        {
            // Basic Options
            options.RouteBasePath = "/profiler";

            // Change Theme Color
            options.ColorScheme = ColorScheme.Dark;
            
            options.SqlFormatter = new StackExchange.Profiling.SqlFormatters.SqlServerFormatter();
            
            // Disable authorization temporarily for testing
            options.ResultsAuthorize = _ => true;
            
            // Enable for all paths during testing
            options.ShouldProfile = _ => true;
            
            // Storage settings
            var serviceProvider = builder.Services.BuildServiceProvider();
            var memoryCache = serviceProvider.GetRequiredService<IMemoryCache>();
            options.Storage = new MemoryCacheStorage(memoryCache, TimeSpan.FromMinutes(30));
            
            // // Configure storage (Optional - In-Memory by default)
            // options.Storage = new SqlServerStorage(builder.Configuration["ConnectionStrings:SqlConnectionString"]);
            
            // PopupRenderPosition
            options.PopupRenderPosition = RenderPosition.BottomRight;
            options.PopupShowTimeWithChildren = true;
            
            // Database profiling settings
            options.EnableServerTimingHeader = true;
            options.TrackConnectionOpenClose = true;
            
            // Verbose output
            options.EnableDebugMode = true;
            options.EnableMvcFilterProfiling = true;
            options.EnableMvcViewProfiling = true;
        }).AddEntityFramework();
    }

    // Extension method for DbContext configuration
    public static void AddMiniProfilerToDbContext(this DbContextOptionsBuilder options)
    {
        // Add EF Core interceptor for MiniProfiler
        options.AddInterceptors(new ProfilingDbCommandInterceptor());
    }

    public static void UseCustomMiniProfiler(this WebApplication app)
    {
        app.UseMiniProfiler();

        // Add a test endpoint for database profiling
        app.MapGet("/test-db-profiler", async (AppDbContext dbContext) =>
        {
            var profiler = MiniProfiler.Current;
            
            using (profiler.Step("Database Operations"))
            {
                // Example database operations
                using (profiler.Step("Simple Query"))
                {
                    var users = await dbContext.Users
                        .AsNoTracking()
                        .Take(5)
                        .ToListAsync();
                }

                using (profiler.Step("Complex Query"))
                {
                    var result = await dbContext.Users
                        .AsNoTracking()
                        .Where(u => !u.IsDelete)
                        .OrderByDescending(u => u.CreatedAt)
                        .Take(10)
                        .Select(u => new { u.Id, u.Username, u.CreatedAt })
                        .ToListAsync();
                }
            }

            return Results.Ok(new { message = "Database profiling test complete" });
        });

        // Profile all requests
        app.Use(async (context, next) =>
        {
            var profiler = MiniProfiler.Current;
            using (profiler.Step($"Request: {context.Request.Method} {context.Request.Path}"))
            {
                await next(context);
            }
        });
    }
}

// Custom DbCommandInterceptor for profiling
public class ProfilingDbCommandInterceptor : DbCommandInterceptor
{
    public override ValueTask<InterceptionResult<DbDataReader>> ReaderExecutingAsync(
        DbCommand command, 
        CommandEventData eventData, 
        InterceptionResult<DbDataReader> result, 
        CancellationToken cancellationToken = default)
    {
        var profiler = MiniProfiler.Current;
        if (profiler != null)
        {
            profiler.Step($"SQL: {command.CommandText}");
        }
        return base.ReaderExecutingAsync(command, eventData, result, cancellationToken);
    }
}