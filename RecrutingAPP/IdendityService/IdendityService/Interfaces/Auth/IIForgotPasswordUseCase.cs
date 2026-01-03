namespace IdendityService.Interfaces.Auth
{
    public interface IForgotPasswordUseCase
    {
        Task ExecuteAsync(string email, string resetUrl);
    }
}
