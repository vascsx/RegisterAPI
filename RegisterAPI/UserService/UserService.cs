using Microsoft.EntityFrameworkCore;
using RegisterAPI.Db;
using RegisterAPI.DTOs;
using RegisterAPI.Models;
using System.Text.RegularExpressions;

namespace RegisterAPI.Services
{
    public interface IUserService
    {
        Task<ServiceResult> RegisterUserAsync(RegisterDTO dto);
        Task<ServiceResult<User>> AuthenticateUserAsync(LoginDTO dto);
        Task<List<User>> GetAllUsersAsync();
        Task<ServiceResult> UpdateUserAsync(int id, RegisterDTO dto);
        Task<ServiceResult> DeleteUserAsync(int id);
    }

    public class UserService : IUserService
    {
        private readonly AppDbContext _context;

        public UserService(AppDbContext context)
        {
            _context = context;
        }

        public async Task<ServiceResult> RegisterUserAsync(RegisterDTO dto)
        {
            var errors = ValidateUser(dto);
            if (errors.Any())
                return ServiceResult.Failure(errors);

            var user = CreateUser(dto);
            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            return ServiceResult.Success();
        }

        public async Task<ServiceResult<User>> AuthenticateUserAsync(LoginDTO dto)
        {
            if (string.IsNullOrWhiteSpace(dto.Email) || string.IsNullOrWhiteSpace(dto.Password))
                return ServiceResult<User>.Failure("E-mail e senha são obrigatórios.");

            var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == dto.Email);
            if (user == null || !BCrypt.Net.BCrypt.Verify(dto.Password, user.PasswordHash))
                return ServiceResult<User>.Failure("E-mail ou senha inválidos.");

            return ServiceResult<User>.Success(user);
        }

        public async Task<List<User>> GetAllUsersAsync()
        {
            return await _context.Users.ToListAsync();
        }

        public async Task<ServiceResult> UpdateUserAsync(int id, RegisterDTO dto)
        {
            var user = await _context.Users.FindAsync(id);
            if (user == null)
                return ServiceResult.Failure("Usuário não encontrado.");

            var errors = ValidateUser(dto, id);
            if (errors.Any())
                return ServiceResult.Failure(errors);

            user.FullName = dto.FullName.Trim();
            user.Email = dto.Email.Trim().ToLowerInvariant();
            if (!string.IsNullOrWhiteSpace(dto.Password))
                user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.Password);
            user.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            return ServiceResult.Success();
        }

        public async Task<ServiceResult> DeleteUserAsync(int id)
        {
            var user = await _context.Users.FindAsync(id);
            if (user == null)
                return ServiceResult.Failure("Usuário não encontrado.");

            _context.Users.Remove(user);
            await _context.SaveChangesAsync();

            return ServiceResult.Success();
        }

        private List<string> ValidateUser(RegisterDTO dto, int? id = null)
        {
            var errors = new List<string>();

            if (string.IsNullOrWhiteSpace(dto.FullName))
                errors.Add("O campo 'FullName' é obrigatório.");
            else if (!Regex.IsMatch(dto.FullName, @"^[A-Za-zÀ-ÖØ-öø-ÿ\s]+$"))
                errors.Add("O campo 'FullName' deve conter apenas letras e espaços, sem números.");

            if (string.IsNullOrWhiteSpace(dto.Email))
                errors.Add("O campo 'Email' é obrigatório.");
            else if (!Regex.IsMatch(dto.Email, @"^[^@\s]+@[^@\s]+\.[^@\s]+$"))
                errors.Add("O campo 'Email' não é válido.");
            else if (_context.Users.Any(u => u.Email == dto.Email && u.Id != id))
                errors.Add("E-mail já cadastrado.");

            if (string.IsNullOrWhiteSpace(dto.Password))
                errors.Add("O campo 'Password' é obrigatório.");
            else if (dto.Password.Length < 6)
                errors.Add("A senha deve ter no mínimo 6 caracteres.");

            return errors;
        }

        private User CreateUser(RegisterDTO dto)
        {
            return new User
            {
                FullName = dto.FullName.Trim(),
                Email = dto.Email.Trim().ToLowerInvariant(),
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.Password),
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };
        }
    }

    public class ServiceResult
    {
        public bool Success { get; private set; }
        public List<string> Errors { get; private set; }

        public static ServiceResult Success() => new ServiceResult { Success = true };
        public static ServiceResult Failure(params string[] errors) => new ServiceResult { Success = false, Errors = errors.ToList() };
    }

    public class ServiceResult<T> : ServiceResult
    {
        public T User { get; private set; }

        public static ServiceResult<T> Success(T user) => new ServiceResult<T> { Success = true, User = user };
        public static new ServiceResult<T> Failure(params string[] errors) => new ServiceResult<T> { Success = false, Errors = errors.ToList() };
    }
}