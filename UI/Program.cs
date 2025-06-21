using Application.Services;
using Domain.ConfigurationOptions;
using Domain.ServicesAbstraction;
using Infrastructure.Email;
using Microsoft.EntityFrameworkCore;
using UI.Components;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

builder.Services.AddScoped<IEmailService, ImapEmailService>();
builder.Services.AddScoped<IOrderDataProvider, OrderDataProvider>();
builder.Services.AddHttpClient<IEmailOrderParser, GptEmailOrderParser>();

builder.Services.Configure<EmailOptions>(builder.Configuration.GetSection("EmailOptions"));
builder.Services.Configure<OpenAIOptions>(builder.Configuration.GetSection("OpenAIOptions"));
builder.Logging.AddConsole();


builder.Services.AddDbContext<EmailDbContext>(options =>
options.UseMySql(
        builder.Configuration.GetConnectionString("DefaultConnection"),
        new MySqlServerVersion(new Version(8, 0, 0))
    ));

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    app.UseHsts();
}

app.UseHttpsRedirection();

app.UseStaticFiles();
app.UseAntiforgery();

app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<EmailDbContext>();

    var retries = 5;
    while (retries > 0)
    {
        try
        {
            db.Database.Migrate();
            break;
        }
        catch (Exception ex)
        {
            retries--;
            if (retries == 0) throw;
            Console.WriteLine($"Migracja nieudana, próba ponowienia za 5 sekund: {ex.Message}");
            Thread.Sleep(5000);
        }
    }
}

app.Run();
