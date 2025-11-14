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
// HttpClient for static files (modules, templates) - with BaseAddress
builder.Services.AddScoped<StaticFilesHttpClient>(sp => 
    new StaticFilesHttpClient { BaseAddress = new Uri(builder.HostEnvironment.BaseAddress) });

// HttpClient for API calls to Raspberry Pi (without BaseAddress, will be set dynamically)
builder.Services.AddScoped<ApiHttpClient>(sp => new ApiHttpClient());

// Register services
builder.Services.AddScoped<IModuleService, ModuleService>();
builder.Services.AddScoped<IDocumentService, DocumentService>();
builder.Services.AddScoped<IFormSubmissionService, FormSubmissionService>();
builder.Services.AddScoped<IConfigurationService, ConfigurationService>();
builder.Services.AddScoped<IErrorReportingService, ErrorReportingService>();
builder.Services.AddScoped<ILoggingService, LoggingService>();
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IAdminService, AdminService>();
builder.Services.AddSingleton<IErrorService, ErrorService>();

var host = builder.Build();

// Log application startup
var logger = host.Services.GetRequiredService<ILogger<Program>>();
logger.LogInformation("Application starting at {BaseAddress}", builder.HostEnvironment.BaseAddress);
logger.LogInformation("Environment: {Environment}", builder.HostEnvironment.Environment);

await host.RunAsync();

