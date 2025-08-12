using Microsoft.Data.SqlClient;
using Dapper;
using NotesAPI.Models;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using BCrypt.Net;

namespace NotesAPI.Services
{
    public interface IAuthService
    {
        Task<AuthResponse?> LoginAsync(LoginRequest request);
        Task<AuthResponse?> RegisterAsync(RegisterRequest request);
        Task<User?> GetUserByIdAsync(int userId);
    }

    public class AuthService : IAuthService
    {
        private readonly string _connectionString;
        private readonly IConfiguration _configuration;

        public AuthService(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection") ?? throw new ArgumentNullException("Connection string not found");
            _configuration = configuration;
        }

        public async Task<AuthResponse?> LoginAsync(LoginRequest request)
        {
            using var connection = new SqlConnection(_connectionString);
            
            // Find user by email
            const string sql = "SELECT * FROM Users WHERE Email = @Email";
            var user = await connection.QueryFirstOrDefaultAsync<User>(sql, new { Email = request.Email });
            
            if (user == null || !BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash))
            {
                return null; // Invalid credentials
            }

            var token = GenerateJwtToken(user);
            
            // Don't return password hash
            user.PasswordHash = string.Empty;
            
            return new AuthResponse
            {
                Token = token,
                User = user
            };
        }

        public async Task<AuthResponse?> RegisterAsync(RegisterRequest request)
        {
            using var connection = new SqlConnection(_connectionString);
            
            // Check if user already exists
            const string checkSql = "SELECT COUNT(*) FROM Users WHERE Email = @Email";
            var userExists = await connection.QuerySingleAsync<int>(checkSql, new { Email = request.Email });
            
            if (userExists > 0)
            {
                return null; // User already exists
            }

            // Hash password
            var passwordHash = BCrypt.Net.BCrypt.HashPassword(request.Password);
            
            // Create new user
            const string sql = @"
                INSERT INTO Users (Email, PasswordHash, FirstName, LastName, CreatedAt) 
                OUTPUT INSERTED.*
                VALUES (@Email, @PasswordHash, @FirstName, @LastName, @CreatedAt)";
            
            var user = await connection.QuerySingleAsync<User>(sql, new
            {
                Email = request.Email,
                PasswordHash = passwordHash,
                FirstName = request.FirstName,
                LastName = request.LastName,
                CreatedAt = DateTime.UtcNow
            });

            var token = GenerateJwtToken(user);
            
            // Don't return password hash
            user.PasswordHash = string.Empty;
            
            return new AuthResponse
            {
                Token = token,
                User = user
            };
        }

        public async Task<User?> GetUserByIdAsync(int userId)
        {
            using var connection = new SqlConnection(_connectionString);
            const string sql = "SELECT * FROM Users WHERE Id = @Id";
            var user = await connection.QueryFirstOrDefaultAsync<User>(sql, new { Id = userId });
            
            if (user != null)
            {
                user.PasswordHash = string.Empty; // Don't return password hash
            }
            
            return user;
        }

        private string GenerateJwtToken(User user)
        {
            var jwtSettings = _configuration.GetSection("JwtSettings");
            var secretKey = jwtSettings["SecretKey"] ?? "your-super-secret-key-that-is-at-least-32-characters-long";
            var key = Encoding.ASCII.GetBytes(secretKey);

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new[]
                {
                    new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                    new Claim(ClaimTypes.Email, user.Email),
                    new Claim(ClaimTypes.Name, $"{user.FirstName} {user.LastName}")
                }),
                Expires = DateTime.UtcNow.AddDays(7), // Token expires in 7 days
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
            };

            var tokenHandler = new JwtSecurityTokenHandler();
            var token = tokenHandler.CreateToken(tokenDescriptor);
            return tokenHandler.WriteToken(token);
        }
    }
}
