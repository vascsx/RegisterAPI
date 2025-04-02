using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RegisterAPI.DTOs;
using RegisterAPI.Models;
using RegisterAPI.Db;
using System.Text.RegularExpressions;

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
        var erros = ValidarUsuario(dto);
        if (erros.Any()) return BadRequest(new { erros });

        var user = CriarUsuario(dto);
        _context.Users.Add(user);
        await _context.SaveChangesAsync();

        return Ok(new { mensagem = "Usuário cadastrado com sucesso!" });
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginDTO dto)
    {
        if (string.IsNullOrWhiteSpace(dto.Email) || string.IsNullOrWhiteSpace(dto.Password))
            return BadRequest(new { erro = "E-mail e senha são obrigatórios." });

        var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == dto.Email);
        if (user == null || !BCrypt.Net.BCrypt.Verify(dto.Password, user.PasswordHash))
            return Unauthorized(new { erro = "E-mail ou senha inválidos." });

        return Ok(new { mensagem = $"Bem-vindo {user.FullName}!" });
    }

    [HttpGet("users")]
    public async Task<IActionResult> GetUsers()
    {
        return Ok(await _context.Users.ToListAsync());
    }

    [HttpPut("edit/{id}")]
    public async Task<IActionResult> EditUser(int id, [FromBody] RegisterDTO dto)
    {
        var user = await _context.Users.FindAsync(id);
        if (user == null) return NotFound(new { erro = "Usuário não encontrado." });

        var erros = ValidarUsuario(dto, id);
        if (erros.Any()) return BadRequest(new { erros });

        user.FullName = dto.FullName;
        user.Email = dto.Email;
        user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.Password);
        user.UpdatedAt = DateTime.Now;

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

    private List<string> ValidarUsuario(RegisterDTO dto, int? id = null)
    {
        var erros = new List<string>();

        if (string.IsNullOrWhiteSpace(dto.FullName))
            erros.Add("O campo 'FullName' é obrigatório.");
        else if (!Regex.IsMatch(dto.FullName, @"^[A-Za-zÀ-ÖØ-öø-ÿ\s]+$"))
            erros.Add("O campo 'FullName' deve conter apenas letras e espaços, sem números.");

        if (string.IsNullOrWhiteSpace(dto.Email))
            erros.Add("O campo 'Email' é obrigatório.");
        else if (!Regex.IsMatch(dto.Email, @"^[^@\s]+@[^@\s]+\.[^@\s]+$"))
            erros.Add("O campo 'Email' não é válido.");
        else if (_context.Users.Any(u => u.Email == dto.Email && u.Id != id))
            erros.Add("E-mail já cadastrado.");

        if (string.IsNullOrWhiteSpace(dto.Password))
            erros.Add("O campo 'Password' é obrigatório.");
        else if (dto.Password.Length < 6)
            erros.Add("A senha deve ter no mínimo 6 caracteres.");

        return erros;
    }

    private User CriarUsuario(RegisterDTO dto)
    {
        return new User
        {
            FullName = dto.FullName,
            Email = dto.Email,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.Password),
            CreatedAt = DateTime.Now,
            UpdatedAt = DateTime.Now
        };
    }
}
