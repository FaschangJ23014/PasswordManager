//----------------------------------------
// .Net Core WebApi project create script 
//           v10.2.2 from 2026-04-13
//   (C)Robert Grueneis/HTL Grieskirchen 
//----------------------------------------
using GrueneisR.RestClientGenerator;
using Microsoft.OpenApi;
using backend; // Stell sicher, dass dein Namespace hier stimmt!
using Microsoft.EntityFrameworkCore;

string corsKey = "_myCorsKey";
string swaggerVersion = "v1";
string swaggerTitle = "backend";
string restClientFolder = Environment.CurrentDirectory;
string restClientFilename = "_requests.http";

var builder = WebApplication.CreateBuilder(args);

#region -------------------------------------------- ConfigureServices
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.PropertyNameCaseInsensitive = true;
    });
builder.Services.AddScoped<PasswordsService>();

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
    x => x.SetIsOriginAllowed(_ => true)
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
AppDomain.CurrentDomain.SetData("DataDirectory", AppDomain.CurrentDomain.BaseDirectory);

Console.ForegroundColor = ConsoleColor.Cyan;
Console.WriteLine($"++++ ConnectionString: {connectionString}");
Console.ResetColor();

builder.Services.AddDbContext<DataContext>(options => 
    options.UseSqlite(@"Data Source=C:\Schule\Projekte\PasswordManager\Database\passwords.db"));

#endregion

var app = builder.Build();

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
app.Run();