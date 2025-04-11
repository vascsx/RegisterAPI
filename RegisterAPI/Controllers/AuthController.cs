using Microsoft.AspNetCore.Mvc;
using RegisterAPI.DTOs;
using RegisterAPI.Interfaces;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IUserService _userService;

    public AuthController(IUserService userService)
    {
        _userService = userService;
    }

    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterDTO dto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var result = await _userService.RegisterUserAsync(dto);
        if (!result.Success)
            return BadRequest(new { erros = result.Errors });

        return Ok(new { mensagem = "Usuário cadastrado com sucesso!" });
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginDTO dto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var result = await _userService.AuthenticateUserAsync(dto);
        if (!result.Success)
            return Unauthorized(new { erro = result.Errors.FirstOrDefault() });

        return Ok(new { mensagem = $"Bem-vindo {result.Data.FullName}!" });
    }

    [HttpGet("users")]
    public async Task<IActionResult> GetUsers()
    {
        var users = await _userService.GetAllUsersAsync();
        return Ok(users);
    }

    [HttpPut("edit/{id}")]
    public async Task<IActionResult> EditUser(int id, [FromBody] RegisterDTO dto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var result = await _userService.UpdateUserAsync(id, dto);
        if (!result.Success)
            return BadRequest(new { erros = result.Errors });

        return Ok(new { mensagem = "Usuário atualizado!" });
    }

    [HttpDelete("delete/{id}")]
    public async Task<IActionResult> DeleteUser(int id)
    {
        var result = await _userService.DeleteUserAsync(id);
        if (!result.Success)
            return NotFound(new { erro = result.Errors.FirstOrDefault() });

        return Ok(new { mensagem = "Usuário deletado." });
    }
}