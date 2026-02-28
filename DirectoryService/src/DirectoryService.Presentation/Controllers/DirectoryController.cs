using Microsoft.AspNetCore.Mvc;

namespace DirectoryService.Presentation.Controllers;

[ApiController]
[Route("[controller]")]
public class DirectoryController : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> Test()
    {
        return Ok();
    }
}