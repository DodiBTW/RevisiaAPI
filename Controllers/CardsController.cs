using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RevisiaAPI.Db;
using RevisiaAPI.Models;
using System.Security.Claims;

[ApiController]
[Route("api/[controller]")]
public class CardsController : ControllerBase
{
    [Authorize]
    [HttpGet("{cardId}")]
    public async Task<IActionResult> GetCard(int deckId)
    {
        await using var conn = DbConnection.GetConnection();
        await conn.OpenAsync();
        var card = await CardSql.GetCardByIdAsync(deckId, conn);
        return Ok(card);
    }

    [Authorize]
    [HttpPost]
    public async Task<IActionResult> CreateCard([FromBody] Card card)
    {
        int userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        card.CreatedAt = DateTime.UtcNow;
        card.UpdatedAt = DateTime.UtcNow;
        var deck = await DeckSql.GetDeckByIdAsync(card.DeckId, userId, DbConnection.GetConnection());
        if (deck == null)
        {
            return NotFound("Deck not found.");
        }
        await using var conn = DbConnection.GetConnection();
        await conn.OpenAsync();
        var id = await CardSql.CreateCardAsync(card, conn);
        card.Id = id;
        return Ok(card);
    }
    [Authorize]
    [HttpDelete("{cardId}")]
    public async Task<IActionResult> DeleteCard(int cardId)
    {
        int userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        await using var conn = DbConnection.GetConnection();
        await conn.OpenAsync();
        var card = await CardSql.GetCardByIdAsync(cardId, conn);
        var deck = await DeckSql.GetDeckByIdAsync(card?.DeckId ?? 0, userId, conn);
        if (card == null || deck == null)
        {
            return NotFound("Card not found or does not belong to the user.");
        }
        await CardSql.DeleteCardAsync(cardId, conn);

        return Ok(new { message = "Card deleted successfully." });
    }
    [Authorize]
    [HttpPut("{cardId}")]
    public async Task<IActionResult> UpdateCard(int cardId, [FromBody] Card updatedCard)
    {
        int userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        updatedCard.Id = cardId;
        updatedCard.UpdatedAt = DateTime.UtcNow;
        var deck = await DeckSql.GetDeckByIdAsync(updatedCard.DeckId, userId, DbConnection.GetConnection());
        if (deck == null)
        {
            return NotFound("Deck not found.");
        }
        await using var conn = DbConnection.GetConnection();
        await conn.OpenAsync();
        await CardSql.UpdateCardAsync(updatedCard, conn);
        return Ok(updatedCard);
    }
}