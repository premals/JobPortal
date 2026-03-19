namespace IdendityService.Interfaces.Auth
{
    public interface IForgot<secret>UseCase
    {
        Task ExecuteAsync(string email, string resetUrl);
    }
}
