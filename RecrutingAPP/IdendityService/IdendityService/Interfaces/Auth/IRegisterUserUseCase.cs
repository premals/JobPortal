using IdendityService.DTOs;
using IdendityService.ResultModel;

namespace IdendityService.Interfaces.Auth
{
    public interface IRegisterUserUseCase
    {
        Task<Result> RegisterAsync(RegisterRequest request, string confirmationBaseUrl);
    }
}
