using FloByte.Application;
using FloByte.Infrastructure;
using FloByte.WebUI.Components;
using FloByte.WebUI.Services;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Serilog;

var builder = WebAssemblyHostBuilder.CreateDefault(args);

// Add root component
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

// Add Serilog
builder.Logging.AddSerilog(new LoggerConfiguration()
    .MinimumLevel.Information()
    .WriteTo.BrowserConsole()
    .CreateLogger());

// Add services
builder.Services.AddScoped(sp => new HttpClient { BaseAddress = new Uri(builder.HostEnvironment.BaseAddress) });

// Add application layer
builder.Services.AddApplication();

// Add infrastructure layer
builder.Services.AddInfrastructure(builder.Configuration);
builder.Services.AddIdentityServices(builder.Configuration);

// Add UI services
builder.Services.AddScoped<ILayoutService, LayoutService>();
builder.Services.AddScoped<IThemeService, ThemeService>();
builder.Services.AddScoped<IDialogService, DialogService>();
builder.Services.AddScoped<INotificationService, NotificationService>();

// Add Blazor services
builder.Services.AddMudBlazor();
builder.Services.AddBlazorMonaco();

// Add state management
builder.Services.AddScoped<IProjectState, ProjectState>();
builder.Services.AddScoped<IWorkflowState, WorkflowState>();
builder.Services.AddScoped<ICodeEditorState, CodeEditorState>();

var app = builder.Build();

await app.RunAsync();
