using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;

namespace backend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PasswordsController : ControllerBase
    {
        private readonly PasswordsService _passwordsService;
        private readonly string _masterPassword;

        public PasswordsController(PasswordsService passwordsService, IConfiguration configuration)
        {
            _passwordsService = passwordsService;
            // Sicherstellen, dass hier IMMER ein String drinsteht, niemals null!
            _masterPassword = configuration["ShieldSettings:MasterPassword"] ?? "StandardFallback123!";
        }

        private bool IsAuthorized()
        {
            try
            {
                if (Request.Headers.TryGetValue("X-Master-Password", out var submittedPassword))
                {
                    string rawHeader = submittedPassword.ToString();
                    if (string.IsNullOrWhiteSpace(rawHeader))
                    {
                        Console.WriteLine("++++ Autorisierung fehlgeschlagen: Header ist leer.");
                        return false;
                    }

                    // Absolut sicheres Decodieren
                    string decodedPassword = System.Net.WebUtility.UrlDecode(rawHeader);

                    return decodedPassword == _masterPassword;
                }

                Console.WriteLine("++++ Autorisierung fehlgeschlagen: X-Master-Password Header fehlt komplett.");
                return false;
            }
            catch (Exception ex)
            {
                // Verhindert den HTTP 500 Absturz und loggt das Problem!
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"++++ CRASH IN ISAUTHORIZED: {ex.Message}");
                Console.ResetColor();
                return false;
            }
        }

        [HttpGet]
        public ActionResult<List<PasswordEntry>> GetAll([FromQuery] string? search = null)
        {
            try
            {
                if (!IsAuthorized())
                {
                    return Unauthorized("Falsches oder fehlendes Master-Passwort!");
                }

                if (!string.IsNullOrEmpty(search))
                {
                    var filtered = _passwordsService.SearchPasswords(search);
                    return Ok(filtered);
                }

                var passwords = _passwordsService.GetAllPasswords();
                return Ok(passwords);
            }
            catch (Exception ex)
            {
                // Falls DOCH irgendwas tiefer im Service knallt, fangen wir es hier ab!
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"++++ CRASH IN GETALL: {ex.Message}");
                if (ex.InnerException != null) Console.WriteLine($"++++ INNER: {ex.InnerException.Message}");
                Console.ResetColor();

                // Wir geben ein sauberes BadRequest oder Object zurück, DAMIT CORS NICHT BLOCKIERT
                return StatusCode(500, $"Interner Serverfehler abgefangen: {ex.Message}");
            }
        }

        [HttpPost]
        public ActionResult<PasswordEntry> Create([FromBody] PasswordEntry newEntry)
        {
            if (!IsAuthorized()) return Unauthorized("Falsches oder fehlendes Master-Passwort!");

            try
            {
                if (newEntry == null) return BadRequest("Daten konnten nicht gelesen werden.");

                var created = _passwordsService.CreatePassword(newEntry);
                return Ok(created);
            }
            catch (Exception ex)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"++++ FEHLER BEIM SPEICHERN: {ex.Message}");
                Console.ResetColor();

                return StatusCode(500, $"Datenbank-Fehler: {ex.Message}");
            }
        }

        [HttpDelete("{id}")]
        public ActionResult Delete(int id)
        {
            if (!IsAuthorized()) return Unauthorized("Falsches oder fehlendes Master-Passwort!");

            try
            {
                _passwordsService.Delete(id);
                return Ok("Eintrag erfolgreich gelöscht. 🗑️");
            }
            catch (Exception ex)
            {
                return NotFound(ex.Message);
            }
        }
    }
}