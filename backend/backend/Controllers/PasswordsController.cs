using Microsoft.AspNetCore.Authorization;
using System.Security.Claims; // WICHTIG für User.FindFirst

[Route("api/[controller]")]
[ApiController]
[Authorize] // JWT-Schutz ist jetzt aktiv
public class PasswordsController : ControllerBase
{
    private readonly PasswordsService _passwordsService;

    public PasswordsController(PasswordsService passwordsService)
    {
        _passwordsService = passwordsService;
    }

    // Helper: Holt die User-ID aus dem Token
    private string GetUserId() => User.FindFirst(ClaimTypes.NameIdentifier)?.Value
                                  ?? throw new Exception("User nicht identifiziert.");

    [HttpGet]
    public ActionResult<List<PasswordEntry>> GetAll([FromQuery] string? search = null)
    {
        // Übergib die UserId an den Service, damit nur die eigenen Daten geladen werden
        int userId = int.Parse(GetUserId());
        return Ok(_passwordsService.GetAllForUser(userId, search));
    }

    [HttpPost]
    public ActionResult<PasswordEntry> Create([FromBody] PasswordEntry newEntry)
    {
        try
        {
            string userIdString = GetUserId();
            if (int.TryParse(userIdString, out int userId))
            {
                newEntry.UserId = userId;
                return Ok(_passwordsService.CreatePassword(newEntry));
            }
            return BadRequest("Ungültige User-ID im Token.");
        }
        catch (Exception ex)
        {
            // Schickt den Fehler direkt ins Frontend, statt einfach nur 500 zu werfen
            return StatusCode(500, $"Fehler beim Speichern: {ex.Message} | Stack: {ex.StackTrace}");
        }
    }

    [HttpDelete("{id}")]
    public ActionResult Delete(int id)
    {
        int userId = int.Parse(GetUserId());
        _passwordsService.DeleteForUser(id, userId); // Löschen nur wenn User-ID passt
        return Ok();
    }
}