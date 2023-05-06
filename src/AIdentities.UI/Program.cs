using AIdentities.UI;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Options;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

//logger
builder.Host.UseSerilog((context, configuration) =>
{
   configuration
      .ReadFrom.Configuration(context.Configuration)
      .Enrich.FromLogContext();
});

Microsoft.Extensions.Logging.ILogger startupLogger;

if (builder.Environment.IsDevelopment())
{
   builder.WebHost.UseWebRoot("wwwroot").UseStaticWebAssets();
}

try
{
   builder.Services.AddAIdentitiesServices(builder.Environment, out startupLogger, out IFileProvider pluginStaticWebProvider);

   // Register plugins static asset resolver
   builder.WebHost.UseWebRoot("wwwroot").UseStaticPluginWebAssets(pluginStaticWebProvider);
}
catch (OptionsValidationException)
{
   return;
}

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
   app.UseExceptionHandler("/Error");
   // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
   app.UseHsts();
}

app.UseHttpsRedirection();

app.UseStaticFiles();

app.UseRouting();

app.MapBlazorHub();
app.MapFallbackToPage("/_Host");

try
{
   app.Run();
}
catch (OptionsValidationException ex)
{
   startupLogger.LogError("Failed to validate options");
   foreach (var failure in ex.Failures)
   {
      startupLogger.LogError("Validation failure: {Failure}", failure);
   }
}
