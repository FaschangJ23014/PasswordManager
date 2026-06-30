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
        // Wir schauen, ob im HTTP-Header das Passwort mitgeschickt wurde
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

        var created = _passwordsService.CreatePassword(newEntry);
        return Ok(created);
    }
}
