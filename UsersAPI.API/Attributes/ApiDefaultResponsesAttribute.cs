using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Users.API.Attributes
{
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    
    public class ApiDefaultResponsesAttribute : Attribute
    {
    }
}