using AIdentities.UI;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

//logger
builder.Host.UseSerilog((context, configuration) =>
{
   configuration
      .ReadFrom.Configuration(context.Configuration)
      .Enrich.FromLogContext();
});

builder.Services.AddAIdentitiesServices(builder.Environment);

if (builder.Environment.IsDevelopment())
{
   builder.WebHost.UseWebRoot("wwwroot").UseStaticWebAssets();
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

app.Run();
