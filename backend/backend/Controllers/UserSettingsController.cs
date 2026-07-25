using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace backend.Controllers;

[Route("api/users")]
[ApiController]
[Authorize]
public class UserSettingsController : ControllerBase
{
    private readonly UserSettingsService _userSettingsService;

    public UserSettingsController(UserSettingsService userSettingsService)
    {
        _userSettingsService = userSettingsService;
    }

    [HttpPut("username")]
    public async Task<IActionResult> UpdateUsername([FromBody] UpdateUsernameDto dto)
    {
        var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);

        if (string.IsNullOrEmpty(userIdString) || !int.TryParse(userIdString, out int userId))
        {
            return Unauthorized("Nicht autorisiert.");
        }

        var success = _userSettingsService.UpdateUsername(userId, dto.Username);

        if (!success)
        {
            return NotFound("Benutzer nicht gefunden.");
        }

        return Ok(new { message = "Benutzername erfolreich aktualisiert" });
    }

    [HttpPut("password")]
    public async Task<IActionResult> UpdateUsername([FromBody] UpdatePasswordDto dto)
    {
        var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userIdString) || !int.TryParse(userIdString, out int userId))
        {
            return Unauthorized("Nicht autorisiert.");
        }

        if (string.IsNullOrWhiteSpace(dto.OldPassword) || string.IsNullOrWhiteSpace(dto.NewPassword))
        {
            return BadRequest("Bitte altes und neues Passwort angeben.");
        }

        var success = _userSettingsService.UpdatePassword(userId, dto.NewPassword, dto.OldPassword);

        if (!success)
        {
            return BadRequest("Das alte Passwort ist falsch.");
        }

        return Ok(new { message = "Passwort erfolreich geändert!"});
    }


}
