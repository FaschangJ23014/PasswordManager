namespace backend.Controllers;

public record struct OkStatus(bool IsOk, int Nr, string? Error = null);

[Route("[controller]")]
[ApiController]
public class ValuesController(PasswordsContext db) : ControllerBase
{

  [HttpGet("Passwords")]
  public OkStatus GetPasswordsToTest()
  {
    this.Log();
    try
    {
      int nr = db.Passwords.Count();
      return new OkStatus(true, nr);
    }
    catch (Exception exc)
    {
      return new OkStatus(false, -1, exc.Message);
    }
  }
}
