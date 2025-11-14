using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using StaticWebForms;
using StaticWebForms.Services;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

// Configure logging
builder.Logging.SetMinimumLevel(LogLevel.Information);
builder.Logging.AddFilter("Microsoft", LogLevel.Warning);
builder.Logging.AddFilter("System", LogLevel.Warning);
builder.Logging.AddFilter("StaticWebForms", LogLevel.Debug);

// Register services
builder.Services.AddScoped(sp => new HttpClient { BaseAddress = new Uri(builder.HostEnvironment.BaseAddress) });
builder.Services.AddScoped<IModuleService, ModuleService>();
builder.Services.AddScoped<IDocumentService, DocumentService>();
builder.Services.AddScoped<IFormSubmissionService, FormSubmissionService>();
builder.Services.AddScoped<ILoggingService, LoggingService>();
builder.Services.AddSingleton<IErrorService, ErrorService>();

var host = builder.Build();

// Log application startup
var logger = host.Services.GetRequiredService<ILogger<Program>>();
logger.LogInformation("Application starting at {BaseAddress}", builder.HostEnvironment.BaseAddress);
logger.LogInformation("Environment: {Environment}", builder.HostEnvironment.Environment);

await host.RunAsync();

