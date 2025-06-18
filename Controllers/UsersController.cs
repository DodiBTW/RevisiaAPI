using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MySqlConnector;
using RevisiaAPI.Db;
using RevisiaAPI.Models;
using RevisiaAPI.Services;
using System.Security.Claims;
using System.Threading.Tasks;

[ApiController]
[Route("api/[controller]")]
public class UsersController : ControllerBase
{
    private readonly JwtService _jwtService;

    public UsersController(JwtService jwtService)
    {
        _jwtService = jwtService;
    }

    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] UserLoginDto dto)
    {
        if (string.IsNullOrWhiteSpace(dto.Username) || string.IsNullOrWhiteSpace(dto.Password) || string.IsNullOrWhiteSpace(dto.Email))
            return BadRequest("Username, email, and password are mandatory.");

        string hashedPassword = PasswordHelper.HashPassword(dto.Password);

        await using var conn = DbConnection.GetConnection();
        await conn.OpenAsync();

        bool created = await UserSql.CreateUserAsync(dto.Username, dto.Email, hashedPassword, conn);
        if (!created) return Conflict("Username or email already taken.");

        var user = await UserSql.GetUserByUsernameOrEmailAsync(dto.Username, conn);
        if (user == null) return StatusCode(500, "User creation failed, please try again.");

        var token = _jwtService.GenerateToken(user);
        Response.Cookies.Append("jwt", token, new CookieOptions
        {
            HttpOnly = true,
            Secure = true,
            SameSite = SameSiteMode.Strict,
            Expires = DateTime.UtcNow.AddHours(2)
        });

        return Ok(new { message = "Registered successfully" });
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] UserLoginDto dto)
    {
        if ((string.IsNullOrWhiteSpace(dto.Username) && string.IsNullOrWhiteSpace(dto.Email)) || string.IsNullOrWhiteSpace(dto.Password))
            return BadRequest("Username/email and password required.");

        var usernameOrEmail = dto.Username ?? dto.Email;

        if (string.IsNullOrWhiteSpace(usernameOrEmail)) return BadRequest("Username or email must be provided.");

        await using var conn = DbConnection.GetConnection();
        await conn.OpenAsync();

        var user = await UserSql.GetUserByUsernameOrEmailAsync(usernameOrEmail, conn);
        if (user == null) return Unauthorized("No user found with those creds, blud.");

        bool validPassword = PasswordHelper.VerifyPassword(dto.Password, user.PasswordHash);
        if (!validPassword) return Unauthorized("Password incorrect, no cap.");

        string token = _jwtService.GenerateToken(user);
        Response.Cookies.Append("jwt", token, new CookieOptions
        {
            HttpOnly = true,
            Secure = true,
            SameSite = SameSiteMode.Strict,
            Expires = DateTime.UtcNow.AddHours(2)
        });
        return Ok();
    }

    [Authorize]
    [HttpGet("checkTokenAuth")]
    public IActionResult CheckTokenAuth()
    {
        return Ok(new { message = "Token valid" });
    }
    [Authorize]
    [HttpGet("userData")]
    public async Task<IActionResult> GetUserData()
    {
        int userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        await using var conn = DbConnection.GetConnection();
        await conn.OpenAsync();
        var user = await UserSql.GetUserByUsernameOrEmailAsync(userId.ToString(), conn);
        if (user == null)
        {
            Response.Cookies.Delete("jwt");
            return Unauthorized("User not found, please log in again.");
        }
        return Ok(new
        {
            user.Username,
            user.Email,
            user.CreatedAt,
        });
    }
}

