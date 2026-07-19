//----------------------------------------
// .Net Core WebApi project create script 
//           v10.2.2 from 2026-04-13
//   (C)Robert Grueneis/HTL Grieskirchen 
//----------------------------------------
using backend; // Stell sicher, dass dein Namespace hier stimmt!
using GrueneisR.RestClientGenerator;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi;
using System.Text;

string corsKey = "_myCorsKey";
string swaggerVersion = "v1";
string swaggerTitle = "backend";
string restClientFolder = Environment.CurrentDirectory;
string restClientFilename = "_requests.http";

var builder = WebApplication.CreateBuilder(new WebApplicationOptions
{
    Args = args,
    ContentRootPath = AppContext.BaseDirectory
});

var builder = WebApplication.CreateBuilder(new WebApplicationOptions
{
    Args = args,
    ContentRootPath = AppContext.BaseDirectory
});

// Füge dies hier ein, um das Problem zu beheben:
builder.Configuration.Sources.Clear();
builder.Configuration
    .AddJsonFile("appsettings.json", optional: true, reloadOnChange: false) // <--- WICHTIG: reloadOnChange auf false!
    .AddEnvironmentVariables();

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["JWT:SecretKey"]!)),
            ValidateIssuer = false, //Bei Entwicklung auf false
            ValidateAudience = false
        };
    });

builder.Services.AddAuthorization();

#region -------------------------------------------- ConfigureServices
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.PropertyNameCaseInsensitive = true;
    });
builder.Services.AddScoped<PasswordsService>();
builder.Services.AddScoped<AuthService>();

builder.Services
  .AddEndpointsApiExplorer()
  .AddAuthorization()
  .AddSwaggerGen(x => x.SwaggerDoc(
    swaggerVersion,
    new OpenApiInfo { Title = swaggerTitle, Version = swaggerVersion }
  ))
// HIER ist die originale HTL-CORS-Policy, die absolut ALLES erlaubt (auch dein Svelte auf Port 5173!)
.AddCors(options => options.AddPolicy(
  corsKey,
  x => x.WithOrigins("https://password-manager-sigma-lemon.vercel.app") 
        .AllowAnyMethod()
        .AllowAnyHeader()
        .AllowCredentials()
))

  .AddRestClientGenerator(options => options
    .SetFolder(restClientFolder)
    .SetFilename(restClientFilename)
    .SetAction($"swagger/{swaggerVersion}/swagger.json")
  );

builder.Services.AddLogging(x => x.AddCustomFormatter());

string? connectionString = builder.Configuration.GetConnectionString("Passwords")!;

Console.ForegroundColor = ConsoleColor.Cyan;
Console.WriteLine($"++++ ConnectionString: {connectionString}");
Console.ResetColor();

builder.Services.AddDbContext<DataContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("Passwords")));

#endregion

var app = builder.Build();

app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

#region -------------------------------------------- Middleware pipeline

// WICHTIG: Die CORS-Middleware muss ganz am Anfang der Pipeline stehen!
app.UseCors(corsKey); 

if (app.Environment.IsDevelopment())
{
  app.UseDeveloperExceptionPage();
  Console.ForegroundColor = ConsoleColor.Green;
  Console.WriteLine("++++ Swagger enabled: http://localhost:5000");
  app.UseSwagger();
  Console.WriteLine($@"++++ RestClient generating (after first request) to {restClientFolder}\{restClientFilename}");
  app.UseRestClientGenerator();
  app.UseSwaggerUI(x => x.SwaggerEndpoint($"/swagger/{swaggerVersion}/swagger.json", swaggerTitle));
  Console.ResetColor();
}

#endregion

app.Map("/", () => Results.Redirect("/swagger"));
app.MapControllers();

Console.WriteLine($"Ready for clients at {DateTime.Now:HH:mm:ss} ...");
// Automatische Migration / Tabellenerstellung für PostgreSQL
// Ersetze deinen bisherigen Datenbank-Erstellungs-Block damit:
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    var context = services.GetRequiredService<DataContext>();
    // Dies wendet alle ausstehenden Migrationen automatisch bei App-Start an
    context.Database.Migrate();
    Console.WriteLine("++++ Migrationen erfolgreich auf Datenbank angewendet!");
}
app.Run();