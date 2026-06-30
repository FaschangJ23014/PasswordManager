using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace backend.Controllers;

[Route("api/[controller]")]
[ApiController]
public class PasswordsController : ControllerBase
{
    private readonly PasswordsService _passwordsService;
    private readonly string _masterPassword;
    public PasswordsController(PasswordsService passwordsService, IConfiguration configuration)
    {
        _passwordsService = passwordsService;
        _masterPassword = configuration["ShieldSettings:MasterPassword"] ?? "StandardFallback123!";
    }

    private bool IsAuthorized()
    {
        if (Request.Headers.TryGetValue("X-Master-Password", out var submittedPassword))
        {
            return submittedPassword == _masterPassword;
        }
        return false;
    }

    [HttpGet]
    public ActionResult<List<PasswordEntry>> GetAll([FromQuery] string? search = null)
    {
        if (!IsAuthorized()) return Unauthorized("Falsches oder fehlendes Master-Passwort!");

        if (!string.IsNullOrEmpty(search))
        {
            var filtered = _passwordsService.SearchPasswords(search);
            return Ok(filtered);
        }

        var passwords = _passwordsService.GetAllPasswords();
        return Ok(passwords);
    }

    [HttpPost]
    public ActionResult<PasswordEntry> Create([FromBody] PasswordEntry newEntry)
    {
        if (!IsAuthorized()) return Unauthorized("Falsches oder fehlendes Master-Passwort!");

        try
        {
            // Validierungsschutz: Falls JSON-Mapping schiefging
            if (newEntry == null) return BadRequest("Daten konnten nicht gelesen werden.");

            var created = _passwordsService.CreatePassword(newEntry);
            return Ok(created);
        }
        catch (Exception ex)
        {
            // Das schreibt den echten Fehler rot ins dotnet-Terminal!
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine($"++++ FEHLER BEIM SPEICHERN: {ex.Message}");
            if (ex.InnerException != null) Console.WriteLine($"++++ INNER: {ex.InnerException.Message}");
            Console.ResetColor();

            return StatusCode(500, $"Datenbank-Fehler: {ex.Message}");
        }
    }
}
