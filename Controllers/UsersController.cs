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

        var token = _jwtService.GenerateToken(user, dto.RememberMe ? 15 :1440);
        Response.Cookies.Append("jwt", token, new CookieOptions
        {
            HttpOnly = true,
            Secure = true,
            SameSite = SameSiteMode.Strict,
            Expires = DateTime.UtcNow.AddMinutes(dto.RememberMe ? 15 : 1440)
        });
        if (dto.RememberMe)
        {
            var refreshToken = _jwtService.GenerateRefreshToken(30); 
            refreshToken.UserId = user.Id;
            await RefreshTokenSql.CreateRefreshTokenAsync(refreshToken, conn);

            Response.Cookies.Append("refreshToken", refreshToken.Token, new CookieOptions
            {
                HttpOnly = true,
                Secure = true,
                SameSite = SameSiteMode.Strict,
                Expires = refreshToken.ExpiresAt
            });
        }
        else
        {
            Response.Cookies.Delete("refreshToken");
        }

        return Ok(new { message = "Registered successfully" });
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] UserLoginDto dto)
    {
        if ((string.IsNullOrWhiteSpace(dto.Username) && string.IsNullOrWhiteSpace(dto.Email)) || string.IsNullOrWhiteSpace(dto.Password))
            return BadRequest("Username/email and password required.");

        string? usernameOrEmail = dto.Username ?? dto.Email;
        if (string.IsNullOrWhiteSpace(usernameOrEmail))
            return BadRequest("Username or email must be provided.");

        await using var conn = DbConnection.GetConnection();
        await conn.OpenAsync();

        var user = await UserSql.GetUserByUsernameOrEmailAsync(usernameOrEmail, conn);
        if (user == null) return Unauthorized("No user found with those creds, blud.");

        bool validPassword = PasswordHelper.VerifyPassword(dto.Password, user.PasswordHash);
        if (!validPassword) return Unauthorized("Password incorrect, no cap.");


        string token = _jwtService.GenerateToken(user, dto.RememberMe ? 15 : 1440);
        Response.Cookies.Append("jwt", token, new CookieOptions
        {
            HttpOnly = true,
            Secure = true,
            SameSite = SameSiteMode.Strict,
            Expires = DateTime.UtcNow.AddMinutes(dto.RememberMe ? 15 : 1440)
        });

        if (dto.RememberMe)
        {
            var refreshToken = _jwtService.GenerateRefreshToken(30);
            refreshToken.UserId = user.Id; 
            await RefreshTokenSql.CreateRefreshTokenAsync(refreshToken, conn);

            Response.Cookies.Append("refreshToken", refreshToken.Token, new CookieOptions
            {
                HttpOnly = true,
                Secure = true,
                SameSite = SameSiteMode.Strict,
                Expires = refreshToken.ExpiresAt
            });
        }
        else
        {
            Response.Cookies.Delete("refreshToken");
        }

        return Ok();
    }

    [HttpPost("refresh")]
    public async Task<IActionResult> Refresh()
    {
        if (!Request.Cookies.TryGetValue("refreshToken", out var refreshToken))
            return Unauthorized("No refresh token found, blud.");

        await using var conn = DbConnection.GetConnection();
        await conn.OpenAsync();

        var jwtService = _jwtService;

        // Hash incoming token before querying
        string hashedToken = jwtService.HashToken(refreshToken);

        var storedToken = await RefreshTokenSql.GetRefreshTokenAsync(hashedToken, conn);
        if (storedToken == null || storedToken.IsInvalidated || storedToken.ExpiresAt < DateTime.UtcNow)
        {
            Response.Cookies.Delete("refreshToken");
            return Unauthorized("Invalid or expired refresh token, no cap.");
        }

        var user = await UserSql.GetUserByIdAsync(storedToken.UserId, conn);
        if (user == null)
        {
            Response.Cookies.Delete("refreshToken");
            return Unauthorized("User not found, log back in.");
        }

        await RefreshTokenSql.InvalidateRefreshTokenAsync(storedToken.Id, conn);

        var newRefreshToken = jwtService.GenerateRefreshToken(30);
        newRefreshToken.UserId = user.Id; 

        await RefreshTokenSql.CreateRefreshTokenAsync(newRefreshToken, conn);

        Response.Cookies.Append("refreshToken", newRefreshToken.Token, new CookieOptions
        {
            HttpOnly = true,
            Secure = true,
            SameSite = SameSiteMode.Strict,
            Expires = newRefreshToken.ExpiresAt
        });

        var newJwt = _jwtService.GenerateToken(user, 15); // short JWT 15 min on refresh
        Response.Cookies.Append("jwt", newJwt, new CookieOptions
        {
            HttpOnly = true,
            Secure = true,
            SameSite = SameSiteMode.Strict,
            Expires = DateTime.UtcNow.AddMinutes(15)
        });

        return Ok(new { message = "Tokens refreshed, locked in." });
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
        var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (!int.TryParse(userIdClaim, out int userId))
        {
            Response.Cookies.Delete("jwt");
            return Unauthorized("Invalid token payload, please log in again.");
        }

        await using var conn = DbConnection.GetConnection();
        await conn.OpenAsync();

        var user = await UserSql.GetUserByIdAsync(userId, conn);
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
