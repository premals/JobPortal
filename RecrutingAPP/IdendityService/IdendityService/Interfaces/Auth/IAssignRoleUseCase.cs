namespace IdendityService.Interfaces.Auth
{
    public interface IAssignRoleUseCase
    {
        Task AssignAsync(string userId, string role);
    }
}
