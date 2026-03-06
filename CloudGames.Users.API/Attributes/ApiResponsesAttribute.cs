using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace Users.API.Attributes
{
    public class ApiResponsesAttribute : TypeFilterAttribute
    {
        public ApiResponsesAttribute() : base(typeof(ApiResponsesFilter))
        {
        }
    }

    public class ApiResponsesFilter : IActionFilter
    {
        public void OnActionExecuting(ActionExecutingContext context)
        {
        }

        public void OnActionExecuted(ActionExecutedContext context)
        {
        }
    }
}
