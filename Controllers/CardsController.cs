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
    public async Task<IActionResult> GetCard(int cardId)
    {
        await using var conn = DbConnection.GetConnection();
        await conn.OpenAsync();
        int userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var card = await CardSql.GetCardByIdAsync(cardId, conn);

        var deck = DeckSql.GetDeckByIdAsync(card.DeckId, userId, conn);
        if (card == null || deck == null)
        {
            return NotFound("Card not found or does not belong to the user.");
        }
        return Ok(card);
    }

    [Authorize]
    [HttpPost]
    public async Task<IActionResult> CreateCard([FromBody] Card card)
    {
        int userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        card.CreatedAt = DateTime.UtcNow;
        card.UpdatedAt = DateTime.UtcNow;
        await using var conn = DbConnection.GetConnection();
        await conn.OpenAsync();


        var deck = await DeckSql.GetDeckByIdAsync(card.DeckId, userId, conn);
        if (deck == null)
        {
            return NotFound("Deck not found.");
        }
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
        await CardSql.DeleteCardAsync(cardId, conn);

        return Ok(new { message = "Card deleted successfully." });
    }
    [Authorize]
    [HttpPatch("{cardId}")]
    public async Task<IActionResult> UpdateCard(int cardId, [FromBody] Card updatedCard)
    {
        int userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        updatedCard.Id = cardId;
        updatedCard.UpdatedAt = DateTime.UtcNow;
        int retry = 5;
        var conn = DbConnection.GetConnection();
        while (retry > 0)
        {
            try
            {
                await conn.OpenAsync();
                break;
            }
            catch (Exception ex)
            {
                retry--;
                await Task.Delay(250);
            }
        }

        var originalCard = await CardSql.GetCardByIdAsync(cardId, conn);
        var deck = await DeckSql.GetDeckByIdAsync(originalCard?.DeckId ?? 0, userId, conn);
        if (deck == null)
        {
            return NotFound("Deck not found. Deck id : " + updatedCard.DeckId + " Card : " + updatedCard);
        }
        // This sucks
        updatedCard.CreatedAt = originalCard?.CreatedAt ?? DateTime.UtcNow;
        updatedCard.UpdatedAt = DateTime.UtcNow;
        updatedCard.NextReview = originalCard?.NextReview ?? DateTime.UtcNow.AddDays(1);
        updatedCard.DeckId = originalCard?.DeckId ?? updatedCard.DeckId;
        // The sucky part is over
        await CardSql.UpdateCardAsync(updatedCard, conn);
        return Ok(updatedCard);
    }
    [Authorize]
    [HttpPost("review/{cardId}")]
    public async Task<IActionResult> ReviewCard(int cardId, [FromBody] ReviewData data)
    {
        await using var conn = DbConnection.GetConnection();
        await conn.OpenAsync();
        var card = await CardSql.GetCardByIdAsync(cardId, conn);
        if (card == null)
        {
            return NotFound("Card not found.");
        }
        var deck = await DeckSql.GetDeckByIdAsync(card.DeckId, int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!), conn);
        if (deck == null)
        {
            return NotFound("Deck not found or user doesn't own card.");
        }
        // Update card review data
        card.ReviewCount += 1;
        card.Difficulty = data.NewDifficulty;
        card.Interval = data.NewInterval;
        card.NextReview = data.NextReview;
        card.UpdatedAt = DateTime.UtcNow;

        await CardSql.UpdateCardAsync(card, conn);
        return Ok();
    }
}