using Microsoft.Data.SqlClient;
using Dapper;
using NotesAPI.Models;

namespace NotesAPI.Services
{
    public interface INoteService
    {
        Task<IEnumerable<Note>> GetAllNotesAsync(int userId);
        Task<Note?> GetNoteByIdAsync(int id, int userId);
        Task<Note> CreateNoteAsync(CreateNoteRequest request, int userId);
        Task<Note?> UpdateNoteAsync(int id, UpdateNoteRequest request, int userId);
        Task<bool> DeleteNoteAsync(int id, int userId);
        Task<IEnumerable<Note>> SearchNotesAsync(string searchTerm, int userId);
    }

    public class NoteService : INoteService
    {
        private readonly string _connectionString;

        public NoteService(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection") ?? throw new ArgumentNullException("Connection string not found");
        }

        public async Task<IEnumerable<Note>> GetAllNotesAsync(int userId)
        {
            using var connection = new SqlConnection(_connectionString);
            const string sql = "SELECT * FROM Notes WHERE UserId = @UserId ORDER BY UpdatedAt DESC";
            return await connection.QueryAsync<Note>(sql, new { UserId = userId });
        }

        public async Task<Note?> GetNoteByIdAsync(int id, int userId)
        {
            using var connection = new SqlConnection(_connectionString);
            const string sql = "SELECT * FROM Notes WHERE Id = @Id AND UserId = @UserId";
            return await connection.QueryFirstOrDefaultAsync<Note>(sql, new { Id = id, UserId = userId });
        }

        public async Task<Note> CreateNoteAsync(CreateNoteRequest request, int userId)
        {
            using var connection = new SqlConnection(_connectionString);
            const string sql = @"
                INSERT INTO Notes (Title, Content, CreatedAt, UpdatedAt, UserId) 
                OUTPUT INSERTED.*
                VALUES (@Title, @Content, @CreatedAt, @UpdatedAt, @UserId)";
            
            var now = DateTime.UtcNow;
            var note = await connection.QuerySingleAsync<Note>(sql, new
            {
                Title = request.Title,
                Content = request.Content,
                CreatedAt = now,
                UpdatedAt = now,
                UserId = userId
            });
            
            return note;
        }

        public async Task<Note?> UpdateNoteAsync(int id, UpdateNoteRequest request, int userId)
        {
            using var connection = new SqlConnection(_connectionString);
            const string sql = @"
                UPDATE Notes 
                SET Title = @Title, Content = @Content, UpdatedAt = @UpdatedAt 
                OUTPUT INSERTED.*
                WHERE Id = @Id AND UserId = @UserId";
            
            var updatedNote = await connection.QueryFirstOrDefaultAsync<Note>(sql, new
            {
                Id = id,
                Title = request.Title,
                Content = request.Content,
                UpdatedAt = DateTime.UtcNow,
                UserId = userId
            });
            
            return updatedNote;
        }

        public async Task<bool> DeleteNoteAsync(int id, int userId)
        {
            using var connection = new SqlConnection(_connectionString);
            const string sql = "DELETE FROM Notes WHERE Id = @Id AND UserId = @UserId";
            var rowsAffected = await connection.ExecuteAsync(sql, new { Id = id, UserId = userId });
            return rowsAffected > 0;
        }

        public async Task<IEnumerable<Note>> SearchNotesAsync(string searchTerm, int userId)
        {
            using var connection = new SqlConnection(_connectionString);
            const string sql = @"
                SELECT * FROM Notes 
                WHERE UserId = @UserId AND (Title LIKE @SearchTerm OR Content LIKE @SearchTerm)
                ORDER BY UpdatedAt DESC";
            
            return await connection.QueryAsync<Note>(sql, new { SearchTerm = $"%{searchTerm}%", UserId = userId });
        }
    }
}
