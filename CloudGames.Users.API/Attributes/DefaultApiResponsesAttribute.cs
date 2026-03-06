using Microsoft.AspNetCore.Mvc;

namespace Users.API.Attributes
{
    public class DefaultApiResponsesAttribute : ProducesResponseTypeAttribute
    {
        public DefaultApiResponsesAttribute() : base(StatusCodes.Status500InternalServerError)
        {
        }
    }
}
