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
        if (card == null)
        {
            return NotFound("Card not found.");
        }

        var deck = await DeckSql.GetDeckByIdAsync(card.DeckId, userId, conn);
        if (deck == null)
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
        await DeckSql.UpdateDeckLastUpdatedAsync(deck.Id, userId,conn);
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
            return NotFound("Card or deck not found.");
        }

        // Delete associated image files before deleting the card
        if (!string.IsNullOrEmpty(card.FrontImage))
        {
            string frontImagePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", card.FrontImage.TrimStart('/'));
            if (System.IO.File.Exists(frontImagePath))
            {
                System.IO.File.Delete(frontImagePath);
            }
        }

        if (!string.IsNullOrEmpty(card.BackImage))
        {
            string backImagePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", card.BackImage.TrimStart('/'));
            if (System.IO.File.Exists(backImagePath))
            {
                System.IO.File.Delete(backImagePath);
            }
        }

        // Delete the card directory if it's empty after removing images
        string cardDir = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads", "cards", cardId.ToString());
        if (Directory.Exists(cardDir) && !Directory.EnumerateFileSystemEntries(cardDir).Any())
        {
            Directory.Delete(cardDir);
        }

        await CardSql.DeleteCardAsync(cardId, conn);
        await DeckSql.UpdateDeckLastUpdatedAsync(deck.Id, userId, conn);

        return Ok(new { message = "Card deleted successfully." });
    }
    [Authorize]
    [HttpPatch("{cardId}")]
    public async Task<IActionResult> UpdateCard(int cardId, [FromBody] Card updatedCard)
    {
        int userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        updatedCard.Id = cardId;
        updatedCard.UpdatedAt = DateTime.UtcNow;
        var conn = DbConnection.GetConnection();
        await conn.OpenAsync();

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
        await DeckSql.UpdateDeckLastUpdatedAsync(deck.Id, userId, conn);
        return Ok(updatedCard);
    }
    [Authorize]
    [HttpPost("review/{cardId}")]
    public async Task<IActionResult> ReviewCard(int cardId, [FromBody] ReviewData data)
    {
        await using var conn = DbConnection.GetConnection();
        await conn.OpenAsync();
        int userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var card = await CardSql.GetCardByIdAsync(cardId, conn);
        if (card == null)
        {
            return NotFound("Card not found.");
        }
        var deck = await DeckSql.GetDeckByIdAsync(card.DeckId, userId, conn);
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
        await DeckSql.UpdateDeckLastUpdatedAsync(deck.Id, userId, conn);
        return Ok(new { message = "Card reviewed successfully!"});
    }

    [Authorize]
    [HttpPost("{cardId}/upload-front-image")]
    public async Task<IActionResult> UploadFrontImage(int cardId, IFormFile image)
    {
        return await UploadCardImage(cardId, image, "front");
    }

    [Authorize]
    [HttpPost("{cardId}/upload-back-image")]
    public async Task<IActionResult> UploadBackImage(int cardId, IFormFile image)
    {
        return await UploadCardImage(cardId, image, "back");
    }

    [Authorize]
    [HttpDelete("{cardId}/image/{side}")]
    public async Task<IActionResult> DeleteCardImage(int cardId, string side)
    {
        if (side != "front" && side != "back")
        {
            return BadRequest("Side must be 'front' or 'back'");
        }

        int userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        await using var conn = DbConnection.GetConnection();
        await conn.OpenAsync();

        var card = await CardSql.GetCardByIdAsync(cardId, conn);
        if (card == null)
        {
            return NotFound("Card not found.");
        }

        var deck = await DeckSql.GetDeckByIdAsync(card.DeckId, userId, conn);
        if (deck == null)
        {
            return NotFound("Deck not found or user doesn't own card.");
        }

        // Delete the physical file
        string? imagePath = side == "front" ? card.FrontImage : card.BackImage;
        if (!string.IsNullOrEmpty(imagePath))
        {
            string fullPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", imagePath.TrimStart('/'));
            if (System.IO.File.Exists(fullPath))
            {
                System.IO.File.Delete(fullPath);
            }
        }

        // Update the card in database
        if (side == "front")
        {
            card.FrontImage = null;
        }
        else
        {
            card.BackImage = null;
        }
        card.UpdatedAt = DateTime.UtcNow;

        await CardSql.UpdateCardAsync(card, conn);
        await DeckSql.UpdateDeckLastUpdatedAsync(deck.Id, userId, conn);

        return Ok(new { message = $"{side} image deleted successfully." });
    }

    private async Task<IActionResult> UploadCardImage(int cardId, IFormFile image, string side)
    {
        if (image == null || image.Length == 0)
        {
            return BadRequest("No image file provided.");
        }

        // Validate file type
        var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif", ".webp" };
        var fileExtension = Path.GetExtension(image.FileName).ToLowerInvariant();
        if (!allowedExtensions.Contains(fileExtension))
        {
            return BadRequest("Invalid file type. Only jpg, jpeg, png, gif, and webp are allowed.");
        }

        // Validate file size (max 5MB)
        if (image.Length > 5 * 1024 * 1024)
        {
            return BadRequest("File size cannot exceed 5MB.");
        }

        int userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        await using var conn = DbConnection.GetConnection();
        await conn.OpenAsync();

        var card = await CardSql.GetCardByIdAsync(cardId, conn);
        if (card == null)
        {
            return NotFound("Card not found.");
        }

        var deck = await DeckSql.GetDeckByIdAsync(card.DeckId, userId, conn);
        if (deck == null)
        {
            return NotFound("Deck not found or user doesn't own card.");
        }

        // Create directory structure
        string uploadsDir = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads", "cards", cardId.ToString());
        Directory.CreateDirectory(uploadsDir);

        // Delete existing image if it exists
        string? existingImagePath = side == "front" ? card.FrontImage : card.BackImage;
        if (!string.IsNullOrEmpty(existingImagePath))
        {
            string existingFullPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", existingImagePath.TrimStart('/'));
            if (System.IO.File.Exists(existingFullPath))
            {
                System.IO.File.Delete(existingFullPath);
            }
        }

        // Generate unique filename
        string fileName = $"{side}_{Guid.NewGuid()}{fileExtension}";
        string filePath = Path.Combine(uploadsDir, fileName);
        string relativePath = $"/uploads/cards/{cardId}/{fileName}";

        // Save the file
        using (var stream = new FileStream(filePath, FileMode.Create))
        {
            await image.CopyToAsync(stream);
        }

        // Update the card in database
        if (side == "front")
        {
            card.FrontImage = relativePath;
        }
        else
        {
            card.BackImage = relativePath;
        }
        card.UpdatedAt = DateTime.UtcNow;

        await CardSql.UpdateCardAsync(card, conn);
        await DeckSql.UpdateDeckLastUpdatedAsync(deck.Id, userId, conn);

        return Ok(new { 
            message = $"{side} image uploaded successfully.",
            imagePath = relativePath
        });
    }
}