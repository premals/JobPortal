using IdendityService.DTOs;
using static IdendityService.DTOs.AuthResponseDto;

namespace IdendityService.Interfaces.Auth
{
    public interface ILoginUseCase
    {
        Task<AuthResponse> LoginAsync(LoginRequest request, string ipAddress);
    }
}
