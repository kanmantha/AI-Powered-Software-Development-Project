using Microsoft.EntityFrameworkCore;
using AIPoweredSoftwareDevelopment.Data;
using AIPoweredSoftwareDevelopment.Services;
using AIPoweredSoftwareDevelopment.Filters;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews(options =>
{
    // Register global exception filter
    options.Filters.Add<GlobalExceptionFilter>();
});

// Add DbContext with SQLite
// Connection string is read from appsettings.json
// SQLite is used for simplicity and portability
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection")));

// Add HttpClients for AI services
builder.Services.AddHttpClient("Claude", client =>
{
    client.BaseAddress = new Uri("https://api.anthropic.com/v1/");
    client.DefaultRequestHeaders.Add("x-api-key", builder.Configuration["Claude:ApiKey"]);
    client.DefaultRequestHeaders.Add("anthropic-version", "2023-06-01");
});

builder.Services.AddHttpClient("GitHub", client =>
{
    client.BaseAddress = new Uri("https://api.github.com/");
    client.DefaultRequestHeaders.Add("Authorization", $"token {builder.Configuration["GitHub:CopilotToken"]}");
    client.DefaultRequestHeaders.Add("User-Agent", "AIPoweredSoftwareDevelopment");
    client.DefaultRequestHeaders.Add("Accept", "application/vnd.github.copilot-preview+json");
});

builder.Services.AddHttpClient("Cursor", client =>
{
    client.BaseAddress = new Uri("https://api.cursor.sh/v1/");
    client.DefaultRequestHeaders.Add("Authorization", $"Bearer {builder.Configuration["Cursor:ApiKey"]}");
});

builder.Services.AddHttpClient("Azure", client =>
{
    var org = builder.Configuration["AzureDevOps:Organization"];
    client.BaseAddress = new Uri($"https://dev.azure.com/{org}/");
    var patToken = builder.Configuration["AzureDevOps:PatToken"];
    var authToken = Convert.ToBase64String(System.Text.Encoding.ASCII.GetBytes($":{patToken}"));
    client.DefaultRequestHeaders.Add("Authorization", $"Basic {authToken}");
});

// Add AI service factory
builder.Services.AddSingleton<AIServiceFactory>();

// Add error logging service
builder.Services.AddScoped<ErrorLoggingService>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseRouting();

app.UseAuthorization();

app.MapStaticAssets();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();

// Seed the database
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try
    {
        var context = services.GetRequiredService<ApplicationDbContext>();
        await DbInitializer.Initialize(context);
    }
    catch (Exception ex)
    {
        var logger = services.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "An error occurred while seeding the database.");
    }
}

app.Run();
