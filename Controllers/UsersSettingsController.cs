using RevisiaAPI.Db;
using RevisiaAPI.Models;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;

namespace RevisiaAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class UserSettingsController : ControllerBase
    {
        [Authorize]
        [HttpGet]
        public async Task<IActionResult> GetUserSettings()
        {
            int userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            await using var conn = DbConnection.GetConnection();
            await conn.OpenAsync();
            var settings = await UserSettingsSql.GetByUserIdAsync(userId, conn);
            return Ok(settings);
        }
        [Authorize]
        [HttpPatch]
        public async Task<IActionResult> UpdateUserSettings([FromBody] UserSettings settings)
        {
            int userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            settings.userId = userId;
            await using var conn = DbConnection.GetConnection();
            await conn.OpenAsync();
            var existingSettings = await UserSettingsSql.GetByUserIdAsync(userId, conn);
            if (existingSettings == null)
            {
                var inserted = await UserSettingsSql.InsertSettings(settings, conn);
                if (!inserted)
                {
                    return BadRequest("Failed to insert user settings.");
                }
            }
            else
            {
                var updated = await UserSettingsSql.UpdateSettings(settings, conn);
                if (!updated)
                {
                    return BadRequest("Failed to update user settings.");
                }
            }
            return Ok(settings);
        }
    }
}
