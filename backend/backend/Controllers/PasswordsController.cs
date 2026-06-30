using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace backend.Controllers;

[Route("api/[controller]")]
[ApiController]
public class PasswordsController : ControllerBase
{
    private readonly PasswordsService _passwordsService;

    public PasswordsController(PasswordsService passwordsService)
    {
        _passwordsService = passwordsService;
    }

    [HttpGet]
    public ActionResult<List<PasswordEntry>> GetAll([FromQuery] string? search = null)
    {
        if (!string.IsNullOrEmpty(search))
        {
            var filtered = _passwordsService.SearchPasswords(search);
            return Ok(filtered);
        }

        var allPasswords = _passwordsService.GetAllPasswords();
        return Ok(allPasswords);
    }

    [HttpPost]
    public ActionResult<PasswordEntry> Create([FromBody] PasswordEntry newEntry)
    {
        var created = _passwordsService.CreatePassword(newEntry);
        return Ok(created);
    }
}
