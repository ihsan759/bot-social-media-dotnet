using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace BotSocialMedia.Exceptions
{
    public class ValidationException : HttpException
    {
        public ValidationException(ModelStateDictionary modelState)
            : base("Validation failed", 400, GetErrors(modelState)) { }

        private static object GetErrors(ModelStateDictionary modelState)
        {
            return modelState
                .Where(x => x.Value?.Errors?.Count > 0)
                .ToDictionary(
                    kvp => kvp.Key,
                    kvp => kvp.Value!.Errors.Select(e => e.ErrorMessage).ToArray()
                );
        }
    }
}
