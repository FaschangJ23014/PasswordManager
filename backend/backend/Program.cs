using backend;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi;
using Microsoft.OpenApi.Models; // Wichtig für OpenApiInfo
using System.Text;

var builder = WebApplication.CreateBuilder(new WebApplicationOptions
{
    Args = args,
    ContentRootPath = AppContext.BaseDirectory
});

// Definitionen
string corsKey = "_myCorsKey";
string swaggerVersion = "v1";
string swaggerTitle = "backend";

// Konfiguration
builder.Configuration.Sources.Clear();
builder.Configuration
    .AddJsonFile("appsettings.json", optional: true, reloadOnChange: false)
    .AddEnvironmentVariables();

// Services
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["JWT:SecretKey"] ?? "SUPER_SECRET_KEY_MUST_BE_32_BYTES_LONG")),
            ValidateIssuer = false,
            ValidateAudience = false
        };
    });

// EINMALIGE CORS Konfiguration
builder.Services.AddCors(options =>
{
    options.AddPolicy(corsKey, policy =>
        policy.WithOrigins("https://password-manager-sigma-lemon.vercel.app")
              .AllowAnyMethod()
              .AllowAnyHeader()
              .AllowCredentials());
});

builder.Services.AddControllers();
builder.Services.AddScoped<PasswordsService>();
builder.Services.AddScoped<AuthService>();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(x => x.SwaggerDoc(swaggerVersion, new OpenApiInfo { Title = swaggerTitle, Version = swaggerVersion }));

builder.Services.AddDbContext<DataContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("Passwords")));

var app = builder.Build();

// Pipeline
app.UseCors(corsKey); // CORS muss vor Authentication/Authorization kommen
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

// Swagger (auch in Production, damit du testen kannst)
app.UseSwagger();
app.UseSwaggerUI(x => x.SwaggerEndpoint($"/swagger/{swaggerVersion}/swagger.json", swaggerTitle));

app.Map("/", () => Results.Redirect("/swagger"));

// Migration
using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<DataContext>();
    context.Database.Migrate();
}

app.Run();