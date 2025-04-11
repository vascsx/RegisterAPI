using RegisterAPI.DTOs;
using RegisterAPI.Models;

namespace RegisterAPI.Interfaces
{
    public interface IUserService
    {
        Task<ServiceResult> RegisterUserAsync(RegisterDTO dto);
        Task<ServiceResult<User>> AuthenticateUserAsync(LoginDTO dto);
        Task<List<User>> GetAllUsersAsync();
        Task<ServiceResult> UpdateUserAsync(int id, RegisterDTO dto);
        Task<ServiceResult> DeleteUserAsync(int id);
    }
}