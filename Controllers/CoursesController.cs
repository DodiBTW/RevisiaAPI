using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RevisiaAPI.Db;
using RevisiaAPI.Models;
using System.Security.Claims;

[ApiController]
[Route("api/[controller]")]
public class CoursesController : ControllerBase
{
    [Authorize]
    [HttpGet]
    public async Task<IActionResult> GetCourses()
    {
        int userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        await using var conn = DbConnection.GetConnection();
        await conn.OpenAsync();
        
        var courses = await CourseSql.GetCoursesByUserIdAsync(userId, conn);
        return Ok(courses);
    }

    [Authorize]
    [HttpGet("{courseId}")]
    public async Task<IActionResult> GetCourse(int courseId)
    {
        int userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        await using var conn = DbConnection.GetConnection();
        await conn.OpenAsync();
        
        var course = await CourseSql.GetCourseByIdAsync(courseId, userId, conn);
        if (course == null)
        {
            return NotFound("Course not found.");
        }
        
        return Ok(course);
    }

    [Authorize]
    [HttpPost]
    public async Task<IActionResult> CreateCourse([FromBody] Course course)
    {
        int userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        course.UserId = userId;
        course.CreatedAt = DateTime.UtcNow;
        course.UpdatedAt = DateTime.UtcNow;
        course.IsActive = true;

        await using var conn = DbConnection.GetConnection();
        await conn.OpenAsync();
        
        var id = await CourseSql.CreateCourseAsync(course, conn);
        course.Id = id;
        
        return Ok(course);
    }

    [Authorize]
    [HttpPatch("{courseId}")]
    public async Task<IActionResult> UpdateCourse(int courseId, [FromBody] Course updatedCourse)
    {
        int userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        await using var conn = DbConnection.GetConnection();
        await conn.OpenAsync();

        var originalCourse = await CourseSql.GetCourseByIdAsync(courseId, userId, conn);
        if (originalCourse == null)
        {
            return NotFound("Course not found.");
        }

        // Update fields
        updatedCourse.Id = courseId;
        updatedCourse.UserId = userId;
        updatedCourse.CreatedAt = originalCourse.CreatedAt;
        updatedCourse.UpdatedAt = DateTime.UtcNow;

        await CourseSql.UpdateCourseAsync(updatedCourse, conn);
        return Ok(updatedCourse);
    }

    [Authorize]
    [HttpDelete("{courseId}")]
    public async Task<IActionResult> DeleteCourse(int courseId)
    {
        int userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        await using var conn = DbConnection.GetConnection();
        await conn.OpenAsync();

        var course = await CourseSql.GetCourseByIdAsync(courseId, userId, conn);
        if (course == null)
        {
            return NotFound("Course not found.");
        }

        await CourseSql.DeleteCourseAsync(courseId, conn);
        
        // Remove all deck relationships for this course
        await CourseDeckSql.RemoveAllCourseRelationshipsAsync(courseId, conn);
        
        // Delete all chapters for this course
        await ChapterSql.DeleteAllChaptersByCourseIdAsync(courseId, conn);
        
        return Ok(new { message = "Course deleted successfully." });
    }

    [Authorize]
    [HttpPost("{courseId}/decks/{deckId}")]
    public async Task<IActionResult> AddDeckToCourse(int courseId, int deckId)
    {
        int userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        await using var conn = DbConnection.GetConnection();
        await conn.OpenAsync();

        // Verify user owns the course
        var course = await CourseSql.GetCourseByIdAsync(courseId, userId, conn);
        if (course == null)
        {
            return NotFound("Course not found.");
        }

        // Verify user owns the deck
        var deck = await DeckSql.GetDeckByIdAsync(deckId, userId, conn);
        if (deck == null)
        {
            return NotFound("Deck not found.");
        }

        var relationshipId = await CourseDeckSql.AddDeckToCourseAsync(courseId, deckId, conn);
        if (relationshipId == 0)
        {
            return BadRequest("Deck is already assigned to this course.");
        }

        return Ok(new { message = "Deck added to course successfully.", relationshipId });
    }

    [Authorize]
    [HttpDelete("{courseId}/decks/{deckId}")]
    public async Task<IActionResult> RemoveDeckFromCourse(int courseId, int deckId)
    {
        int userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        await using var conn = DbConnection.GetConnection();
        await conn.OpenAsync();

        // Verify user owns the course
        var course = await CourseSql.GetCourseByIdAsync(courseId, userId, conn);
        if (course == null)
        {
            return NotFound("Course not found.");
        }

        await CourseDeckSql.RemoveDeckFromCourseAsync(courseId, deckId, conn);
        return Ok(new { message = "Deck removed from course successfully." });
    }

    [Authorize]
    [HttpGet("{courseId}/decks")]
    public async Task<IActionResult> GetCourseDecks(int courseId)
    {
        int userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        await using var conn = DbConnection.GetConnection();
        await conn.OpenAsync();

        // Verify user owns the course
        var course = await CourseSql.GetCourseByIdAsync(courseId, userId, conn);
        if (course == null)
        {
            return NotFound("Course not found.");
        }

        // Get deck IDs for this course
        var deckIds = await CourseDeckSql.GetDeckIdsByCourseIdAsync(courseId, conn);
        
        // Get the actual deck objects
        var decks = new List<Deck>();
        foreach (var deckId in deckIds)
        {
            var deck = await DeckSql.GetDeckByIdAsync(deckId, userId, conn);
            if (deck != null)
            {
                decks.Add(deck);
            }
        }

        return Ok(decks);
    }
}
