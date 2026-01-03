using static IdendityService.DTOs.AuthResponseDto;

namespace IdendityService.Interfaces.Auth
{
    public interface IRefreshTokenUseCase
    {
        Task<AuthResponse> RefreshAsync(string refreshToken, string ipAddress);
    }
}
