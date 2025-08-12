using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using NotesAPI.Models;
using NotesAPI.Services;
using System.Security.Claims;

namespace NotesAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize] // Require authentication for all actions
    public class NotesController : ControllerBase
    {
        private readonly INoteService _noteService;

        public NotesController(INoteService noteService)
        {
            _noteService = noteService;
        }

        // Helper method to get current user ID from JWT token
        private int GetCurrentUserId()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            return int.Parse(userIdClaim ?? "0");
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<Note>>> GetNotes([FromQuery] string? search = null)
        {
            try
            {
                var userId = GetCurrentUserId();
                var notes = string.IsNullOrEmpty(search) 
                    ? await _noteService.GetAllNotesAsync(userId)
                    : await _noteService.SearchNotesAsync(search, userId);
                
                return Ok(notes);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<Note>> GetNote(int id)
        {
            try
            {
                var userId = GetCurrentUserId();
                var note = await _noteService.GetNoteByIdAsync(id, userId);
                if (note == null)
                {
                    return NotFound($"Note with ID {id} not found.");
                }
                return Ok(note);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        [HttpPost]
        public async Task<ActionResult<Note>> CreateNote([FromBody] CreateNoteRequest request)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(request.Title))
                {
                    return BadRequest("Title is required.");
                }

                var userId = GetCurrentUserId();
                var note = await _noteService.CreateNoteAsync(request, userId);
                return CreatedAtAction(nameof(GetNote), new { id = note.Id }, note);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        [HttpPut("{id}")]
        public async Task<ActionResult<Note>> UpdateNote(int id, [FromBody] UpdateNoteRequest request)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(request.Title))
                {
                    return BadRequest("Title is required.");
                }

                var userId = GetCurrentUserId();
                var updatedNote = await _noteService.UpdateNoteAsync(id, request, userId);
                if (updatedNote == null)
                {
                    return NotFound($"Note with ID {id} not found.");
                }
                
                return Ok(updatedNote);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        [HttpDelete("{id}")]
        public async Task<ActionResult> DeleteNote(int id)
        {
            try
            {
                var userId = GetCurrentUserId();
                var deleted = await _noteService.DeleteNoteAsync(id, userId);
                if (!deleted)
                {
                    return NotFound($"Note with ID {id} not found.");
                }
                
                return NoContent();
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }
    }
}
