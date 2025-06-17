using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RevisiaAPI.Db;
using RevisiaAPI.Models;
using System.Security.Claims;

[ApiController]
[Route("api/[controller]")]
public class DecksController : ControllerBase
{
    [Authorize]
    [HttpGet]
    public async Task<IActionResult> GetUserDecks()
    {
        int userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        await using var conn = DbConnection.GetConnection();
        await conn.OpenAsync();
        var decks = await DeckSql.GetDecksByUserIdAsync(userId, conn);
        return Ok(decks);
    }

    [Authorize]
    [HttpPost]
    public async Task<IActionResult> CreateDeck([FromBody] Deck deck)
    {
        int userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        deck.UserId = userId;
        deck.CreatedAt = DateTime.UtcNow;
        deck.UpdatedAt = DateTime.UtcNow;
        deck.CardCount = 0;
        await using var conn = DbConnection.GetConnection();
        await conn.OpenAsync();
        var id = await DeckSql.CreateDeckAsync(deck, conn);
        deck.Id = id;
        return Ok(deck);
    }
}