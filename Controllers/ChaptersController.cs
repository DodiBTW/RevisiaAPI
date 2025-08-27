using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RevisiaAPI.Db;
using RevisiaAPI.Models;
using System.Security.Claims;

[ApiController]
[Route("api/courses/{courseId}/[controller]")]
public class ChaptersController : ControllerBase
{
    [Authorize]
    [HttpGet]
    public async Task<IActionResult> GetChapters(int courseId)
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
        
        var chapters = await ChapterSql.GetChaptersByCourseIdAsync(courseId, conn);
        return Ok(chapters);
    }

    [Authorize]
    [HttpGet("{chapterId}")]
    public async Task<IActionResult> GetChapter(int courseId, int chapterId)
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
        
        var chapter = await ChapterSql.GetChapterByIdAsync(chapterId, conn);
        if (chapter == null || chapter.CourseId != courseId)
        {
            return NotFound("Chapter not found.");
        }
        
        return Ok(chapter);
    }

    [Authorize]
    [HttpPost]
    public async Task<IActionResult> CreateChapter(int courseId, [FromBody] Chapter chapter)
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

        // Validate notes length
        if (!string.IsNullOrEmpty(chapter.Notes) && chapter.Notes.Length > 10000)
        {
            return BadRequest("Notes cannot exceed 10,000 characters.");
        }

        chapter.CourseId = courseId;
        chapter.CreatedAt = DateTime.UtcNow;
        chapter.UpdatedAt = DateTime.UtcNow;
        chapter.IsActive = true;
        
        // Auto-assign order index if not provided
        if (chapter.OrderIndex <= 0)
        {
            chapter.OrderIndex = await ChapterSql.GetNextOrderIndexAsync(courseId, conn);
        }
        
        var id = await ChapterSql.CreateChapterAsync(chapter, conn);
        chapter.Id = id;
        
        return Ok(chapter);
    }

    [Authorize]
    [HttpPatch("{chapterId}")]
    public async Task<IActionResult> UpdateChapter(int courseId, int chapterId, [FromBody] Chapter updatedChapter)
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

        var originalChapter = await ChapterSql.GetChapterByIdAsync(chapterId, conn);
        if (originalChapter == null || originalChapter.CourseId != courseId)
        {
            return NotFound("Chapter not found.");
        }

        // Validate notes length
        if (!string.IsNullOrEmpty(updatedChapter.Notes) && updatedChapter.Notes.Length > 10000)
        {
            return BadRequest("Notes cannot exceed 10,000 characters.");
        }

        // Update fields
        updatedChapter.Id = chapterId;
        updatedChapter.CourseId = courseId;
        updatedChapter.CreatedAt = originalChapter.CreatedAt;
        updatedChapter.UpdatedAt = DateTime.UtcNow;

        await ChapterSql.UpdateChapterAsync(updatedChapter, conn);
        return Ok(updatedChapter);
    }

    [Authorize]
    [HttpDelete("{chapterId}")]
    public async Task<IActionResult> DeleteChapter(int courseId, int chapterId)
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

        var chapter = await ChapterSql.GetChapterByIdAsync(chapterId, conn);
        if (chapter == null || chapter.CourseId != courseId)
        {
            return NotFound("Chapter not found.");
        }

        await ChapterSql.DeleteChapterAsync(chapterId, conn);
        return Ok(new { message = "Chapter deleted successfully." });
    }
}
