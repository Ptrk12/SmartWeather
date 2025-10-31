using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace Managers.validation
{
    public class ValidationManager
    {
        public Dictionary<string, string[]>? FormatErrors(ModelStateDictionary modelState)
        {
            var result = modelState.Where(x => x.Value.Errors.Count > 0)
                .ToDictionary(kvp => kvp.Key, kvp => kvp.Value.Errors.Select(x => x.ErrorMessage).ToArray());
            return result;
        }
    }
}
