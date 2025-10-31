using Managers.validation;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace SmartWeather.Filters
{
    public class ValidateModelFilter : ActionFilterAttribute
    {
        private readonly ValidationManager _validationManager;

        public ValidateModelFilter(ValidationManager validationManager)
        {
            _validationManager = validationManager;
        }

        override public void OnActionExecuting(ActionExecutingContext context)
        {
            if (!context.ModelState.IsValid)
            {
                var result = _validationManager.FormatErrors(context.ModelState);
                context.Result = new BadRequestObjectResult(result);
            }
        }
    }
}
