using static IdendityService.DTOs.ResetPasswordRequestDto;

namespace IdendityService.Interfaces.Auth
{
    public interface IResetPasswordUseCase
    {
        Task ExecuteAsync(ResetPasswordRequest request);
    }
}
