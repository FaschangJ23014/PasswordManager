//----------------------------------------
// .Net Core WebApi project - Bereinigt für Build-Stabilität
//----------------------------------------
using backend;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication.JwtBearer; // HINZUGEFÜGT
using Microsoft.IdentityModel.Tokens;               // HINZUGEFÜGT
using System.Text;

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
builder.Services.AddScoped<UserSettingsService>();

// --- AUTHENTICATION HINZUGEFÜGT ---
builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(
            builder.Configuration["JWT:SecretKey"] ?? "DiesIstEinSehrLangerStandardKeyDerDefinitivMehrAls64ZeichenHatDamitEsNichtAbstuerzt1234567890")),
        ValidateIssuer = false,
        ValidateAudience = false
    };
});

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
// WICHTIG: Die Reihenfolge ist hier entscheidend!
app.UseCors(corsKey);
app.UseAuthentication(); // <-- HINZUGEFÜGT: Authentifizierung VOR Authorization
app.UseAuthorization();  // <-- DIESE WAR SCHON DA

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