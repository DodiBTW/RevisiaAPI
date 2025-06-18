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
    [Authorize]
    [HttpGet("{id}")]
    public async Task<IActionResult> GetDeckById(int id)
    {
        int userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        await using var conn = DbConnection.GetConnection();
        await conn.OpenAsync();
        
        var deck = await DeckSql.GetDeckByIdAsync(id, userId, conn);
        
        if (deck == null)
        {
            return NotFound();
        }
        
        return Ok(deck);
    }
    [Authorize]
    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateDeck([FromBody] Deck updatedDeck)
    {
        int userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        updatedDeck.UserId = userId;
        updatedDeck.UpdatedAt = DateTime.UtcNow;
        
        await using var conn = DbConnection.GetConnection();
        await conn.OpenAsync();
        
        var existingDeck = await DeckSql.GetDeckByIdAsync(updatedDeck.Id, userId, conn);
        
        if (existingDeck == null)
        {
            return NotFound();
        }
        
        var resp = DeckSql.UpdateDeckAsync(updatedDeck, conn);

        if (resp == null)
        {
            return BadRequest("Failed to update deck");
        }
        return NoContent();
    }
    [Authorize]
    [HttpGet("{id}/cards")]
    public async Task<IActionResult> GetDeckCards(int id)
    {
        int userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        await using var conn = DbConnection.GetConnection();
        await conn.OpenAsync();
        
        var deck = await DeckSql.GetDeckByIdAsync(id, userId, conn);
        
        if (deck == null)
        {
            return NotFound("Deck not found");
        }
        
        var cards = await CardSql.GetCardsByDeckIdAsync(id, conn);
        
        return Ok(cards);
    }
}