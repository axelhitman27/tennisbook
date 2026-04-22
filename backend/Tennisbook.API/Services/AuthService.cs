using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Tennisbook.API.Data;
using Tennisbook.API.DTOs;
using Tennisbook.API.Models;

namespace Tennisbook.API.Services;

public class AuthService
{
    private readonly TennisbookDbContext _db;
    private readonly IConfiguration _config;

    public AuthService(TennisbookDbContext db, IConfiguration config)
    {
        _db = db;
        _config = config;
    }

    public async Task<AuthResponseDto?> LoginAsync(LoginDto dto)
    {
        var user = await _db.Users.FirstOrDefaultAsync(u => u.Email == dto.Email);
        if (user == null || !VerifyPassword(dto.Password, user.PasswordHash))
            return null;

        var token = GenerateJwt(user);
        return new AuthResponseDto(token, user.Role, user.Id, user.PlayerId, user.Email);
    }

    public async Task<AuthResponseDto?> RegisterAsync(RegisterDto dto)
    {
        if (await _db.Users.AnyAsync(u => u.Email == dto.Email))
            return null;

        var player = new Player
        {
            FirstName = dto.FirstName,
            LastName = dto.LastName,
            Email = dto.Email,
            Phone = dto.Phone,
            DateOfBirth = dto.DateOfBirth,
            Level = dto.Level,
            QrCode = Guid.NewGuid().ToString()
        };
        _db.Players.Add(player);
        await _db.SaveChangesAsync();

        var user = new User
        {
            Email = dto.Email,
            PasswordHash = HashPassword(dto.Password),
            Role = "Player",
            PlayerId = player.Id
        };
        _db.Users.Add(user);
        await _db.SaveChangesAsync();

        var token = GenerateJwt(user);
        return new AuthResponseDto(token, user.Role, user.Id, user.PlayerId, user.Email);
    }

    public async Task SeedAdminAsync()
    {
        if (await _db.Users.AnyAsync(u => u.Role == "Admin"))
            return;

        var admin = new User
        {
            Email = "admin@tennisbook.com",
            PasswordHash = HashPassword("Admin123!"),
            Role = "Admin"
        };
        _db.Users.Add(admin);
        await _db.SaveChangesAsync();
    }

    private string GenerateJwt(User user)
    {
        var key = new SymmetricSecurityKey(
            Encoding.UTF8.GetBytes(_config["Jwt:Key"] ?? "TennisbookSuperSecretKey123456789!"));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new(ClaimTypes.Email, user.Email),
            new(ClaimTypes.Role, user.Role),
        };
        if (user.PlayerId.HasValue)
            claims.Add(new Claim("PlayerId", user.PlayerId.Value.ToString()));

        var token = new JwtSecurityToken(
            issuer: _config["Jwt:Issuer"] ?? "Tennisbook",
            audience: _config["Jwt:Audience"] ?? "Tennisbook",
            claims: claims,
            expires: DateTime.UtcNow.AddDays(7),
            signingCredentials: creds
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    private static string HashPassword(string password)
    {
        var salt = RandomNumberGenerator.GetBytes(16);
        var hash = Rfc2898DeriveBytes.Pbkdf2(password, salt, 100000, HashAlgorithmName.SHA256, 32);
        return $"{Convert.ToBase64String(salt)}.{Convert.ToBase64String(hash)}";
    }

    private static bool VerifyPassword(string password, string storedHash)
    {
        var parts = storedHash.Split('.');
        if (parts.Length != 2) return false;

        var salt = Convert.FromBase64String(parts[0]);
        var hash = Convert.FromBase64String(parts[1]);
        var computedHash = Rfc2898DeriveBytes.Pbkdf2(password, salt, 100000, HashAlgorithmName.SHA256, 32);
        return CryptographicOperations.FixedTimeEquals(hash, computedHash);
    }
}
