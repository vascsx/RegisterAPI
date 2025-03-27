using Microsoft.AspNetCore.Mvc;
using RegisterAPI.DTOs;
using RegisterAPI.Models;
using RegisterAPI.Db;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Http.HttpResults;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly AppDbContext _context;

    public AuthController(AppDbContext context)
    {
        _context = context;
    }

    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterDTO dto)
    {
        if (string.IsNullOrWhiteSpace(dto.FullName) && string.IsNullOrWhiteSpace(dto.Email) && string.IsNullOrWhiteSpace(dto.Password))
            return BadRequest(new { erro = "Nenhum campo foi preenchido!." });

        if (string.IsNullOrWhiteSpace(dto.FullName))
            return BadRequest(new { erro = "O campo 'FullName' é obrigatório." });

        if (string.IsNullOrWhiteSpace(dto.Email))
            return BadRequest(new { erro = "O campo 'Email' é obrigatório." });

        if (string.IsNullOrWhiteSpace(dto.Password))
            return BadRequest(new { erro = "O campo 'Password' é obrigatório." });

        if (dto.Password.Length < 6)
            return BadRequest(new { erro = "A senha deve ter no mínimo 6 caracteres." });

        if (await _context.Users.AnyAsync(u => u.Email == dto.Email))
            return BadRequest(new { erro = "E-mail já cadastrado." });

        var user = new User
        {
            FullName = dto.FullName,
            Email = dto.Email,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.Password),
            CreatedAt = DateTime.Now
        };

        _context.Users.Add(user);
        await _context.SaveChangesAsync();

        return Ok(new { mensagem = "Usuário cadastrado com sucesso!" });
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginDTO dto)
    {
        if (string.IsNullOrWhiteSpace(dto.Email))
            return BadRequest(new { erro = "O campo 'Email' é obrigatório." });

        if (string.IsNullOrWhiteSpace(dto.Password))
            return BadRequest(new { erro = "O campo 'Password' é obrigatório." });

        var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == dto.Email);
        if (user == null || !BCrypt.Net.BCrypt.Verify(dto.Password, user.PasswordHash))
            return Unauthorized(new { erro = "Email ou senha inválidos." });

        return Ok(new { mensagem = $"Bem-vindo {user.FullName}!" });
    }

    [HttpGet("users")]
    public async Task<IActionResult> GetUsers()
    {
        var users = await _context.Users.ToListAsync();
        return Ok(users);
    }

    [HttpPut("edit/{id}")]
    public async Task<IActionResult> EditUser(int id, [FromBody] RegisterDTO dto)
    {
        var user = await _context.Users.FindAsync(id);
        if (user == null) return NotFound(new { erro = "Usuário não encontrado." });

        // Validações
        if (string.IsNullOrWhiteSpace(dto.FullName))
            return BadRequest(new { erro = "O campo 'FullName' é obrigatório." });

        if (string.IsNullOrWhiteSpace(dto.Email))
            return BadRequest(new { erro = "O campo 'Email' é obrigatório." });

        if (string.IsNullOrWhiteSpace(dto.Password))
            return BadRequest(new { erro = "O campo 'Password' é obrigatório." });

        if (dto.Password.Length < 6)
            return BadRequest(new { erro = "A senha deve ter no mínimo 6 caracteres." });

        // Verifica se o novo e-mail já existe e não é o do próprio usuário
        if (await _context.Users.AnyAsync(u => u.Email == dto.Email && u.Id != id))
            return BadRequest(new { erro = "E-mail já cadastrado." });

        user.FullName = dto.FullName;
        user.Email = dto.Email;
        user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.Password);

        await _context.SaveChangesAsync();
        return Ok(new { mensagem = "Usuário atualizado!" });
    }

    [HttpDelete("delete/{id}")]
    public async Task<IActionResult> DeleteUser(int id)
    {
        var user = await _context.Users.FindAsync(id);
        if (user == null) return NotFound(new { erro = "Usuário não encontrado." });

        _context.Users.Remove(user);
        await _context.SaveChangesAsync();
        return Ok(new { mensagem = "Usuário deletado." });
    }
}
