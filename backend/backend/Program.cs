//----------------------------------------
// .Net Core WebApi project - Bereinigt für Build-Stabilität
//----------------------------------------
using backend;
using Microsoft.EntityFrameworkCore;

string corsKey = "_myCorsKey";

var builder = WebApplication.CreateBuilder(new WebApplicationOptions
{
    Args = args,
    ContentRootPath = AppContext.BaseDirectory
});

builder.Configuration.Sources.Clear();
builder.Configuration.AddEnvironmentVariables();

#region -------------------------------------------- ConfigureServices

builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.PropertyNameCaseInsensitive = true;
    });

builder.Services.AddScoped<PasswordsService>();
builder.Services.AddScoped<AuthService>();
builder.Services.AddAuthorization();

// CORS-Policy
builder.Services.AddCors(options => options.AddPolicy(
  corsKey,
  x => x.WithOrigins("https://password-manager-sigma-lemon.vercel.app")
        .AllowAnyMethod()
        .AllowAnyHeader()
        .AllowCredentials()
));

builder.Services.AddLogging();

string? connectionString = builder.Configuration.GetConnectionString("Passwords")!;
Console.ForegroundColor = ConsoleColor.Cyan;
Console.WriteLine($"++++ ConnectionString: {connectionString}");
Console.ResetColor();

builder.Services.AddDbContext<DataContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("Passwords")));

#endregion

var app = builder.Build();

#region -------------------------------------------- Middleware pipeline

// WICHTIG: Die CORS-Middleware muss ganz am Anfang der Pipeline stehen!
app.UseCors(corsKey);

if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
}

#endregion

app.MapGet("/", () => "API is running");
app.MapControllers();

Console.WriteLine($"Ready for clients at {DateTime.Now:HH:mm:ss} ...");

// Automatische Migration
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    var context = services.GetRequiredService<DataContext>();
    context.Database.Migrate();
    Console.WriteLine("++++ Migrationen erfolgreich auf Datenbank angewendet!");
}

app.Run();