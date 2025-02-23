using DataBalkTaskLisit.Entities;
using DataBalkTaskLisitAPI.Dtos;

namespace DataBalkTaskLisitAPI.Services
{
    public interface IAuthService
    {
        Task<User?> RegisterAsync(UserDto request);
        Task<TokenResponseDto?> LoginAsync(UserDto request);
        Task<TokenResponseDto?> RefreshTokensAsync(RefreshTokenRequestDto request);

    }
}
