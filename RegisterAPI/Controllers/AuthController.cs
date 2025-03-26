using Microsoft.AspNetCore.Mvc;
using RegisterAPI.DTOs;
using RegisterAPI.Models;
using RegisterAPI.Db;
using Microsoft.EntityFrameworkCore;
using System.Linq;

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
        if (!ModelState.IsValid)
        {
            var errorMessages = ModelState.Values
                                         .SelectMany(v => v.Errors)
                                         .Select(e => e.ErrorMessage)
                                         .ToList();
            return BadRequest(new { errors = errorMessages });
        }

        if (await _context.Users.AnyAsync(u => u.Email == dto.Email))
            return BadRequest("E-mail já cadastrado.");

        var user = new User
        {
            FullName = dto.FullName,
            Email = dto.Email,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.Password)
        };

        _context.Users.Add(user);
        await _context.SaveChangesAsync();

        return Ok("Usuário cadastrado com sucesso!");
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginDTO dto)
    {
        if (string.IsNullOrEmpty(dto.Email))
            return BadRequest("O campo 'E-mail' é obrigatório.");
        if (string.IsNullOrEmpty(dto.Password))
            return BadRequest("O campo 'Senha' é obrigatório.");

        var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == dto.Email);
        if (user == null || !BCrypt.Net.BCrypt.Verify(dto.Password, user.PasswordHash))
            return Unauthorized("Email ou senha inválidos.");

        return Ok($"Bem-vindo {user.FullName}!");
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
        if (user == null) return NotFound("Usuário não encontrado.");

        if (!ModelState.IsValid)
        {
            var errorMessages = ModelState.Values
                                         .SelectMany(v => v.Errors)
                                         .Select(e => e.ErrorMessage)
                                         .ToList();
            return BadRequest(new { errors = errorMessages });
        }

        if (await _context.Users.AnyAsync(u => u.Email == dto.Email))
            return BadRequest("E-mail já cadastrado.");

        user.FullName = dto.FullName;
        user.Email = dto.Email;
        user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.Password);

        await _context.SaveChangesAsync();
        return Ok("Usuário atualizado!");
    }

    [HttpDelete("delete/{id}")]
    public async Task<IActionResult> DeleteUser(int id)
    {
        var user = await _context.Users.FindAsync(id);
        if (user == null) return NotFound("Usuário não encontrado.");

        _context.Users.Remove(user);
        await _context.SaveChangesAsync();
        return Ok("Usuário deletado.");
    }
}
