using static IdendityService.DTOs.Reset<secret>RequestDto;

namespace IdendityService.Interfaces.Auth
{
    public interface IReset<secret>UseCase
    {
        Task ExecuteAsync(Reset<secret>Request request);
    }
}
