namespace IdendityService.ResultModel
{
    public class Result
    {
        public bool Success { get; private set; }
        public string Message { get; private set; }

        public static Result Ok(string message) =>
            new Result { Success = true, Message = message };

        public static Result Fail(string message) =>
            new Result { Success = false, Message = message };
    }
}
