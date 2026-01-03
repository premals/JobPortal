namespace IdendityService.Interfaces.Auth
{
    public interface IConfirmEmailUseCase
    {
        Task ConfirmAsync(string userId, string token);
    }
}
