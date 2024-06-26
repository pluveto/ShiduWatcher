using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using ShiduWatcher.Controllers;

namespace ShiduWatcher.ShiduWatcher
{
    internal static class RestfulAPIHost
    {

        internal static IHostBuilder CreateHostBuilder(string[] args, int port, ProgramUsageService usageService) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureServices((context, services) =>
                {
                    services.AddSingleton(usageService);
                    services.AddControllers() // Some warning, but it works
                            .AddNewtonsoftJson(options =>
                            {
                                options.SerializerSettings.ContractResolver = new Newtonsoft.Json.Serialization.CamelCasePropertyNamesContractResolver();
                            })
                            .AddApplicationPart(typeof(ControlController).Assembly)
                            .AddApplicationPart(typeof(UsageReportController).Assembly)
                            .AddApplicationPart(typeof(IconController).Assembly)
                            .AddControllersAsServices();

                    // Add CORS services and configure the policy
                    services.AddCors(options =>
                    {
                        options.AddPolicy("AllowAll", builder =>
                        {
                            builder.AllowAnyOrigin()
                                   .AllowAnyMethod()
                                   .AllowAnyHeader();
                        });
                    });
                })
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder.Configure(app =>
                    {
                        app.UseRouting();

                        // Apply the CORS policy
                        app.UseCors("AllowAll");

                        // Custom middleware to print all registered routes
                        app.Use(async (context, next) =>
                        {
                            var endpointDataSource = context.RequestServices.GetRequiredService<EndpointDataSource>();
                            var routes = endpointDataSource.Endpoints.OfType<RouteEndpoint>();

                            foreach (var route in routes)
                            {
                                Console.WriteLine($"Route: {route.RoutePattern.RawText}");
                            }

                            await next.Invoke();
                        });

                        app.UseEndpoints(endpoints =>
                        {
                            endpoints.MapControllers();
                        });
                    });
                    webBuilder.UseUrls($"http://localhost:{port}");
                });
    }
}
